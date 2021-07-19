
using System.Collections.Generic;
using System.Linq;
using PrefabSerialization.Editor;
using Unity.Scenes.Editor;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.Graphs;
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
            public const string NotPrefab = "Entity is not a prefab";
            public const string NotConverted = "No conversion";
            public const string NotAssetNoConversion = "Not a prefab asset and no entity conversion";
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
                var TargetsList = new List<GameObject>();
                TargetsList.Clear();
                TargetsList.AddRange(editor.targets.OfType<GameObject>());
                
                var goList = new List<GameObject>();
                var componentToRemoveFromGOList = new List<Component>();
                var gameObjectToAddComponentList = new List<GameObject>();
                

                if (TargetsList.Count > 1)
                {
                    //DrawToggleMultipleEntries(TargetsList, goList, gameObjectToAddComponentList, componentToRemoveFromGOList);
                    return;
                }

                using (new EditorGUI.DisabledGroupScope(true))
                {
                    // make sure not prefab stage
                    if (PrefabStageUtility.GetCurrentPrefabStage() == null)
                    {
                        if (!PrefabUtility.IsPartOfAnyPrefab(selectedGameObject))
                        {
                            if (EditorEntityScenes.IsEntitySubScene(selectedGameObject.scene) || selectedGameObject.GetComponent<ConvertToEntity>() == null)
                            {
                                EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(EntityPrefabHeaderTextStrings.NotConverted, EditorIcons.EntityPrefab),
                                    EditorStyles.label);
                                return;
                            }
                            else
                            {
                                EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(EntityPrefabHeaderTextStrings.NotPrefab, EditorIcons.EntityPrefab),
                                    EditorStyles.label);
                                return;
                            }
                        }

                        if (!PrefabUtility.IsPartOfPrefabAsset(selectedGameObject) && selectedGameObject.GetComponent<ConvertToEntity>() == null)
                        {
                            EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(EntityPrefabHeaderTextStrings.NotAssetNoConversion, EditorIcons.EntityPrefab),
                                EditorStyles.label);
                            return;
                        }
                    }
                }

                EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(EntityPrefabHeaderTextStrings.PrefabEntity, EditorIcons.EntityPrefab), EditorStyles.label,
                    GUILayout.MaxWidth(130));


                bool hasSaveComponent = SaveEntityUtility.IsSaved(selectedGameObject);
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    if (hasSaveComponent)
                    {
                        //EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent(""), SaveEntityUtility.IsSaved(selectedGameObject));
                        EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent("Saved"), true, GUILayout.MaxWidth(130));
                    }
                    else
                    {
                        EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent("Not saved"), false, GUILayout.MaxWidth(130));
                    }
                    
                    if (changeScope.changed)
                    {
                        if (selectedGameObject.GetComponent<SaveEntityToDisk>() == null)
                        {
                            Undo.AddComponent<SaveEntityToDisk>(selectedGameObject);
                        }
                        else
                        {
                            Undo.DestroyObjectImmediate(selectedGameObject.GetComponent<SaveEntityToDisk>());
                        }
                    }
                }

                if (hasSaveComponent)
                {
                    //DrawUILine(UnityEngine.Color.gray);
                    //if(hasSaveComponent)
                    DrawSaveIdInfo(selectedGameObject);
                    DetectMissingPrefabComponent(selectedGameObject);
                }
            }
        }

        private static void DetectMissingPrefabComponent(GameObject selectedGameObject)
        {            
            if (PrefabUtility.IsPartOfPrefabInstance(selectedGameObject))
            {
                var addedComponents = PrefabUtility.GetAddedComponents(selectedGameObject);
                foreach (var component in addedComponents)
                {
                    if (component.instanceComponent.GetType() == typeof(SaveEntityToDisk))
                    {
                        EditorGUILayout.HelpBox("Warning, Prefab: "+selectedGameObject.name+" has unapplied " + component.instanceComponent.GetType(), MessageType.Error);
                        
                        break;
                    }
                }
            }

        }

        private static void DrawToggleMultipleEntries(List<GameObject> TargetsList, List<GameObject> goList, List<GameObject> gameObjectToAddComponentList, List<Component> componentToRemoveFromGOList)
        {
            EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(EntityPrefabHeaderTextStrings.PrefabEntity, EditorIcons.EntityPrefab), EditorStyles.label,
                GUILayout.MaxWidth(130));

            foreach (var gameObject in TargetsList)
            {
                var convertToEntityComponent = gameObject.GetComponent<ConvertToEntity>();

                // must be slated for conversion
                if (convertToEntityComponent != null)
                {
                    goList.Add(gameObject);

                    var saveEntityComponent = gameObject.GetComponent<SaveEntityToDisk>();

                    if (saveEntityComponent == null)
                    {
                        gameObjectToAddComponentList.Add(gameObject);
                    }
                    else
                    {
                        componentToRemoveFromGOList.Add(saveEntityComponent);
                    }
                }
            }

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                var componentToRemoveFromGOListLength  = componentToRemoveFromGOList.Count;
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
                                Undo.AddComponent<SaveEntityToDisk>(gameObject);
                            }

                            return;
                        }
                        case ToggleState.AllOff:
                        {
                            foreach (var gameObject in gameObjectToAddComponentList)
                            {
                                Undo.AddComponent<SaveEntityToDisk>(gameObject);
                            }

                            return;
                        }
                    }
                }
            }

            return;
        }

        private static void DrawSaveIdInfo(GameObject selectedGameObject)
        {
            if (selectedGameObject.TryGetComponent<SaveEntityToDisk>(out var serializeable))
            {
                // EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon(EntityPrefabHeaderTextStrings.PrefabEntity, EditorIcons.EntityPrefab), EditorStyles.label, GUILayout.MaxWidth(130));

                EditorGUILayout.BeginHorizontal();
                // convert icon

                if (serializeable.guid==null || serializeable.guid.Length.Equals(0))
                {
                    RegenerateId(serializeable);
                    return;
                }

                //EditorGUILayout.LabelField("", GUILayout.MaxWidth(15));
                EditorGUILayout.LabelField("id: ", GUILayout.MaxWidth(15));
                EditorGUILayout.LabelField(serializeable.guid, EditorStyles.boldLabel, GUILayout.MaxWidth(120));

                if (GUILayout.Button("New Id", GUILayout.MaxWidth(65)))
                {
                    RegenerateId(serializeable);
                }
                /*
                //EditorGUI.Toggle()
                overrideEnabled = EditorGUILayout.Foldout(overrideEnabled, "override");
                if (overrideEnabled)
                {
                    //overrideEnabled = EditorGUILayout.BeginToggleGroup ("Override", overrideEnabled);
                }

                
                //EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndFoldoutHeaderGroup();*/

                EditorGUILayout.EndHorizontal();
            }
        }

        private static void RegenerateId(SaveEntityToDisk serializeable)
        {
            serializeable.guid = SaveEntityUtility.UniqueGuid();

            // If prefab is selected in scene
            if (PrefabUtility.IsPartOfPrefabInstance(serializeable.gameObject))
            {
                //Debug.Log("IsPartOfPrefabInstance");
                //var prefab = PrefabUtility.GetCorrespondingObjectFromSource(serializeable);
                PrefabUtility.ApplyPrefabInstance(serializeable.gameObject, InteractionMode.UserAction);
            }

            // If prefab is selected from project overview
            if (PrefabUtility.IsPartOfPrefabAsset(serializeable.gameObject))
            {
                //Debug.Log("IsPartOfPrefabAsset");
                PrefabUtility.SavePrefabAsset(serializeable.gameObject);
            }

            // If prefab is in stage mode
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                //Debug.Log("IsStage?");
                var prefabPath = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
                var prefabRoot = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
        }
        
        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
            r.height =  thickness;
            r.y      += padding/2;
            r.x      -= 2;
            r.width  += 6;
            EditorGUI.DrawRect(r, color);
        }

    }
}
