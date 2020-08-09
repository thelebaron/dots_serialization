using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UIElements;
using Collider = Unity.Physics.Collider;
using Object = UnityEngine.Object;

namespace DOTS.Serialization.ReferencedObjects
{
    public static class AssetDatabaseUtility
    {
        public static List<Object> GetEverything()
        {
            var list = new List<Object>();
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var path in allAssetPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                var prefab = PrefabUtility.GetPrefabAssetType(asset);
                
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (prefab)
                {
                    case PrefabAssetType.Model:
                    {
                        var tr = (GameObject) asset;
                        var meshFilters = tr.GetComponentsInChildren<MeshFilter>();
                        foreach (var meshFilter in meshFilters)
                        {
                            var mesh = meshFilter.sharedMesh;
                            list.Add(mesh);
                        }
                        break;
                    }
                }
                
                if (ShouldIgnore(asset.GetType()))
                    continue;
                list.Add(asset);
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
    }
}