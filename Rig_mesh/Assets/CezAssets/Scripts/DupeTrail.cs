using Parabox.CSG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GK
{
    public class DupeTrail : MonoBehaviour
    {
        public GameObject cuttingWeapon;
        // public GameObject trail;
        private GameObject composite;
        Mesh mesh;

        List<Vector3> points;
        List<int> tris;
        List<Vector3> normals;
        List<Vector3> verts;
        ConvexHullCalculator calc;
        // Start is called before the first frame update
        void Start()
        {

            composite = new GameObject();
          //  composite.transform.SetPositionAndRotation(cuttingWeapon.transform.position,cuttingWeapon.transform.rotation);
            composite.AddComponent<MeshFilter>();
            composite.AddComponent<MeshRenderer>();
            Model result = CSG.Union(cuttingWeapon, cuttingWeapon);
            composite.GetComponent<MeshFilter>().sharedMesh = result.mesh;
            composite.GetComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();
            /*
             * 

                        calc = new ConvexHullCalculator();
                        verts = new List<Vector3>();
                        tris = new List<int>();
                        normals = new List<Vector3>();
                        points = new List<Vector3>();
                        calc.GenerateHull(points, true, ref verts, ref tris, ref normals);

                        cuttingWeapon.GetComponent<MeshFilter>().mesh.SetVertices(verts);
                        cuttingWeapon.GetComponent<MeshFilter>().mesh.SetTriangles(tris, 0);
                        cuttingWeapon.GetComponent<MeshFilter>().mesh.SetNormals(normals);
                        points.Clear();*/
        }

        // Update is called once per frame
        void Update()
        {
            /*
            Model result = CSG.Union(cuttingWeapon, composite);
            composite.GetComponent<MeshFilter>().mesh = result.mesh;
            composite.GetComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();*/
            if (transform.hasChanged)
            {
                var dupe = new GameObject();
                dupe.AddComponent<MeshFilter>().sharedMesh = cuttingWeapon.GetComponent<MeshFilter>().sharedMesh;
                dupe.AddComponent<MeshRenderer>().sharedMaterials = cuttingWeapon.GetComponent<MeshRenderer>().sharedMaterials;
                dupe.transform.SetPositionAndRotation(cuttingWeapon.transform.position, cuttingWeapon.transform.rotation);
            }

        }
    }

}