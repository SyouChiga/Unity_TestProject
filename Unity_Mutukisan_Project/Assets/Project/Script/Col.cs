using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Col : MonoBehaviour
{
    bool hit = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LateUpdate()
    {

        GetComponent<CharacterController>().Move(new Vector3(0.05f, 0.0f, 0.0f));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("aaaaaa");
    }
}