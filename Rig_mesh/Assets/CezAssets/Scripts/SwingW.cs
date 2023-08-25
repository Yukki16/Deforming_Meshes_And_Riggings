using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingW : MonoBehaviour
{
    public GameObject swingWeapon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r"))
        swingWeapon.transform.localPosition = Vector3.Slerp(swingWeapon.transform.localPosition, new Vector3(1, 0, 1), 0.01f);
    }
}
