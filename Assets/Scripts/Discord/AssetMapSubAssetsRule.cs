#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

class AssetMapSubAssetsRule : AnalyzeRule
{
    public override bool CanFix { get; set; } = true;

    private Type[] allowedTypes = new[]
    {
        typeof(Material),
        typeof(Mesh),
        typeof(GameObject) //need gameobject for mesh, so dumb
    };

    public override void FixIssues(AddressableAssetSettings settings)
    {
        foreach (var addressableAssetGroup in settings.groups)
        {
            foreach (var addressableAssetEntry in addressableAssetGroup.entries)
            {
                var dependancies =
                    AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(addressableAssetEntry.TargetAsset), true);

                foreach (var dependancy in dependancies)
                {
                    var type = AssetDatabase.GetMainAssetTypeAtPath(dependancy);

                    if (!allowedTypes.Contains(type))
                        continue;

                    var guid = AssetDatabase.AssetPathToGUID(dependancy);
                    var assetEntry = addressableAssetGroup.GetAssetEntry(AssetDatabase.AssetPathToGUID(dependancy));
                    if (assetEntry == null)
                    {
                        settings.CreateOrMoveEntry(guid, addressableAssetGroup, false, false);
                    }
                }
            }
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
    }

    public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
    {
        var results = new List<AnalyzeResult>();
        foreach (var addressableAssetGroup in settings.groups)
        {
            foreach (var addressableAssetEntry in addressableAssetGroup.entries)
            {
                var dependancies =
                    AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(addressableAssetEntry.TargetAsset), true);

                foreach (var dependancy in dependancies)
                {
                    if (dependancy.Contains("Character_Dummy_Male_01"))
                    {
                        Debug.Log(dependancy);
                        Debug.Log(AssetDatabase.GetMainAssetTypeAtPath(dependancy));
                    }
                    var type = AssetDatabase.GetMainAssetTypeAtPath(dependancy);

                    if (!allowedTypes.Contains(type))
                        continue;

                    var assetEntry = addressableAssetGroup.GetAssetEntry(AssetDatabase.AssetPathToGUID(dependancy));
                    if (assetEntry == null)
                    {
                        results.Add(new AnalyzeResult()
                        {
                            severity = MessageType.Warning,
                            resultName =
                                $"Ohhh noessss, Dependancy {dependancy} for {addressableAssetEntry.MainAsset.name} isnt addressable too"
                        });
                    }
                }
            }
        }

        return results;
    }
}


[InitializeOnLoad]
class RegisterAssetMapSubAssetsRule
{
    static RegisterAssetMapSubAssetsRule()
    {
        AnalyzeWindow.RegisterNewRule<AssetMapSubAssetsRule>();
    }
}

#endif