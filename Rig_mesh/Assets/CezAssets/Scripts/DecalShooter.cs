using System.Collections;
using System.Collections.Generic;
using System.Net;
using SkinnedMeshDecals;
using UnityEngine;

public class DecalShooter : MonoBehaviour {
    public LayerMask hitMask;
    public bool useProjector;
    public bool useProjector2;
    public Material projector;
    public Material projector2;
    public Color color;
    [Range(0f,5f)]
    public float size;
    [Range(0f,5f)]
    public float size2;
    void Start() {
        projector = Instantiate(projector);
        projector2 = Instantiate(projector2);
    }
    void Update() {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, hitMask, QueryTriggerInteraction.Ignore)) {
            projector.color = color;
            projector2.color = color;
            if (!hit.collider.TryGetComponent(out DecalableCollider decalableCollider)) {
                return;
            }
           foreach (var decalableRenderer in decalableCollider.GetDecalableRenderers()) {
                if(useProjector)  PaintDecal.RenderDecal(decalableRenderer, projector, hit.point-transform.forward*0.25f,
                    Quaternion.FromToRotation(Vector3.forward, transform.forward), Vector2.one * size, 0.6f);
                 if(useProjector2)  PaintDecal.RenderDecal(decalableRenderer, projector2, hit.point-transform.forward*0.25f,
                    Quaternion.FromToRotation(Vector3.forward, transform.forward), Vector2.one * size2, 0.6f, "_DecalAlphaMap");
              
            }
        }
    }
}
