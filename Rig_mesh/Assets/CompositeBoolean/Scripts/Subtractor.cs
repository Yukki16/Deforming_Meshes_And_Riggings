using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace ScreenSpaceBoolean
{

[ExecuteInEditMode]
public class Subtractor : MonoBehaviour
{
    static public HashSet<Subtractor> instances = new HashSet<Subtractor>();

    void OnEnable()
    {
        instances.Add(this);
    }

    void OnDisable()
    {
        instances.Remove(this);
    }

    static public HashSet<Subtractor> GetAll()
    {
        return instances;
    }

}

}