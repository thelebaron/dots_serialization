﻿
using System.Collections.Generic;
using System.Linq;
using PrefabSerialization.Editor;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Unity.Entities.Editor
{
    [InitializeOnLoad]
    class SaveEntityIdHeader
    {
        private static bool overrideEnabled;
        
        static SaveEntityIdHeader()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += DisplayPrefabIdHeaderCallBack;
        }

        static class EntityPrefabHeaderTextStrings
        {
            public const string PrefabEntity = "SaveEntity";
            public const string ConvertByAncestor = "(by ancestor)";
            public const string ConvertByScene = "(by scene)";
            public const string StopConvertToEntityInHierarchy = "(" + nameof(StopConvertToEntity) + " in hierarchy)";
            public const string ConvertAndInjectInParents = "(ConvertAndInject mode in parents)";
        }

        enum ToggleState
        {
            AllOn, Mixed, AllOff
        }

        static void DisplayPrefabIdHeaderCallBack(UnityEditor.Editor editor)
        {
            var selectedGameObject = editor.target as GameObject;

            if (selectedGameObject == null)
                return;

            
            using (new EditorGUILayout.HorizontalScope(EditorStyles.largeLabel))
            {
                // convert icon
                //EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(EntityPrefabHeaderTextStrings.PrefabEntity, EditorIcons.EntityPrefab), EditorStyles.label, GUILayout.MaxWidth(130));
                
                // Multi-selection
                List<GameObject> TargetsList = new List<GameObject>();
                TargetsList.Clear();
                TargetsList.AddRange(editor.targets.OfType<GameObject>());

                List<Component> componentToRemoveFromGOList = new List<Component>();
                List<GameObject> gameObjectToAddComponentList = new List<GameObject>();
                
                /*foreach (var gameObject in TargetsList)
                {
                    var convertToEntityComponent = gameObject.GetComponent<ConvertToEntity>();

                    if (convertToEntityComponent != null)
                    {
                        gameObjectToAddComponentList.Add(gameObject);
                    }
                    else
                    {
                        componentToRemoveFromGOList.Add(convertToEntityComponent);
                    }
                }*/
                
                if(selectedGameObject.TryGetComponent<SaveEntityToDisk>(out var serializeable))
                {
                    EditorGUILayout.BeginHorizontal();
                    // convert icon
                    EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(EntityPrefabHeaderTextStrings.PrefabEntity, EditorIcons.EntityPrefab), EditorStyles.label, GUILayout.MaxWidth(130));
                    
                    if(serializeable.guid.Length.Equals(0))
                        RegenerateId(serializeable);
                    
                    //EditorGUILayout.LabelField("", GUILayout.MaxWidth(15));
                    EditorGUILayout.LabelField("id: ", GUILayout.MaxWidth(15));
                    EditorGUILayout.LabelField(serializeable.guid, EditorStyles.boldLabel, GUILayout.MaxWidth(120));

                    
                    //EditorGUI.Toggle()
                    overrideEnabled = EditorGUILayout.Foldout(overrideEnabled, "override");
                    if(overrideEnabled)
                    {
                        //overrideEnabled = EditorGUILayout.BeginToggleGroup ("Override", overrideEnabled);
                        if (GUILayout.Button("New Id", GUILayout.MaxWidth(65)))
                        {
                            RegenerateId(serializeable);
                        }
                    
                    }
                    //EditorGUILayout.EndToggleGroup();
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    
                    EditorGUILayout.EndHorizontal();

                    
                    


                }
                
                
                
                // Converted by ConvertToEntity.
                /*using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent(""), GameObjectConversionEditorUtility.IsConverted(GameObjectConversionEditorUtility.GetGameObjectConversionResultStatus(selectedGameObject)));
                    if (changeScope.changed)
                    {
                        if (selectedGameObject.GetComponent<ConvertToEntity>() == null)
                        {
                            Undo.AddComponent<ConvertToEntity>(selectedGameObject);
                        }
                        else
                        {
                            Undo.DestroyObjectImmediate(selectedGameObject.GetComponent<ConvertToEntity>());
                        }
                    }
                }*/
                
                
                
                
                
                
                
                
                
                
                
                
                /*
                if (TargetsList.Count > 1)
                {
                    foreach (var gameObject in TargetsList)
                    {
                        var convertToEntityComponent = gameObject.GetComponent<ConvertToEntity>();

                        if (convertToEntityComponent == null)
                        {
                            gameObjectToAddComponentList.Add(gameObject);
                        }
                        else
                        {
                            componentToRemoveFromGOList.Add(convertToEntityComponent);
                        }
                    }

                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        var componentToRemoveFromGOListLength = componentToRemoveFromGOList.Count;
                        var gameObjectToAddComponentListLength = gameObjectToAddComponentList.Count;

                        var toggleState = ToggleState.AllOn;

                        if (componentToRemoveFromGOListLength > 0 && gameObjectToAddComponentListLength > 0)
                        {
                            toggleState = ToggleState.Mixed;
                        }
                        else if (componentToRemoveFromGOListLength == 0 && gameObjectToAddComponentListLength > 0)
                        {
                            toggleState = ToggleState.AllOff;
                        }

                        var oldShowMixedValue = EditorGUI.showMixedValue;
                        EditorGUI.showMixedValue = toggleState == ToggleState.Mixed;
                        EditorGUILayout.Toggle(toggleState == ToggleState.AllOn);
                        EditorGUI.showMixedValue = oldShowMixedValue;

                        if (changeScope.changed)
                        {
                            switch (toggleState)
                            {
                                case ToggleState.AllOn:
                                {
                                    foreach (var component in componentToRemoveFromGOList)
                                    {
                                        Undo.DestroyObjectImmediate(component);
                                    }
                                    return;
                                }
                                case ToggleState.Mixed:
                                {
                                    foreach (var gameObject in gameObjectToAddComponentList)
                                    {
                                        Undo.AddComponent<ConvertToEntity>(gameObject);
                                    }
                                    return;
                                }
                                case ToggleState.AllOff:
                                {
                                    foreach (var gameObject in gameObjectToAddComponentList)
                                    {
                                        Undo.AddComponent<ConvertToEntity>(gameObject);
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    return;
                }
                else
                
                {
                    var conversionStatus = GameObjectConversionEditorUtility.GetGameObjectConversionResultStatus(selectedGameObject);
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        switch (conversionStatus)
                        {
                            case GameObjectConversionResultStatus.ConvertedBySubScene:
                            {
                                EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent(ConvertToEntityHeaderTextStrings.ConvertByScene), true);
                                return;
                            }

                            case GameObjectConversionResultStatus.NotConvertedByStopConvertToEntityComponent:
                            {
                                EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent(ConvertToEntityHeaderTextStrings.StopConvertToEntityInHierarchy), false);
                                return;
                            }

                            case GameObjectConversionResultStatus.NotConvertedByConvertAndInjectMode:
                            {
                                EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent(ConvertToEntityHeaderTextStrings.ConvertAndInjectInParents), false);
                                return;
                            }

                            case GameObjectConversionResultStatus.ConvertedByAncestor:
                            {
                                EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent(ConvertToEntityHeaderTextStrings.ConvertByAncestor), true);
                                return;
                            }
                        }
                    }
                }

                // Converted by ConvertToEntity.
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent(""), GameObjectConversionEditorUtility.IsConverted(GameObjectConversionEditorUtility.GetGameObjectConversionResultStatus(selectedGameObject)));
                    if (changeScope.changed)
                    {
                        if (selectedGameObject.GetComponent<ConvertToEntity>() == null)
                        {
                            Undo.AddComponent<ConvertToEntity>(selectedGameObject);
                        }
                        else
                        {
                            Undo.DestroyObjectImmediate(selectedGameObject.GetComponent<ConvertToEntity>());
                        }
                    }
                }*/
            }
        }

        private static void RegenerateId(SaveEntityToDisk serializeable)
        {
            serializeable.guid = PrefabSerializeUtility.UniqueGuid();

            // If prefab is selected in scene
            if (PrefabUtility.IsPartOfPrefabInstance(serializeable.gameObject))
            {
                //Debug.Log("IsPartOfPrefabInstance");
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(serializeable);
                prefab.guid = serializeable.guid;
                PrefabUtility.SavePrefabAsset(prefab.gameObject);
            }

            // If prefab is selected from project overview
            if (PrefabUtility.IsPartOfPrefabAsset(serializeable.gameObject))
            {
                //Debug.Log("IsPartOfPrefabAsset");
                PrefabUtility.SavePrefabAsset(serializeable.gameObject);
            }

            /*if (PrefabUtility.IsPartOfNonAssetPrefabInstance(serializeable.gameObject))
                        {
                            //Debug.Log("IsPartOfNonAssetPrefabInstance");
                            //PrefabUtility.SavePrefabAsset(serializeable.gameObject);
                        }
                        if (PrefabUtility.IsPartOfModelPrefab(serializeable.gameObject))
                        {
                            //Debug.Log("IsPartOfModelPrefab");
                            //PrefabUtility.SavePrefabAsset(serializeable.gameObject);
                        }
                        if (PrefabUtility.IsDisconnectedFromPrefabAsset(serializeable.gameObject))
                        {
                            //Debug.Log("IsDisconnectedFromPrefabAsset");
                            //PrefabUtility.SavePrefabAsset(serializeable.gameObject);
                        }*/

            // If prefab is in stage mode
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                //Debug.Log("IsStage?");
                var prefabPath = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
                var prefabRoot = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
        }
    }
}