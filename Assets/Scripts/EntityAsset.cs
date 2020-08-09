using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

#if UNITY_EDITOR
        
        [MenuItem("Serialize/Create Entity Asset")]
        public static void CreateAsset()
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<EntitySettings>(), "Assets/mydotsasset.asset");
        }
#endif
    }
 
    
    public class EntitySettings : ScriptableObject
    {
        [SerializeField]
        private EntityAsset _player = null;
 
        public EntityAsset Player => _player;
    }