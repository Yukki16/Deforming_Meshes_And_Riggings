﻿//BSD 2 - Clause License

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

//#define DEBUG_GPU_OUTPUT
using UnityEngine;

public class DDMUtilsGPU
{
    public struct IndexWeightPair
    {
        public int index;

        public float weight;
    }

    public static int omegaCount = DDMSkinnedMeshGPUVar0.maxOmegaCount;

    public static bool isTestingPerformance = false;

    private static void SynchronizeCompute()
    {
    }

    public static IndexWeightPair[,]
    ComputeLaplacianWithIndexFromAdjacency(int[,] adjacencyMatrix)
    {
        UnityEngine
            .Profiling
            .Profiler
            .BeginSample("ComputeLaplacianWithIndexFromAdjacency");
        int vCount = adjacencyMatrix.GetLength(0);
        int aCount = adjacencyMatrix.GetLength(1);
        IndexWeightPair[,] indexWeightPairsCPU =
            new IndexWeightPair[vCount, omegaCount];

        for (int vi = 0; vi < vCount; ++vi)
        {
            float sum = 0.0f;
            int ai = 0;
            for (; ai < aCount && ai < omegaCount; ++ai)
            {
                int adjVi = adjacencyMatrix[vi, ai];
                if (adjVi >= 0)
                {
                    sum += 1.0f;
                }
                indexWeightPairsCPU[vi, ai].index = adjVi;
            }
            for (ai = 0; ai < aCount && ai < omegaCount; ++ai)
            {
                int adjVi = adjacencyMatrix[vi, ai];
                if (adjVi < 0)
                {
                    break;
                }
                indexWeightPairsCPU[vi, ai].weight = -1.0f / sum;
            }
        }

        UnityEngine.Profiling.Profiler.EndSample();
        return indexWeightPairsCPU;
    }

    public static bool
    ComputeLaplacianCBFromAdjacencyCPU(
        ref ComputeBuffer laplacianCB,
        ComputeShader precomputeShader,
        int[,] adjacencyMatrix
    )
    {
        if (precomputeShader == null)
        {
            return false;
        }
        UnityEngine
            .Profiling
            .Profiler
            .BeginSample("ComputeLaplacianCBFromAdjacencyCPU");

        int vCount = adjacencyMatrix.GetLength(0);
        int aCount = adjacencyMatrix.GetLength(1);

        Debug
            .Assert(laplacianCB.count == vCount * omegaCount &&
            laplacianCB.stride == sizeof(int) + sizeof(float));

        IndexWeightPair[,] indexWeightPairsCPU =
            ComputeLaplacianWithIndexFromAdjacency(adjacencyMatrix);
        laplacianCB.SetData(indexWeightPairsCPU);

        UnityEngine.Profiling.Profiler.EndSample();
        return true;
    }

    public static bool
    ComputeLaplacianCBFromAdjacency(
        ref ComputeBuffer laplacianCB,
        ComputeShader precomputeShader,
        int[,] adjacencyMatrix
    )
    {
        if (precomputeShader == null)
        {
            return false;
        }
        UnityEngine
            .Profiling
            .Profiler
            .BeginSample("ComputeLaplacianCBFromAdjacency");

        int vCount = adjacencyMatrix.GetLength(0);
        int aCount = adjacencyMatrix.GetLength(1);

        ComputeBuffer AdjacencyCB =
            new ComputeBuffer(vCount * aCount, sizeof(int));
        AdjacencyCB.SetData(adjacencyMatrix);

        Debug
            .Assert(laplacianCB.count == vCount * omegaCount &&
            laplacianCB.stride == sizeof(int) + sizeof(float));

        precomputeShader.SetInt("VertexCount", vCount);

        uint
            threadGroupSizeX,
            threadGroupSizeY,
            threadGroupSizeZ;

        int kernelBuildLaplacian =
            precomputeShader.FindKernel("BuildLaplacianFromAdjacency");
        precomputeShader
            .GetKernelThreadGroupSizes(kernelBuildLaplacian,
            out threadGroupSizeX,
            out threadGroupSizeY,
            out threadGroupSizeZ);
        int threadGroupsX =
            (vCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
        precomputeShader
            .SetBuffer(kernelBuildLaplacian, "Adjacency", AdjacencyCB);
        precomputeShader
            .SetBuffer(kernelBuildLaplacian, "OutLaplacian", laplacianCB);
        precomputeShader.Dispatch(kernelBuildLaplacian, threadGroupsX, 1, 1);

        AdjacencyCB.Release();

        if (isTestingPerformance)
        {
            SynchronizeCompute();
        }

        UnityEngine.Profiling.Profiler.EndSample();
        return true;
    }

    public static DDMUtilsIterative.OmegaWithIndex[,]
    ComputeOmegasFromLaplacian(
        Vector3[] vertices,
        IndexWeightPair[,] laplacian,
        BoneWeight[] weights,
        int boneCount,
        int iterations,
        float lambda
    )
    {
        int vCount = vertices.Length;
        int aCount = omegaCount;
        UnityEngine
            .Profiling
            .Profiler
            .BeginSample("ComputeOmegasFromLaplacian");

        DDMUtilsIterative.OmegaWithIndex[,] omegas =
            new DDMUtilsIterative.OmegaWithIndex[vCount, aCount];

        Matrix4x4[,] tmpOmegas0 = new Matrix4x4[vCount, boneCount];
        int[,] tmpOmegaIdxs0 = new int[vCount, boneCount];
        Matrix4x4[,] tmpOmegas1 = new Matrix4x4[vCount, boneCount];
        int[,] tmpOmegaIdxs1 = new int[vCount, boneCount];

        // InitOmegas
        for (int vi = 0; vi < vCount; ++vi)
        {
            for (int bi = 0; bi < boneCount; ++bi)
            {
                tmpOmegaIdxs0[vi, bi] = -1;
                tmpOmegaIdxs1[vi, bi] = -1;
            }
        }

        // PreStep
        for (int vi = 0; vi < vCount; ++vi)
        {
            Vector3 pos = vertices[vi];

            Matrix4x4 Pi4 = Matrix4x4.identity;
            for (int row = 0; row < 3; ++row)
            {
                for (int col = 0; col < 3; ++col)
                {
                    Pi4[row, col] = pos[row] * pos[col];
                }
            }
            Pi4[3, 0] = pos[0];
            Pi4[3, 1] = pos[1];
            Pi4[3, 2] = pos[2];
            Pi4[0, 3] = pos[0];
            Pi4[1, 3] = pos[1];
            Pi4[2, 3] = pos[2];

            BoneWeight bw = weights[vi];

            /// Start whole bone
            if (bw.boneIndex0 >= 0 && bw.weight0 > 0.0)
            {
                tmpOmegaIdxs0[vi, bw.boneIndex0] = bw.boneIndex0;
                Matrix4x4 tmpMat = Matrix4x4.zero;
                for (int i = 0; i < 16; ++i)
                {
                    tmpMat[i] = Pi4[i] * bw.weight0;
                }
                tmpOmegas0[vi, bw.boneIndex0] = tmpMat;
            }

            if (bw.boneIndex1 >= 0 && bw.weight1 > 0.0)
            {
                tmpOmegaIdxs0[vi, bw.boneIndex1] = bw.boneIndex1;
                Matrix4x4 tmpMat = Matrix4x4.zero;
                for (int i = 0; i < 16; ++i)
                {
                    tmpMat[i] = Pi4[i] * bw.weight1;
                }
                tmpOmegas0[vi, bw.boneIndex1] = tmpMat;
            }

            if (bw.boneIndex2 >= 0 && bw.weight2 > 0.0)
            {
                tmpOmegaIdxs0[vi, bw.boneIndex2] = bw.boneIndex2;
                Matrix4x4 tmpMat = Matrix4x4.zero;
                for (int i = 0; i < 16; ++i)
                {
                    tmpMat[i] = Pi4[i] * bw.weight2;
                }
                tmpOmegas0[vi, bw.boneIndex2] = tmpMat;
            }

            if (bw.boneIndex3 >= 0 && bw.weight3 > 0.0)
            {
                tmpOmegaIdxs0[vi, bw.boneIndex3] = bw.boneIndex3;
                Matrix4x4 tmpMat = Matrix4x4.zero;
                for (int i = 0; i < 16; ++i)
                {
                    tmpMat[i] = Pi4[i] * bw.weight3;
                }
                tmpOmegas0[vi, bw.boneIndex3] = tmpMat;
            }
        }

        // OneSweep
        for (int it = 0; it < iterations; ++it)
        {
            Matrix4x4[,] inOmegas = (it % 2 == 0) ? tmpOmegas0 : tmpOmegas1;
            int[,] inOmegaIdxs = (it % 2 == 0) ? tmpOmegaIdxs0 : tmpOmegaIdxs1;
            Matrix4x4[,] outOmegas = (it % 2 == 0) ? tmpOmegas1 : tmpOmegas0;
            int[,] outOmegaIdxs = (it % 2 == 0) ? tmpOmegaIdxs1 : tmpOmegaIdxs0;

            for (int vi = 0; vi < vCount; ++vi)
            {
                for (int bi = 0; bi < boneCount; ++bi)
                {
                    float weight0 = 1.0f - lambda;
                    Matrix4x4 inOmega0 = inOmegas[vi, bi];
                    int inOmegaIdx0 = inOmegaIdxs[vi, bi];
                    int counter = 0;
                    Matrix4x4 outOmega = Matrix4x4.zero;
                    if (inOmegaIdx0 >= 0)
                    {
                        counter = 1;
                        for (int i = 0; i < 16; ++i)
                        {
                            outOmega[i] = inOmega0[i] * weight0;
                        }
                    }

                    // Correct?
                    for (int ai = 0; ai < aCount; ++ai)
                    {
                        IndexWeightPair iwp = laplacian[vi, ai];
                        int ki = iwp.index;
                        if (ki < 0)
                        {
                            break;
                        }
                        float weight = -iwp.weight * lambda;
                        Matrix4x4 inOmega = inOmegas[ki, bi];
                        int inOmegaIdx = inOmegaIdxs[ki, bi];
                        if (inOmegaIdx >= 0)
                        {
                            ++counter;
                            for (int i = 0; i < 16; ++i)
                            {
                                outOmega[i] += inOmega[i] * weight;
                            }
                        }
                    }
                    if (counter == 0)
                    {
                        outOmegas[vi, bi] = Matrix4x4.zero;
                        outOmegaIdxs[vi, bi] = -1;
                    }
                    else
                    {
                        outOmegas[vi, bi] = outOmega;
                        outOmegaIdxs[vi, bi] = bi;
                    }
                }
            }
        }

        // Compress
        Matrix4x4[,] inOmegas1 =
            (iterations % 2 == 0) ? tmpOmegas0 : tmpOmegas1;
        int[,] inOmegaIdxs1 =
            (iterations % 2 == 0) ? tmpOmegaIdxs0 : tmpOmegaIdxs1;
        for (int vi = 0; vi < vCount; ++vi)
        {
            int curOI = 0;
            for (int bi = 0; bi < boneCount; ++bi)
            {
                int inIdx = inOmegaIdxs1[vi, bi];
                if (inIdx < 0)
                {
                    continue;
                }
                Matrix4x4 inOmega = inOmegas1[vi, bi];
                DDMUtilsIterative.OmegaWithIndex oswi =
                    new DDMUtilsIterative.OmegaWithIndex();
                oswi.boneIndex = bi;
                oswi.m00 = inOmega.m00;
                oswi.m01 = inOmega.m01;
                oswi.m02 = inOmega.m02;
                oswi.m03 = inOmega.m03;
                oswi.m11 = inOmega.m11;
                oswi.m12 = inOmega.m12;
                oswi.m13 = inOmega.m13;
                oswi.m22 = inOmega.m22;
                oswi.m23 = inOmega.m23;
                oswi.m33 = inOmega.m33;
                omegas[vi, curOI] = oswi;
                ++curOI;
            }

            while (curOI < DDMSkinnedMeshGPUVar0.maxOmegaCount)
            {
                DDMUtilsIterative.OmegaWithIndex oswi =
                    new DDMUtilsIterative.OmegaWithIndex();
                oswi.boneIndex = -1;
                omegas[vi, curOI] = oswi;
                ++curOI;
            }
        }

        UnityEngine.Profiling.Profiler.EndSample();
        return omegas;
    }

    public static bool
    ComputeOmegasCBFromLaplacianCB(
        ref ComputeBuffer omegasCB,
        ComputeShader precomputeShader,
        ComputeBuffer verticesCB,
        ComputeBuffer laplacianCB,
        ComputeBuffer weightsCB,
        int boneCount,
        int iterations,
        float lambda
    )
    {
        if (precomputeShader == null)
        {
            return false;
        }
        UnityEngine
            .Profiling
            .Profiler
            .BeginSample("ComputeOmegasCBFromLaplacianCB");
        int vCount = verticesCB.count;
        int aCount = omegaCount;

        int stride = (10 * sizeof(float) + sizeof(int));
        Debug
            .Assert(omegasCB.count == vCount * aCount &&
            omegasCB.stride == stride);

        //omegasCB = new ComputeBuffer(vCount * aCount, stride);
        ComputeBuffer tmpOmegasCB0 =
            new ComputeBuffer(vCount * boneCount, stride);
        ComputeBuffer tmpOmegasCB1 =
            new ComputeBuffer(vCount * boneCount, stride);

        precomputeShader.SetInt("VertexCount", vCount);
        precomputeShader.SetInt("BoneCount", boneCount);
        precomputeShader.SetFloat("Lambda", lambda);

        uint
            threadGroupSizeX,
            threadGroupSizeY,
            threadGroupSizeZ;

        int kernelInitOmegas = precomputeShader.FindKernel("InitOmegas");
        precomputeShader
            .GetKernelThreadGroupSizes(kernelInitOmegas,
            out threadGroupSizeX,
            out threadGroupSizeY,
            out threadGroupSizeZ);
        int threadGroupsX =
            (vCount * boneCount + (int)threadGroupSizeX - 1) /
            (int)threadGroupSizeX;
        precomputeShader.SetBuffer(kernelInitOmegas, "Omegas", tmpOmegasCB0);
        precomputeShader.Dispatch(kernelInitOmegas, threadGroupsX, 1, 1);
        precomputeShader.SetBuffer(kernelInitOmegas, "Omegas", tmpOmegasCB1);
        precomputeShader.Dispatch(kernelInitOmegas, threadGroupsX, 1, 1);

#if DEBUG_GPU_OUTPUT
        DDMUtilsIterative.OmegaWithIndex[,] tmpOmegas =
            new DDMUtilsIterative.OmegaWithIndex[vCount, boneCount];
        tmpOmegasCB1.GetData (tmpOmegas);

        BoneWeight[] bws = new BoneWeight[vCount];
        weightsCB.GetData (bws);

        IndexWeightPair[,] laplacians = new IndexWeightPair[vCount, omegaCount];
        laplacianCB.GetData (laplacians);
#endif // DEBUG_GPU_OUTPUT

        int kernelPreStep = precomputeShader.FindKernel("ComputeOmegasPreStep");
        threadGroupsX =
            (vCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

        precomputeShader.SetBuffer(kernelPreStep, "Vertices", verticesCB);
        precomputeShader.SetBuffer(kernelPreStep, "Weights", weightsCB);

        precomputeShader.SetBuffer(kernelPreStep, "Omegas", tmpOmegasCB0);

        precomputeShader.Dispatch(kernelPreStep, threadGroupsX, 1, 1);

#if DEBUG_GPU_OUTPUT
        tmpOmegasCB0.GetData (tmpOmegas);

#endif // DEBUG_GPU_OUTPUT

        if (iterations > 0)
        {
            int kernelOneSweep =
                precomputeShader.FindKernel("ComputeOmegasOneSweep");

            precomputeShader
                .SetBuffer(kernelOneSweep, "Laplacian", laplacianCB);

            for (int it = 0; it < iterations; ++it)
            {
                precomputeShader
                    .SetBuffer(kernelOneSweep,
                    "PreOmegas",
                    (it % 2 == 0) ? tmpOmegasCB0 : tmpOmegasCB1);
                precomputeShader
                    .SetBuffer(kernelOneSweep,
                    "Omegas",
                    (it % 2 == 0) ? tmpOmegasCB1 : tmpOmegasCB0);
                precomputeShader.Dispatch(kernelOneSweep, threadGroupsX, 1, 1);

#if DEBUG_GPU_OUTPUT
                DDMUtilsIterative.OmegaWithIndex[,] tmpOmegas1 =
                    new DDMUtilsIterative.OmegaWithIndex[vCount, boneCount];
                ComputeBuffer tmpCB =
                    (it % 2 == 0) ? tmpOmegasCB1 : tmpOmegasCB0;
                tmpCB.GetData (tmpOmegas1);
                Debug.Log("tmpCB" + it.ToString());
#endif // DEBUG_GPU_OUTPUT
            }
        }

        int kernelCompressOmegas =
            precomputeShader.FindKernel("CompressOmegas");

        precomputeShader
            .SetBuffer(kernelCompressOmegas,
            "PreOmegas",
            (iterations % 2 == 0) ? tmpOmegasCB0 : tmpOmegasCB1);
        precomputeShader.SetBuffer(kernelCompressOmegas, "Omegas", omegasCB);

        precomputeShader.Dispatch(kernelCompressOmegas, threadGroupsX, 1, 1);

#if DEBUG_GPU_OUTPUT
        DDMUtilsIterative.OmegaWithIndex[,] outOmegas =
            new DDMUtilsIterative.OmegaWithIndex[vCount, omegaCount];
        omegasCB.GetData (outOmegas);
#endif // DEBUG_GPU_OUTPUT

        tmpOmegasCB0.Release();
        tmpOmegasCB1.Release();

        if (isTestingPerformance)
        {
            SynchronizeCompute();
        }

        UnityEngine.Profiling.Profiler.EndSample();
        return true;
    }

    public static bool
    ComputePppsCBFromOmegasCB(
        ref ComputeBuffer pppsCB,
        ComputeShader precomputeShader,
        ComputeBuffer omegasCB,
        int vCount,
        int boneCount
    )
    {
        if (precomputeShader == null)
        {
            return false;
        }

        precomputeShader.SetInt("VertexCount", vCount);
        precomputeShader.SetInt("BoneCount", boneCount);

        uint
            threadGroupSizeX,
            threadGroupSizeY,
            threadGroupSizeZ;

        int kernelComputePpps = precomputeShader.FindKernel("ComputePpps");
        precomputeShader
            .GetKernelThreadGroupSizes(kernelComputePpps,
            out threadGroupSizeX,
            out threadGroupSizeY,
            out threadGroupSizeZ);
        int threadGroupsX =
            (vCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
        precomputeShader.SetBuffer(kernelComputePpps, "PreOmegas", omegasCB);
        precomputeShader.SetBuffer(kernelComputePpps, "Ppps", pppsCB);
        precomputeShader.Dispatch(kernelComputePpps, threadGroupsX, 1, 1);

        return true;
    }

    public static bool
    ComputeLastcolsCBPsCBFromOmegasCB(
        ref ComputeBuffer lastcolsCB,
        ref ComputeBuffer psCB,
        ComputeShader precomputeShader,
        ComputeBuffer omegasCB,
        int vCount,
        int boneCount
    )
    {
        if (precomputeShader == null)
        {
            return false;
        }

        precomputeShader.SetInt("VertexCount", vCount);
        precomputeShader.SetInt("BoneCount", boneCount);

        uint
            threadGroupSizeX,
            threadGroupSizeY,
            threadGroupSizeZ;

        int kernelComputeLastcolsPs =
            precomputeShader.FindKernel("ComputeLastcolsPs");
        precomputeShader
            .GetKernelThreadGroupSizes(kernelComputeLastcolsPs,
            out threadGroupSizeX,
            out threadGroupSizeY,
            out threadGroupSizeZ);
        int threadGroupsX =
            (vCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
        precomputeShader
            .SetBuffer(kernelComputeLastcolsPs, "PreOmegas", omegasCB);
        precomputeShader
            .SetBuffer(kernelComputeLastcolsPs, "Lastcols", lastcolsCB);
        precomputeShader.SetBuffer(kernelComputeLastcolsPs, "Ps", psCB);
        precomputeShader.Dispatch(kernelComputeLastcolsPs, threadGroupsX, 1, 1);

        return true;
    }

    public static bool
    ComputeLastomegasCBPsCBFromOmegasCB(
        ref ComputeBuffer lastomegasCB,
        ref ComputeBuffer psCB,
        ComputeShader precomputeShader,
        ComputeBuffer omegasCB,
        int vCount,
        int boneCount
    )
    {
        if (precomputeShader == null)
        {
            return false;
        }

        precomputeShader.SetInt("VertexCount", vCount);
        precomputeShader.SetInt("BoneCount", boneCount);

        uint
            threadGroupSizeX,
            threadGroupSizeY,
            threadGroupSizeZ;

        int kernelComputeLastomegasPs =
            precomputeShader.FindKernel("ComputeLastomegasPs");
        precomputeShader
            .GetKernelThreadGroupSizes(kernelComputeLastomegasPs,
            out threadGroupSizeX,
            out threadGroupSizeY,
            out threadGroupSizeZ);
        int threadGroupsX =
            (vCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
        precomputeShader
            .SetBuffer(kernelComputeLastomegasPs, "PreOmegas", omegasCB);
        precomputeShader
            .SetBuffer(kernelComputeLastomegasPs, "Lastomegas", lastomegasCB);
        precomputeShader.SetBuffer(kernelComputeLastomegasPs, "Ps", psCB);
        precomputeShader
            .Dispatch(kernelComputeLastomegasPs, threadGroupsX, 1, 1);

        return true;
    }
}