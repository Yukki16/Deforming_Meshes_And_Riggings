using System.Collections;
using System.Collections.Generic;
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
        projector = Material.Instantiate(projector);
      projector2 = Material.Instantiate(projector2);
    }
    void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, hitMask, QueryTriggerInteraction.Ignore)) {
            projector.color = color;
            projector2.color = color;
          if(useProjector)  SkinnedMeshDecals.PaintDecal.RenderDecalForCollision(hit.collider, projector, hit.point, hit.normal, UnityEngine.Random.Range(0f,360f), Vector2.one * size, 0.6f);
          if(useProjector2) SkinnedMeshDecals.PaintDecal.RenderDecalForCollision(hit.collider, projector2, hit.point, hit.normal, UnityEngine.Random.Range(0f,360f), Vector2.one * size2, 0.6f , "_DecalAlphaMap");
        }
    }
}
