using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTexture : MonoBehaviour
{
    public GameObject objectWithMaterial;
    // Start is called before the first frame update
    Vector3 pos;
    Ray ray;
    MeshCollider meshCollider;
    public Texture aTexture;
    void Start()
    {
        meshCollider = objectWithMaterial.GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
     
        if(meshCollider.Raycast(ray, out hit, 1000))
            {
                pos = hit.point;
                Vector2 coord = new Vector2(hit.textureCoord.x * 1024, 1024 - hit.textureCoord.y * 1024);
                Debug.Log(hit.textureCoord);
                Graphics.DrawTexture(new Rect(coord.x, coord.y, 100, 100), aTexture);
            }
    }
}
