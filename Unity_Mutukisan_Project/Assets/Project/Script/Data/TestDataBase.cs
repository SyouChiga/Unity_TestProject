#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create TestDataBase")]
public class TestDataBase : ScriptableObject
{
    [System.Serializable]
    public struct BrainData
    {
        public int brain_id_;
        public string brain_name_;
    }

    [SerializeField]
    public List<BrainData> brain_data_;
}
#endif
