using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[InitializeOnLoad]
public class FavouriteBarIcons
{
    public static string emptyStar = "X";
    public static string star = "*";
    public static string ping = "•";
    public static List<string> favoritedGUIDs = new List<string>();

    static FavouriteBarIcons()
    {
        var data = PlayerPrefs.GetString("favbar-data", defaultValue: "");
        var guids = data.Split(',').Where(element => element.Length > 0);
        favoritedGUIDs.Clear();
        favoritedGUIDs.AddRange(guids);

        EditorApplication.projectWindowItemOnGUI += DrawProjectItem;
    }

    private static void DrawProjectItem(string guid, Rect selectionRect)
    {
        var frame = new Rect(selectionRect);
        frame.x += frame.width - 20f;
        frame.width = 18f;

        if (favoritedGUIDs.Contains(guid))
        {
            if (GUI.Button(frame, star, GUI.skin.label))
            {
                RemoveFavorite(guid);
            }
        }
        else if (selectionRect.Contains(Event.current.mousePosition))
        {
            if (GUI.Button(frame, emptyStar, GUI.skin.label))
            {
                AddFavorite(guid);
            }
        }
    }

    public static void AddFavorite(string guid)
    {
        favoritedGUIDs.Add(guid);
        var data = string.Join(",", favoritedGUIDs.ToArray());
        PlayerPrefs.SetString("favbar-data", data);
        PlayerPrefs.Save();

        if (FavouriteBarEditor.IsOpen)
        {
            var window = EditorWindow.GetWindow<FavouriteBarEditor>(title: "Favorites", focus: false);
            window.Repaint();
        }
    }

    public static void RemoveFavorite(string guid)
    {
        favoritedGUIDs.Remove(guid);
        var data = string.Join(",", favoritedGUIDs.ToArray());
        PlayerPrefs.SetString("favbar-data", data);
        PlayerPrefs.Save();

        if (FavouriteBarEditor.IsOpen)
        {
            var window = EditorWindow.GetWindow<FavouriteBarEditor>(title: "Favorites", focus: false);
            window.Repaint();
        }
        EditorApplication.RepaintProjectWindow();
    }
}