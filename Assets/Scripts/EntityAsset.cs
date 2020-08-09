using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

    [Serializable]
    public class EntityAsset
    {
        [SerializeField]
        private AssetReference _asset = null;
 
        [SerializeField]
        private AssetReference _material = null;
 
        public AssetReference Asset => _asset;
        public AssetReference Material => _material;


        [MenuItem("Serialize/Create Entity Asset")]
        public static void CreateAsset()
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<EntitySettings>(), "Assets/mydotsasset.asset");
        }
    }
 
    
    public class EntitySettings : ScriptableObject
    {
        [SerializeField]
        private EntityAsset _player = null;
 
        public EntityAsset Player => _player;
    }