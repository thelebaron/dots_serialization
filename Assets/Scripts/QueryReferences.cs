using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

public static class QueryReferences
{
    public static void Query(ReferencedUnityObjects referencedUnityObjects)
    {
        for (int i = 0; i < referencedUnityObjects.Array.Length; i++)
        {
            Debug.Log(i + " " + referencedUnityObjects.Array[i].name +" "+ referencedUnityObjects.Array[i].GetType());
        }
    }
}
