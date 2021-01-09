using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxDraw : MonoBehaviour
{
    [SerializeField]
    BoxCollider collider_;

    private void Reset()
    {
        collider_ = (BoxCollider)GetComponent<Collider>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {


        Gizmos.matrix = Matrix4x4.TRS(this.transform.TransformPoint(collider_.center), this.transform.rotation, this.transform.lossyScale);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, collider_.size);
    }
}
