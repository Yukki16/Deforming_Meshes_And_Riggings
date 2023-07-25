using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DrawTexture : MonoBehaviour
{
    public Camera cam;
   // public Material projector;
    public float size;
    Vector3 pos;
    Ray ray;
    MeshCollider meshCollider;
    public GameObject posObject1;

    void Start()
    {
        cam = GetComponent<Camera>();
      //  projector = Instantiate(projector);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0))
        return;

       RaycastHit hit;
            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

       posObject1.transform.position= hit.point;

      /*
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

            
            pos = hit.point;
               if (!hit.collider.TryGetComponent(out DecalableCollider decalableCollider)) {
               Debug.Log("no collider");
                return;
               
            }
           
            foreach (var decalableRenderer in decalableCollider.GetDecalableRenderers()) {

              /* PaintDecal.RenderDecal(decalableRenderer, projector, hit.point-transform.forward*0.25f,
                    Quaternion.FromToRotation(Vector3.forward, transform.forward), Vector2.one * size, 0.1f);
                     PaintDecal.RenderDecal(decalableRenderer, projector, hit.point,
                  Quaternion.FromToRotation(Vector3.forward, hit.collider.transform.forward), Vector2.one * size, 0.01f);
                      Debug.Log("i");
            
            }
            */
    }
}
