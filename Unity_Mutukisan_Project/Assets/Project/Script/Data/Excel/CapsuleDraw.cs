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
        float scale = Mathf.Max(_collider.transform.localScale.x, _collider.transform.localScale.z);
        float radius = _collider.radius * scale;
        _height = (((_collider.height * _collider.transform.localScale.y) / 2.0f) - radius) ;
        Debug.Log(_collider.transform.localScale.y);
        Vector3 start_pos = Vector3.zero;
        start_pos = _collider.center + _collider.transform.position + ((_height * _collider.transform.up));
        Vector3 end_pos = Vector3.zero;

        end_pos = _collider.center + _collider.transform.position + ((_height * (_collider.transform.up * -1.0f)));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start_pos, radius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(end_pos, radius);
    }

}
