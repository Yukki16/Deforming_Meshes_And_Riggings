using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderControl : MonoBehaviour
{
    public GameObject objectWithMaterial;
    GameObject sphere;
    Vector3 pos;
       Ray ray;
       SphereCollider sphereCollider;
    // Start is called before the first frame update
    void Start()
    {
        pos = new Vector3(0.0f,0.0f,0.0f);

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(0, 1.5f, 0);
        sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        sphereCollider = objectWithMaterial.GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        if(sphereCollider.Raycast(ray, out hitData, 1000))
            {
                pos = hitData.point;
                sphere.transform.position = pos;
                objectWithMaterial.GetComponent<MeshRenderer>().material.SetVector("_test", pos); 
            }
    
  
    }
}
