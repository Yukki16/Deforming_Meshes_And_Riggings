using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace GK {
public class MeshTrailSkinned : MonoBehaviour
{
    public GameObject cuttingWeapon;
    Mesh trail;
    Mesh mesh;
    public GameObject posIndicator;

    public GameObject cutBody;
    Vector3[] vertices;
    Vector3[] originalVertices;
    List<Vector3> points;
    List<int> tris;
    List<Vector3> normals;
    List<Vector3> verts;
    ConvexHullCalculator calc;
    void Start()
    {  
        mesh =cuttingWeapon.GetComponent<MeshFilter>().mesh;
        GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
        vertices = new Vector3[mesh.vertices.Length];
        originalVertices = new Vector3[mesh.vertices.Length];
        vertices =mesh.vertices; 
        for(int i =0; i< vertices.Length; i++)
        originalVertices[i] = vertices[i];
        trail = new Mesh();

        calc = new ConvexHullCalculator();
        verts = new List<Vector3>();
		tris = new List<int>();
		normals = new List<Vector3>();
		points = new List<Vector3>();
        StartCoroutine(AddMeshSnapshot()); 
 
    }

    void Update()
    {
       
  // if (Input.GetKeyDown("x"))
  
    }

    IEnumerator AddMeshSnapshot()
    {
        while(true){
        //creating mesh
        for(int i =0; i< vertices.Length; i++){
       vertices[i] = originalVertices[i];
        }
        Vector3 rot = transform.InverseTransformDirection(posIndicator.transform.forward);
        for (var i = 0; i < vertices.Length; i++)
        {

          points.Add(Vector3.ProjectOnPlane(vertices[i], rot));
          points.Add(Vector3.ProjectOnPlane(vertices[i], rot)+Vector3.back);
        }
        calc.GenerateHull(points, true, ref verts, ref tris, ref normals);


        trail.SetVertices(verts);
		trail.SetTriangles(tris, 0);
		trail.SetNormals(normals);
        points.Clear();
        GetComponent<SkinnedMeshRenderer>().sharedMesh = trail;


        //Assigning weights
        int vertsArrayLength = GetComponent<SkinnedMeshRenderer>().sharedMesh.vertices.Length;
        GetComponent<SkinnedMeshRenderer>().bones = cutBody.GetComponent<SkinnedMeshRenderer>().bones;
        GetComponent<SkinnedMeshRenderer>().sharedMesh.bindposes = cutBody.GetComponent<SkinnedMeshRenderer>().sharedMesh.bindposes;
        byte[] bonesPerVertex = new byte[vertsArrayLength];
        BoneWeight1[] weights = new BoneWeight1[vertsArrayLength];
         for(int i =0; i< vertsArrayLength; i++){
            bonesPerVertex[i]=1;
            weights[i].boneIndex = 10;
            weights[i].weight = 1;
            
        }
        var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);
        var weightsArray = new NativeArray<BoneWeight1>(weights, Allocator.Temp);
        GetComponent<SkinnedMeshRenderer>().sharedMesh.SetBoneWeights(bonesPerVertexArray, weightsArray);
        bonesPerVertexArray.Dispose();
        weightsArray.Dispose();

        yield return new WaitForSeconds(0.5f);
        //mesh.RecalculateBounds();
        //transform.rotation= cuttingWeapon.transform.rotation;
    }   
    }

}
}





