using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public TestDataBase test_data_;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (test_data_ == null) return;

        Debug.Log(test_data_.brain_data_[0].brain_name_);
    }
}
