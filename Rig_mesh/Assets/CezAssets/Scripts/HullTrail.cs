using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using System;

namespace GK {
public class HullTrail : MonoBehaviour
{
    public GameObject cuttingWeapon;
    Mesh mesh;
   // public GameObject posIndicator;
  
    List<Vector3> points;
    List<int> tris;
    List<Vector3> normals;
    List<Vector3> verts;
    ConvexHullCalculator calc;
    Mesh mesh2;
public struct UpdateHull : IJob
{

        public void Execute()
    {
    }
}
    void Start()
    {   mesh2 = new Mesh();
        mesh = new Mesh();
        calc = new ConvexHullCalculator();
        verts = new List<Vector3>();
		tris = new List<int>();
		normals = new List<Vector3>();
		points = new List<Vector3>();
        
     
    }

    void Update()
    {
       
   if (Input.GetKeyDown("x")){

      StartCoroutine(AddMeshSnapshot(mesh.vertices)); 
   }
    }

    IEnumerator AddMeshSnapshot(Vector3[] vertices)
    {
			
			
          
            while(true) {
                  
			for (int i = 0; i < vertices.Length; i++) {
		    points.Add(transform.InverseTransformPoint(cuttingWeapon.transform.TransformPoint(vertices[i])));
            Debug.Log("" + i + "" );
			}
            calc.GenerateHull(points, false, ref verts, ref tris, ref normals);
            mesh2.SetVertices(verts);
			mesh2.SetTriangles(tris, 0);
			mesh2.SetNormals(normals);
            points.Clear();
            points.AddRange(verts);
            GetComponent<MeshFilter>().mesh = mesh2;
			GetComponent<MeshCollider>().sharedMesh = mesh2;

			yield return new WaitForSeconds(0.5f);
            }

    }
}

}





     /*   for(int i =0; i< vertices.Length; i++)
        vertices[i] = originalVertices[i];
        Vector3 rot = transform.InverseTransformDirection(posIndicator.transform.forward);
        for (var i = 0; i < vertices.Length; i++)
        {
           //vertices[i] -= rot*Vector3.Dot(originalVertices[i]-posIndicator.transform.position,rot) ;
           vertices[i]= Vector3.ProjectOnPlane(vertices[i], rot);
        }
        hull.vertices = vertices;*/
        //mesh.RecalculateBounds();
        //transform.rotation= cuttingWeapon.transform.rotation;