using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutController : MonoBehaviour
{
    private Texture2D Pos1;
    private Texture2D Pos2;

    private Texture2D Normal1;
    private Texture2D Normal2;
    private Texture2D MainTexture;
    private Material mat;
    
    public GameObject objectWithMaterial;
    public GameObject posObject1;
    public GameObject posObject2;

    int Counter1 = 0;
    int Counter2 = 0;
    void onTriggerEnter(Collider other)
    {
        /*if (other.gameObject.tag == "Cutter")
        {
            Debug.Log("enter");
        }*/
                    Debug.Log("enter");

    }
  void onTriggerStay(Collider other)
    {
        /*if (other.gameObject.tag == "Cutter")
        {
            Debug.Log("stay");
        }*/
        Debug.Log("stay");
    }
  void onTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Cutter")
        {
            Debug.Log("exit");
        }
    }
        void Start()
    {
        mat = objectWithMaterial.GetComponent<MeshRenderer>().sharedMaterial;

        Pos1 = new Texture2D(256, 1, TextureFormat.RGBAFloat,false);
        for(int i=0;i<256;i++){
            Pos1.SetPixel(i, 0,new Color(0f, 0f, 0f, 0f)); 
        }

        mat.SetTexture("_posText1",Pos1);
        Pos1.Apply();

        Pos2 = new Texture2D(256, 1, TextureFormat.RGBAFloat,false);
        for(int i=0;i<256;i++){
            Pos2.SetPixel(i, 0,new Color(0f, 0f, 0f, 0f)); 
        }
        mat.SetTexture("_posText2",Pos2);
        Pos2.Apply();

        Normal1 = new Texture2D(256, 1, TextureFormat.RGBAFloat,false);
        for(int i=0;i<256;i++){
            Normal1.SetPixel(i, 0,new Color(0f, 0f, 0f, 0f)); 
        }
        mat.SetTexture("_normalText1",Normal1);
        Normal1.Apply();

        Normal2 = new Texture2D(256, 1, TextureFormat.RGBAFloat,false);
        for(int i=0;i<256;i++){
            Normal2.SetPixel(i, 0,new Color(0f, 0f, 0f, 0f)); 
        }
        mat.SetTexture("_normalText2",Normal2);
        Normal2.Apply();
    }
     void Update()
    {
       if (Input.GetKeyDown("space"))
        {
            //Debug.Log("space key was pressed");
            mat = objectWithMaterial.GetComponent<MeshRenderer>().sharedMaterial;
           // for(int Counter1=0;Counter1<256;Counter1++){ 
            Pos1.SetPixel(Counter1,0, new Color(posObject1.transform.position.x,posObject1.transform.position.y,posObject1.transform.position.z,1.0f));
            mat.SetTexture("_posText1",Pos1);
            Pos1.Apply();
           
            Normal1.SetPixel(Counter1,0, new Color(-posObject1.transform.forward.x,-posObject1.transform.forward.y,-posObject1.transform.forward.z,1.0f));
            mat.SetTexture("_normalText1",Normal1);
            Normal1.Apply();

            Counter1++; //~~nya

            Pos2.SetPixel(Counter2,0, new Color(posObject2.transform.position.x,posObject2.transform.position.y,posObject2.transform.position.z,1.0f));
            mat.SetTexture("_posText2",Pos2);
            Pos2.Apply();
           
            Normal2.SetPixel(Counter2,0, new Color(-posObject2.transform.forward.x,-posObject2.transform.forward.y,-posObject2.transform.forward.z,1.0f));
            mat.SetTexture("_normalText2",Normal2);
            Normal2.Apply();
            Counter2++; //~~nya
            
            if(Counter1 == 255) {Counter1= 0; Counter2= 0;}
            }
      //  }
      if (Input.GetKeyDown("k")) for(int i=0;i<256;i++) {Debug.Log(Pos1.GetPixel(i,0));}
      if (Input.GetKeyDown("z")) {
        for(int i=0;Counter1<256;i++) {    
        
        Pos1.SetPixel(i, 0,new Color(0f, 0f, 0f, 0f)); 
        mat.SetTexture("_posText1",Pos1);
        Pos1.Apply();

        Pos2.SetPixel(i, 0,new Color(0f, 0f, 0f, 0f)); 
        mat.SetTexture("_posText2",Pos2);
        Pos2.Apply();

        Normal1.SetPixel(i, 0,new Color(0f, 0f, 0f, 0f)); 
        mat.SetTexture("_normalText1",Normal1);
        Normal1.Apply();


        Normal2.SetPixel(i, 0,new Color(0f, 0f, 0f, 0f)); 
        mat.SetTexture("_normalText1",Normal2);
        Normal2.Apply();}
        
      }
    }
}
