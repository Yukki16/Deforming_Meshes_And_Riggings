using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace ScreenSpaceBoolean
{

[ExecuteInEditMode]
public class Subtractee : MonoBehaviour
{

    static public HashSet<Subtractee> instances = new HashSet<Subtractee>();

    void OnEnable()
    {
        instances.Add(this);
    }

    void OnDisable()
    {
        instances.Remove(this);
    }

    static public HashSet<Subtractee> GetAll()
    {
        return instances;
    }

}

}