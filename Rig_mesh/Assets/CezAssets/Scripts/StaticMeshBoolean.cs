using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;

public class Boolean : MonoBehaviour
{
    public GameObject objectA;
    public GameObject objectB;
    
    private Mesh meshA;
    private Mesh meshB;
    private Mesh meshC;
    void Start()
    {
    meshA = objectA.GetComponent<SkinnedMeshRenderer>().sharedMesh;
    meshB = objectB.GetComponent<MeshFilter>().mesh;
Model result = CSG.Subtract(objectA, objectB);
var composite = new GameObject();
GameObject newObj = new GameObject(); //a new gameobject for creating the mesh
newObj.AddComponent<SkinnedMeshRenderer>();
meshC = new Mesh();
meshC.vertices = meshA.vertices;
meshC.triangles = meshA.triangles;
meshC.boneWeights=meshA.boneWeights;
meshC.bindposes= meshA.bindposes;
composite.GetComponent<SkinnedMeshRenderer>().bones= objectA.GetComponent<SkinnedMeshRenderer>().bones;
composite.GetComponent<SkinnedMeshRenderer>().rootBone= objectA.GetComponent<SkinnedMeshRenderer>().rootBone;
composite.GetComponent<SkinnedMeshRenderer>().materials= objectA.GetComponent<SkinnedMeshRenderer>().materials;
composite.GetComponent<SkinnedMeshRenderer>().sharedMesh = meshC;
composite.AddComponent<SkinnedMeshRenderer>().sharedMesh = result.mesh;
composite.AddComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
