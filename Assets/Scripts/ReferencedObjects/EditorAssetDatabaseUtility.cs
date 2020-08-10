using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using UnityEngine;
using Collider = Unity.Physics.Collider;
using Object = UnityEngine.Object;
using System.Security.Cryptography;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace DOTS.Serialization.ReferencedObjects
{
    public static class EditorAssetDatabaseUtility
    {
        

#if UNITY_EDITOR
        public static void Add(List<Object> list, Object obj)
        {
            if(!list.Contains(obj))
                list.Add(obj);
        }
        
        public static List<Object> GetEverything()
        {
            var list = new List<Object>();
            // Add default material
            Add(list, PrimitiveHelper.DefaultMaterial());
            // Add all default meshes
            foreach (var builtinMesh in BuiltinMeshes())
            {
                Add(list, builtinMesh);
            }
            // get every asset that exists
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var path in allAssetPaths)
            {
                // check if its a prefab
                var assetObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                var assetType = PrefabUtility.GetPrefabAssetType(assetObject);
                
                // if its a prefab, get its meshes -todo recursive child search
                switch (assetType)
                {
                    case PrefabAssetType.Model:
                    {
                        var prefab = (GameObject)assetObject;
                        
                        // Add all meshes from skinnedmeshrenderers
                        var skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                        foreach (var meshRenderer in skinnedRenderers)
                        {
                            Add(list, meshRenderer.sharedMaterial);
                            Add(list, meshRenderer.sharedMesh);
                        }
                        
                        // Add all materials from meshrenderers
                        var meshRenderers = prefab.GetComponentsInChildren<Renderer>();
                        foreach (var meshRenderer in meshRenderers)
                            Add(list, meshRenderer.sharedMaterial);
                        
                        // Add all meshes from meshfilters
                        var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
                        foreach (var meshFilter in meshFilters)
                            Add(list, meshFilter.sharedMesh);
                        
                        break;
                    }
                }
                
                if (ShouldIgnore(assetObject.GetType()))
                    continue;
                
                Add(list, assetObject);
            }
            
            return list;
        }
        
        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T>  assets = new List<T>();
            string[] guids  = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for( int i = 0; i < guids.Length; i++ )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                T      asset     = AssetDatabase.LoadAssetAtPath<T>( assetPath );
                if( asset != null )
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        /// <summary>
        /// Only add a few types for now, expand later
        /// </summary>
        private static bool ShouldIgnore(Type t)
        {
            if (t == typeof(Mesh) ||
                t == typeof(Material)||
                t == typeof(AnimationClip)||
                t == typeof(BlobAssetReference<Collider>)
            )
                return false;
            

            return true;
        }

        private static List<Mesh> BuiltinMeshes()
        {
            var list = new List<Mesh>();
            // Loop through all enum possibilities - https://stackoverflow.com/questions/972307/how-to-loop-through-all-enum-values-in-c
            foreach (PrimitiveType primitive in Enum.GetValues(typeof(PrimitiveType)))
            {
                list.Add(PrimitiveHelper.GetPrimitiveMesh(primitive));
            }
            
            
            return list;
        }
#endif
        
        public static Guid GenerateHash(Object obj)
        {
            Guid result;
            
            string input = obj.name + obj.GetType();
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash   = md5.ComputeHash(Encoding.Default.GetBytes(input));
                result = new Guid(hash);
            }

            return result;
        }
    }
}