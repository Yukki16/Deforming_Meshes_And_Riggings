using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ScreenSpaceBoolean
{

[ExecuteInEditMode]
public class SetSubstracteeArray : MonoBehaviour
{

    public List<CustomRendererFeature> features;
    public List<DepthRendererFeature> featuresDepth;
    public List<MaskRendererFeature> featuresSubtractors;
    List<Renderer> SubtracteeSet = new List<Renderer>();
    List<Renderer> SubtractorSet = new List<Renderer>();
    void SetSubtractees(List<Renderer> a){
        foreach(CustomRendererFeature r in features)
        r.settings.subtractees = a;
        foreach( DepthRendererFeature r in featuresDepth)
        r.settings.subtractees = a; 
    }
    void SetSubtractors(List<Renderer> a){
        foreach(MaskRendererFeature r in featuresSubtractors)
        r.settings.subtractors = a;
    }
    void Start()
    {
        foreach (var subtractee in Subtractee.GetAll()) {
           SubtracteeSet.Add(subtractee.GetComponent<Renderer>());
        }
        SetSubtractees(SubtracteeSet);

        foreach (var subtractor in Subtractor.GetAll()) {
           SubtractorSet.Add(subtractor.GetComponent<Renderer>());
        }
        SetSubtractors(SubtractorSet);
    }

    void Update()
    {

    }
}
}