using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhatIf_Update_collider : MonoBehaviour
{
    public GameObject objectA;
    
    private Mesh meshA;

    void Start()
    {
         InvokeRepeating("UpdateCollisionMesh", 2.0f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        meshA = new Mesh();

    }

public void UpdateCollisionMesh()

{
    objectA.GetComponent<SkinnedMeshRenderer>().BakeMesh(meshA,true); //lmao le funny computationally expensive task
    objectA.GetComponent<MeshCollider>().sharedMesh = meshA;
}
}


