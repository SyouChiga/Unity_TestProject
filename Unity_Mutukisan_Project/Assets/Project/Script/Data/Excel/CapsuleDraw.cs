using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleDraw : MonoBehaviour
{
    [SerializeField]
    private CapsuleCollider _collider;

    [SerializeField]
    private float _height = 0.0f;


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
        if (_collider == null) return;
        _height = ((_collider.height / 2.0f) - _collider.radius) * _collider.transform.localScale.y;
        Vector3 start_pos = Vector3.zero;
        start_pos = _collider.center + _collider.transform.position + ((_height * _collider.transform.up));
        Vector3 end_pos = Vector3.zero;

        end_pos = _collider.center + _collider.transform.position + ((_height * (_collider.transform.up * -1.0f)));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start_pos,_collider.radius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(end_pos, _collider.radius);
    }

}
