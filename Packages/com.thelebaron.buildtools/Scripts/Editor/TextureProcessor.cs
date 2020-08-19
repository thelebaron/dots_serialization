using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;
using Directory = UnityEngine.Windows.Directory;
#if UNITY_EDITOR
using System.Collections;
using Unity.Assertions;
using UnityEditor;
using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace Unity.Build
{
    public static class TextureProcessor
    {
        private static bool unlit = false;
        private static bool toggleDiminishedlighting = false;
        [MenuItem("Shader Controls/Diffuse Toggle _%U")]
        public static void ToggleUnlit()
        {
            unlit = !unlit;
            
            if (unlit)
                Shader.SetGlobalFloat("_Diffuse", 0);
            if (!unlit)
                Shader.SetGlobalFloat("_Diffuse", 1);
            
        }
        
        [MenuItem("Shader Controls/Diminished Lighting Toggle _%I")]
        public static void ToggleDiminished()
        {
            toggleDiminishedlighting = !toggleDiminishedlighting;
            
            if(toggleDiminishedlighting)
                Shader.SetGlobalFloat("_GlobalDisableDimLighting", 1);
            if(!toggleDiminishedlighting)
                Shader.SetGlobalFloat("_GlobalDisableDimLighting", 0);
        }

        [MenuItem("Shader Controls/Test")]
        public static void TextureStuff()
        {
            var path = "";

            if (Selection.objects.Length>1)
            {
                Debug.Log(">1");
                for (int i = 0; i < Selection.objects.Length; i++)
                {
                    var t = Selection.objects[i] as Texture2D;
                    CreateTextureMaterial(t);
                }
                /*Debug.Log("handle array");
                var textureArray = Selection.objects as IEnumerable;
                foreach (Texture2D tex in textureArray)
                {
                    CreateTextureMaterial(tex);
                }
                // Iterate all to find textures
                if (Selection.objects.GetType() == typeof(IEnumerable))
                {

                }*/
            }
            
            if (!Selection.objects.Any())
            {
                Debug.Log("1/0 selected");
            }
            
            return;
            var obj  = Selection.activeObject;
            if (obj == null) path = "Assets";
            else path             = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (path.Length > 0)
            {
                if (Directory.Exists(path))
                {
                    Debug.Log("Directory/Not a texture");
                }
                else
                {
                    /*Debug.Log(path);

                    if (obj is Texture2D texture2d)
                    {
                        Debug.Log("CreateTextures");
                        CreateTextureMaterial(texture2d);
                    }*/

                    if (obj.GetType() == typeof(IEnumerable))
                    {
                        Debug.Log("handle array");
                        IEnumerable textureArray = obj as IEnumerable;
                        foreach (Texture2D tex in textureArray)
                        {
                            CreateTextureMaterial(tex);
                        }
                    }
                    Debug.Log("File");
                }
            }
            else
            {
                Debug.Log("Not in assets folder");
            }
        }

        static void CreateTextureMaterial(Texture2D texture)
        {
            Assert.IsNotNull(texture);
            Debug.Log(AssetDatabase.GetAssetPath(texture));
            var assetPath = AssetDatabase.GetAssetPath(texture);
            Debug.Log(assetPath);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath));
            

            // get material name + path
            var materialName = Path.GetFileNameWithoutExtension(assetPath);
            var materialPath = TryGetPath(Path.GetDirectoryName(assetPath));
            
            // get shader
            var material = new Material(Shader.Find("RetroJunk")); // was JitterGI
            
            // create actual material asset
            AssetDatabase.CreateAsset(material, materialPath + "\\" + materialName + ".mat");
            
            // reload material
            material = (Material)AssetDatabase.LoadAssetAtPath(materialPath + "\\" + materialName + ".mat", typeof(Material));
            
            Assert.IsNotNull(texture, "texture != null");
            Assert.IsNotNull(material, "material != null");
            Assert.IsTrue(material.HasProperty(_baseMap), "material.HasProperty(_baseMap)");
            
            // set material asset texture with texture asset
            material.SetTexture(_baseMap, texture);
        }

        static string TryGetPath(string assetPath)
        {
            //var path = System.IO.Directory.GetCurrentDirectory() + "\\" + "Materials";
            //var p = AssetDatabase.GetAssetPath(g) + "\\" + "Materials";
            var p = assetPath + "\\" + "Materials";

            
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }

            return p;
        }
        
        private static readonly int _baseMap = Shader.PropertyToID("_BaseMap");
        
        
        
        
        
        
        
    }
}

#endif