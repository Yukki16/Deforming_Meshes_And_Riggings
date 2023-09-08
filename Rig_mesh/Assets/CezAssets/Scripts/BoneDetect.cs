using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneDetect : MonoBehaviour
{


    void Start()
    {
    }

    void OnTriggerEnter(Collider collision)
    { 
      /*  foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }*/
            Debug.Log("Hit "+ collision.gameObject.name);  
    }
}