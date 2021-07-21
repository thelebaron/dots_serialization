using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// see http://pixelpirate.github.io/UnityEditorExtentionsBook/
/// </summary>
public class FavouriteBarEditor: EditorWindow
{
    private string searchString = "";
    private string filter;

    public static bool IsOpen
    {
        get;
        private set;
    }

    List<string> Types(List<string> GUIDs)
    {
        List<string> types = new List<string>();
        foreach (var GUID in GUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(GUID);
            var elementObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            var typeName = elementObject.GetType().Name;

            if (!types.Contains(typeName))
            {
                types.Add(typeName);
            }
        }

        return types;
    }

    private void HandleMenuFunction(object userData)
    {
        filter = (string)userData;
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        var typeNames = Types(FavouriteBarIcons.favoritedGUIDs);
        if (GUILayout.Button("Filter", GUI.skin.FindStyle("ToolbarButton")))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("All"), on: string.IsNullOrEmpty(filter), func: HandleMenuFunction, userData: null);
            menu.AddSeparator(string.Empty);
            foreach (var name in typeNames)
            {
                menu.AddItem(new GUIContent(name), on: name == filter, func: HandleMenuFunction, userData: name);
            }
            menu.ShowAsContext();
        }

        GUILayout.FlexibleSpace();
        searchString = GUILayout.TextField(searchString,
                                           GUI.skin.FindStyle("ToolbarSeachTextField"),
                                           GUILayout.MinWidth(100f));
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
        {
            searchString = "";
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();

        var deletionElements = new List<string>();

        foreach (var element in FavouriteBarIcons.favoritedGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(element);
            var elementObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

            if ((!elementObject.name.ToLowerInvariant().Contains(searchString.ToLowerInvariant())
                && !string.IsNullOrEmpty(searchString))
                ||
                (!string.IsNullOrEmpty(filter)
                && elementObject.GetType().Name != filter))
            {
                continue;
            }

            var content = EditorGUIUtility.ObjectContent(elementObject, elementObject.GetType());

            var bounds = GUILayoutUtility.GetRect(content,
                                                  GUI.skin.label,
                                                  GUILayout.MaxHeight(16f));
            var buttonFrame = new Rect(bounds);
            buttonFrame.width -= 40f;

            if (Selection.activeObject == elementObject)
            {
                GUI.Box(bounds, "", GUI.skin.box);
            }

            if (GUI.Button(buttonFrame, content, GUI.skin.label))
            {
                Selection.activeObject = elementObject;
                EditorGUIUtility.PingObject(elementObject);
            }

            var starRect = new Rect(bounds);
            starRect.x += starRect.width - 20f;
            starRect.width = 18f;

            if (GUI.Button(starRect, FavouriteBarIcons.emptyStar))
            {
                deletionElements.Add(element);
            }

            var pingRect = new Rect(bounds);
            pingRect.x += pingRect.width - 40f;
            pingRect.width = 18f;

            if (GUI.Button(pingRect, FavouriteBarIcons.ping))
            {
                EditorGUIUtility.PingObject(elementObject);
            }
        }

        foreach (var element in deletionElements)
        {
            FavouriteBarIcons.RemoveFavorite(element);
        }
    }

    private void OnSelectionChange()
    {
        if (FavouriteBarEditor.IsOpen)
        {
            var window = EditorWindow.GetWindow<FavouriteBarEditor>(title: "Favorites", focus: false);
            window.Repaint();
        }
    }

    [MenuItem("Tools/Favorites")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<FavouriteBarEditor>(title: "Favorites");
        window.Show();
    }

    private void OnEnable()
    {
        IsOpen = true;
    }

    private void OnDisable()
    {
       IsOpen = false;
    }
}