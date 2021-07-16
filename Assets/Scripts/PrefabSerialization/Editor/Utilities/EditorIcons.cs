using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PrefabSerialization.Editor
{
    static class EditorIcons
    {
        const string k_IconsLightDirectory = PrefabSerialization.Editor.Constants.EditorTempResourcesPath + "icons/light";
        const string k_IconsDarkDirectory = PrefabSerialization.Editor.Constants.EditorTempResourcesPath + "icons/dark";

        public static Texture2D EntityPrefab { get; private set; }
        public static Texture2D RuntimeComponent { get; private set; }
        public static Texture2D Remove { get; private set; }
        public static Texture2D RoundedCorners { get; private set; }
        public static Texture2D Entity { get; private set; }
        public static Texture2D EntityGroup { get; private set; }
        public static Texture2D Filter { get; private set; }
        public static Texture2D System { get; private set; }

        static EditorIcons()
        {
            LoadIcons();
        }

        static void LoadIcons()
        {
            EntityPrefab = LoadIcon("EntityPrefab/" + nameof(EntityPrefab));
            
            /*RuntimeComponent = LoadIcon("RuntimeComponent/" + nameof(RuntimeComponent));
            Remove = LoadIcon("Remove/" + nameof(Remove));
            RoundedCorners = LoadIcon("RoundedCorners/" + nameof(RoundedCorners));
            Entity = LoadIcon("Entity/" + nameof(Entity));
            EntityGroup = LoadIcon("EntityGroup/" + nameof(EntityGroup));
            Filter = LoadIcon("Filter/" + nameof(Filter));
            System = LoadIcon("System/" + nameof(System));*/
        }

        /// <summary>
        /// Workaround for `EditorGUIUtility.LoadIcon` not working with packages. This can be removed once it does
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static Texture2D LoadIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var iconsDirectory = k_IconsLightDirectory;
            if (EditorGUIUtility.isProSkin)
            {
                iconsDirectory = k_IconsDarkDirectory;
            }

            // Try to use high DPI if possible
            if (true)//(Bridge.GUIUtility.pixelsPerPoint > 1.0)
            {
                var texture = LoadIconTexture($"{iconsDirectory}/{name}@2x.png");

                if (null != texture)
                {
                    return texture;
                }
            }

            // Fallback to low DPI if we couldn't find the high res or we are on a low res screen
            return LoadIconTexture($"{iconsDirectory}/{name}.png");
        }

        static Texture2D LoadIconTexture(string path)
        {
            var texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));

            /*if (texture != null &&
                !Mathf.Approximately(texture.GetPixelsPerPoint(), (float)InternalsHelpers.pixelsPerPoint()) &&
                !Mathf.Approximately((float)InternalsHelpers.pixelsPerPoint() % 1f, 0.0f))
            {
                texture.filterMode = FilterMode.Bilinear;
            }*/
            texture.filterMode = FilterMode.Bilinear;
            
            return texture;
        }
    }
    
    
    // because the entities version is internal
    // This helper is here to encapsulate the access to `pixelsPerPoint` private property on Texture2D.
    // We need to access this property to enable bilinear filter mode on the texture when its pixel
    // per point is different from the editor ppp.
    // TODO: @antoineb remove this when ppp is public or thanks to a cleaner solution
    static class InternalsHelpers
    {
        static PropertyInfo s_TexturePixelsPerPoint;

        static InternalsHelpers()
        {
            s_TexturePixelsPerPoint = typeof(Texture2D).GetProperty("pixelsPerPoint", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public static float GetPixelsPerPoint(this Texture2D @this)
        {
            var v = s_TexturePixelsPerPoint?.GetValue(@this);
            if (v == null)
                return 1.0f;

            return (float)v;
        }
        
        public static double pixelsPerPoint()
        {
            FieldInfo field = typeof(GUIUtility).GetField("pixelsPerPoint", BindingFlags.NonPublic | BindingFlags.Static);

            var lookup = field.GetValue(null);

            float result = (float)lookup;
                
            /* MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
             if (disableMethod != null)
                 disableMethod.Invoke(defaultEditor,null);
             
             Type      myTypeA     = typeof(GUIUtility);
             FieldInfo myFieldInfo = myTypeA.GetField("pixelsPerPoint");
             FieldInfo myFieldInfo1 = myTypeA.GetField("field", BindingFlags.NonPublic | BindingFlags.Static);

             Console.WriteLine("The value of the public field is: '{0}'",
                 myFieldInfo.GetValue(myFieldObjectA));
             Console.WriteLine("The value of the private field is: '{0}'",
                 myFieldInfo1.GetValue(myFieldObjectB));
             */
            return result;
        }
    }
}
