using System;
using Unity.Rendering;
using UnityEngine;
using Object = System.Object;

namespace DOTS.Serialization
{
    [ExecuteAlways]
    public class TestObjectSerialization : MonoBehaviour
    {
        public object[] Array;
        
        private void OnEnable()
        {
            Array = new Object[2];

            Array[0] = 13;
            Array[1] = new RenderMesh();

            for (int i = 0; i < Array.Length; i++)
            {
                Debug.Log(Array[i]);
            }
        }
    }
}