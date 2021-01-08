using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Col1 : MonoBehaviour
{
    bool hit = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void ControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log(hit.gameObject.name);
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("aaaaaa");
    }
}
