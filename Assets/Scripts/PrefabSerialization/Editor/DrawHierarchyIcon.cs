using UnityEditor;
using UnityEngine;

namespace PrefabSerialization.Editor
{
    /// <summary>
    /// Draws a Comment Icon on GameObjects in the Hierarchy that contain the Comment component.
    /// </summary>
    [InitializeOnLoad]
    public class CommentHierarchyIcon
    {
        private static readonly Texture2D Icon;
        private static readonly Texture2D ParentIcon;

        static CommentHierarchyIcon()
        {
            Icon       = AssetDatabase.LoadAssetAtPath("Assets/Gizmos/SaveEntityToDisk icon.png", typeof(Texture2D)) as Texture2D;
            ParentIcon = AssetDatabase.LoadAssetAtPath("Assets/Gizmos/SaveEntityToDisk icon.png", typeof(Texture2D)) as Texture2D;

            if (Icon == null)
            {
                return;
            }

            EditorApplication.hierarchyWindowItemOnGUI += DrawIconOnWindowItem;
            EditorApplication.projectWindowItemOnGUI   += DrawProjectItem;
        }

        private static void DrawProjectItem(string guid, Rect selectionRect)
        {
            var frame = new Rect(selectionRect);
            frame.x     += frame.width - 20f;
            frame.width =  18f;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(".prefab"))
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go.TryGetComponent<SaveEntityToDisk>(out var serializeable))
                {
                    DrawIcon(Icon, frame, "df");
                }
            }

            //Debug.Log(path);
        }

        private static void DrawIconOnWindowItem(int instanceID, Rect rect)
        {
            if (Icon == null)
            {
                return;
            }

            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (gameObject == null)
            {
                return;
            }

            var comments = gameObject.GetComponentsInChildren<SaveEntityToDisk>();
            foreach (var comment in comments)
            {
                DrawIcon(ParentIcon, rect, string.Empty);
            }

            //comments.Free();
        }

        private static void DrawIcon(Texture2D icon, Rect rect, string tooltip)
        {
            float iconWidth = 15;
            EditorGUIUtility.SetIconSize(new Vector2(iconWidth, iconWidth));
            var padding        = new Vector2(20, 0);
            var iconDrawRect   = new Rect(rect.xMax - (iconWidth + padding.x), rect.yMin, rect.width, rect.height);
            var iconGUIContent = new GUIContent(icon, tooltip);
            EditorGUI.LabelField(iconDrawRect, iconGUIContent);
            EditorGUIUtility.SetIconSize(Vector2.zero);
        }
    }
}