//BSD 2 - Clause License

//Copyright(c) 2022, K. S. Ernest (iFire) Lee
//Copyright(c) 2021, PacosLelouch
//All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:

//1.Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.

//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
//FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
//CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
//OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.LinearAlgebra.Solvers;
using UnityEditor;
using UnityEngine;

//[ExecuteInEditMode]
public class DirectDeltaMushSkinnedMesh : MonoBehaviour
{
    public int iterations = 2;

    public float translationSmooth = 0.9f;

    public float rotationSmooth = 0.9f;

    public float dm_blend = 0.0f;

    public bool deformNormals = true;

    public bool weightedSmooth = true;

    public bool useCompute = true;

    public float adjacencyMatchingVertexTolerance = 1e-4f;

    public enum DebugMode
    {
        Off,
        CompareWithSkinning
    }

    public DebugMode debugMode = DebugMode.Off;

    bool disableDeltaPass
    {
        get
        {
            return false;
        }
    }

    bool actuallyUseCompute
    {
        get
        {
            return useCompute && debugMode != DebugMode.CompareWithSkinning;
        }
    }

    internal Mesh mesh;

    internal Mesh meshForCPUOutput;

    internal SkinnedMeshRenderer skin;

    struct DeformedMesh
    {
        public DeformedMesh(int vertexCount_)
        {
            vertexCount = vertexCount_;
            vertices = new Vector3[vertexCount];
            normals = new Vector3[vertexCount];
            deltaV = new Vector3[vertexCount];
            deltaN = new Vector3[vertexCount];
        }

        public int vertexCount;

        public Vector3[] vertices;

        public Vector3[] normals;

        public Vector3[] deltaV;

        public Vector3[] deltaN;
    }

    DeformedMesh deformedMesh;

    internal int[,] adjacencyMatrix;

    internal Vector3[] deltaV;

    internal Vector3[] deltaN;

    internal int deltaIterations = -1;

    internal Func<Vector3[], int[,], Vector3[]> smoothFilter;

    internal DenseMatrix[,] omegas;

    internal DDMUtilsIterative ddmUtils;

    // Compute
    //[HideInInspector]
    public Shader ductTapedShader;

    //[HideInInspector]
    public ComputeShader computeShader;

    private int deformKernel;

    private int computeThreadGroupSizeX;

    internal ComputeBuffer verticesCB; // float3

    internal ComputeBuffer normalsCB; // float3

    internal ComputeBuffer weightsCB; // float4 + int4

    internal ComputeBuffer bonesCB; // float4x4

    internal ComputeBuffer omegasCB; // float4x4 * 4

    internal ComputeBuffer outputCB; // float3 + float3

    internal Material ductTapedMaterial;

    internal const int maxOmegaCount = 16;

    void Start()
    {
        if (computeShader)
        {
            computeShader = Instantiate(computeShader);
        }
        skin = GetComponent<SkinnedMeshRenderer>();
        mesh = skin.sharedMesh;
        meshForCPUOutput = Instantiate(mesh);

        deformedMesh = new DeformedMesh(mesh.vertexCount);

        adjacencyMatrix =
            GetCachedAdjacencyMatrix(mesh, adjacencyMatchingVertexTolerance);

        // Store matrix to Math.NET matrix.
        int vCount = mesh.vertexCount;
        int bCount = skin.bones.Length;
        SparseMatrix lapl =
            MeshUtils
                .BuildLaplacianMatrixFromAdjacentMatrix(vCount,
                adjacencyMatrix,
                true,
                weightedSmooth);

        DenseMatrix V = new DenseMatrix(vCount, 3);
        Vector3[] vs = mesh.vertices;
        for (int i = 0; i < vCount; ++i)
        {
            Vector3 v = vs[i];
            V[i, 0] = v.x;
            V[i, 1] = v.y;
            V[i, 2] = v.z;
        }

        DenseMatrix W = new DenseMatrix(vCount, bCount);

        BoneWeight[] bws = mesh.boneWeights;
        for (int i = 0; i < vCount; ++i)
        {
            BoneWeight w = bws[i];
            if (w.boneIndex0 >= 0 && w.weight0 > 0.0f)
            {
                W[i, w.boneIndex0] = w.weight0;
            }
            if (w.boneIndex1 >= 0 && w.weight1 > 0.0f)
            {
                W[i, w.boneIndex1] = w.weight1;
            }
            if (w.boneIndex2 >= 0 && w.weight2 > 0.0f)
            {
                W[i, w.boneIndex2] = w.weight2;
            }
            if (w.boneIndex3 >= 0 && w.weight3 > 0.0f)
            {
                W[i, w.boneIndex3] = w.weight3;
            }
        }

        SparseMatrix I = SparseMatrix.CreateIdentity(vCount);
        ddmUtils = new DDMUtilsIterative();
        ddmUtils.dm_blend = dm_blend;
        ddmUtils.n = vCount;
        ddmUtils.num_transforms = bCount;

        ddmUtils.B = iterations == 0 ? I : I - (translationSmooth) * lapl; //B;
        ddmUtils.C = iterations == 0 ? I : I - (rotationSmooth) * lapl; //C;
        ddmUtils.V = V;
        ddmUtils.W = W;

        ddmUtils.translationSmooth = translationSmooth;
        ddmUtils.rotationSmooth = rotationSmooth;

        ddmUtils.InitCache();

        omegas = ddmUtils.ComputeOmegas(vCount, bCount, iterations);

        //TODO: Precompute others.
        // Compute
        if (
            SystemInfo.supportsComputeShaders &&
            computeShader &&
            ductTapedShader
        )
        {
            DDMUtilsIterative.OmegaWithIndex[,] convertedOmegas =
                DDMUtilsIterative.ConvertOmegas1D(omegas, maxOmegaCount);

            verticesCB = new ComputeBuffer(vCount, 3 * sizeof(float));
            normalsCB = new ComputeBuffer(vCount, 3 * sizeof(float));
            weightsCB =
                new ComputeBuffer(vCount, 4 * sizeof(float) + 4 * sizeof(int));
            bonesCB = new ComputeBuffer(bCount, 16 * sizeof(float));
            verticesCB.SetData(mesh.vertices);
            normalsCB.SetData(mesh.normals);
            weightsCB.SetData (bws);

            omegasCB =
                new ComputeBuffer(vCount * maxOmegaCount,
                    (10 * sizeof(float) + sizeof(int)));
            omegasCB.SetData (convertedOmegas);

            outputCB = new ComputeBuffer(vCount, 6 * sizeof(float));

            deformKernel = computeShader.FindKernel("DeformMesh");
            computeShader.SetBuffer(deformKernel, "Vertices", verticesCB);
            computeShader.SetBuffer(deformKernel, "Normals", normalsCB);
            computeShader.SetBuffer(deformKernel, "Weights", weightsCB);
            computeShader.SetBuffer(deformKernel, "Bones", bonesCB);
            computeShader.SetBuffer(deformKernel, "Omegas", omegasCB);
            computeShader.SetBuffer(deformKernel, "Output", outputCB);
            computeShader.SetInt("VertexCount", vCount);

            uint
                threadGroupSizeX,
                threadGroupSizeY,
                threadGroupSizeZ;
            computeShader
                .GetKernelThreadGroupSizes(deformKernel,
                out threadGroupSizeX,
                out threadGroupSizeY,
                out threadGroupSizeZ);
            computeThreadGroupSizeX = (int) threadGroupSizeX;

            ductTapedMaterial = new Material(ductTapedShader);
            ductTapedMaterial.CopyPropertiesFromMaterial(skin.sharedMaterial);
        }
        else
        {
            useCompute = false;
        }
    }

    void OnDestroy()
    {
        if (verticesCB == null)
        {
            return;
        }

        verticesCB.Release();
        normalsCB.Release();
        weightsCB.Release();
        bonesCB.Release();
        omegasCB.Release();
        outputCB.Release();
    }

    void LateUpdate()
    {
        bool compareWithSkinning = debugMode == DebugMode.CompareWithSkinning;

        if (actuallyUseCompute)
        {
            UpdateMeshOnGPU();
        }

        if (compareWithSkinning)
        {
            DrawVerticesVsSkin();
        }
        else
        {
            DrawMesh();
        }

        skin.enabled = compareWithSkinning;
    }


    [System.Serializable]
    public struct AdjacencyMatrix
    {
        public int

                w,
                h;

        public int[] storage;

        public AdjacencyMatrix(int[,] src)
        {
            w = src.GetLength(0);
            h = src.GetLength(1);
            storage = new int[w * h];
            Buffer.BlockCopy(src, 0, storage, 0, storage.Length * sizeof(int));
        }

        public int[,] data
        {
            get
            {
                var retVal = new int[w, h];
                Buffer
                    .BlockCopy(storage,
                    0,
                    retVal,
                    0,
                    storage.Length * sizeof(int));
                return retVal;
            }
        }
    }

    private static int[,]
    GetCachedAdjacencyMatrix(Mesh mesh, float adjacencyMatchingVertexTolerance)
    {
        int[,] adjacencyMatrix;

        adjacencyMatrix =
            MeshUtils
                .BuildAdjacencyMatrix(mesh.vertices,
                mesh.triangles,
                16,
                adjacencyMatchingVertexTolerance *
                adjacencyMatchingVertexTolerance);

        return adjacencyMatrix;
    }

    void UpdateMeshOnGPU()
    {
        int threadGroupsX =
            (mesh.vertices.Length + computeThreadGroupSizeX - 1) /
            computeThreadGroupSizeX;

        Matrix4x4[] boneMatrices = GenerateBoneMatrices();

        bonesCB.SetData (boneMatrices);
        computeShader.SetBuffer(deformKernel, "Bones", bonesCB);

        computeShader.Dispatch(deformKernel, threadGroupsX, 1, 1);
        ductTapedMaterial.SetBuffer("Vertices", outputCB);
    }

    Matrix4x4[] GenerateBoneMatrices()
    {
        Matrix4x4[] boneMatrices = new Matrix4x4[skin.bones.Length];

        for (int i = 0; i < boneMatrices.Length; i++)
        {
            Matrix4x4 localToWorld = skin.bones[i].localToWorldMatrix;
            Matrix4x4 bindPose = mesh.bindposes[i];
            boneMatrices[i] = localToWorld * bindPose;
        }
        return boneMatrices;
    }



#region Helpers
    void DrawMesh()
    {
        if (actuallyUseCompute)
        {
            mesh.bounds = skin.bounds; // skin is actually disabled, so it only remembers the last animation frame

            Graphics.DrawMesh(mesh, Matrix4x4.identity, ductTapedMaterial, 0);
        }
        else
        {
            Graphics
                .DrawMesh(meshForCPUOutput,
                Matrix4x4.identity,
                skin.sharedMaterial,
                0);
        }
    }

    void DrawDeltas()
    {
    }

    void DrawVerticesVsSkin()
    {
    }
#endregion
}
