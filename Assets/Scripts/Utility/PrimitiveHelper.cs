using UnityEngine;

namespace DOTS.Serialization
{
    using System.Collections.Generic;
    using UnityEngine;
 
    public static class PrimitiveHelper
    {
        private static Dictionary<PrimitiveType, Mesh> primitiveMeshes = new Dictionary<PrimitiveType, Mesh>();
 
        public static GameObject CreatePrimitive(PrimitiveType type, bool withCollider)
        {
            if (withCollider) { return GameObject.CreatePrimitive(type); }
 
            GameObject gameObject = new GameObject(type.ToString());
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = PrimitiveHelper.GetPrimitiveMesh(type);
            gameObject.AddComponent<MeshRenderer>();
 
            return gameObject;
        }
 
        public static Mesh GetPrimitiveMesh(PrimitiveType type)
        {
            if (!PrimitiveHelper.primitiveMeshes.ContainsKey(type))
            {
                PrimitiveHelper.CreatePrimitiveMesh(type);
            }
 
            return PrimitiveHelper.primitiveMeshes[type];
        }
 
        private static Mesh CreatePrimitiveMesh(PrimitiveType type)
        {
            var gameObject = GameObject.CreatePrimitive(type);
            var       mesh       = gameObject.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(gameObject);
 
            PrimitiveHelper.primitiveMeshes[type] = mesh;
            return mesh;
        }

        public static Material DefaultMaterial()
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            var mat       = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
            Object.DestroyImmediate(gameObject);;
            return mat;
        }
    }
}