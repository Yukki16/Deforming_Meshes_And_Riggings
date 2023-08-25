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
using UnityEngine.UIElements;

namespace GK {
public class MeshTrail : MonoBehaviour
{
    public GameObject cuttingWeapon;
    Mesh trail;
    Mesh mesh;
    public GameObject posIndicator;
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
        GetComponent<MeshFilter>().mesh = mesh;
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
        for(int i =0; i< vertices.Length; i++){
       vertices[i] = originalVertices[i];
        }
        Vector3 rot = transform.InverseTransformDirection(posIndicator.transform.forward);
        for (var i = 0; i < vertices.Length; i++)
        {
           //vertices[i] -= rot*Vector3.Dot(originalVertices[i]-posIndicator.transform.position,rot) ;
          // vertices[i]= Vector3.ProjectOnPlane(vertices[i], rot);
          points.Add(Vector3.ProjectOnPlane(vertices[i], rot));
          points.Add(Vector3.ProjectOnPlane(vertices[i], rot)+Vector3.back);
        }
        calc.GenerateHull(points, true, ref verts, ref tris, ref normals);
        trail.SetVertices(verts);
		trail.SetTriangles(tris, 0);
		trail.SetNormals(normals);
        points.Clear();
        GetComponent<MeshFilter>().mesh = trail;

        yield return new WaitForSeconds(0.5f);
        //mesh.RecalculateBounds();
        //transform.rotation= cuttingWeapon.transform.rotation;
    }   
    }

}
}





