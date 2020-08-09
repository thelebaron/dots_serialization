﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Scenes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public struct MyData
{
    public string     _name;
    public string     _uniqueID;
    public string     _description;
    public int        _vendorPrice;
    public bool       _moddable;
    public float      _RecoilCompensation;
    public bool[]     _allowedFireModes;
    //public Sprite     _icon;
    //public GameObject _prefab;
}

public class SerializeComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public bool saveOnStart = true;
    public bool useYaml;
    public MyData myData;
    public object[] serialized_objects;
    public List<GameObject> SavedGameObjects;
    
    
    public List<GameObject> HiddenGameObjects;
    
    public void Serialize(XmlWriter _w)
    {
        _w.WriteStartElement("Item");
        _w.WriteAttributeString("Name", myData._name);
        _w.WriteAttributeString("UniqueID", myData._uniqueID.ToString());                                  
        _w.WriteAttributeString("Description", myData._description);
        _w.WriteAttributeString("VendorPrice", myData._vendorPrice.ToString());
        _w.WriteAttributeString("Moddable", myData._moddable.ToString());
        _w.WriteAttributeString("RecoilCompensation", myData._RecoilCompensation.ToString());
        _w.WriteAttributeString("Semi-Auto", myData._allowedFireModes[0].ToString());
        _w.WriteAttributeString("Burst", myData._allowedFireModes[1].ToString());
        _w.WriteAttributeString("Full-Auto", myData._allowedFireModes[2].ToString());

        _w.WriteEndElement();
    }

    public void Deserialize(XmlElement _e)
    {
        //myData._itemType = (object)int.Parse(_e.GetAttribute("ItemType"));

        myData._name = _e.GetAttribute("Name");
        myData._uniqueID = _e.GetAttribute("UniqueID");          
        myData._description = _e.GetAttribute("Description");
        myData._vendorPrice = int.Parse(_e.GetAttribute("VendorPrice"));
        //myData._icon = AssetDatabase.LoadAssetAtPath<Sprite>(_e.GetAttribute("Icon"));
        //myData._prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_e.GetAttribute("Prefab"));
        myData._moddable = bool.Parse(_e.GetAttribute("Moddable"));
        myData._RecoilCompensation = float.Parse(_e.GetAttribute("RecoilCompensation"));
        myData._allowedFireModes[0] = bool.Parse(_e.GetAttribute("Semi-Auto"));
        myData._allowedFireModes[1] = bool.Parse(_e.GetAttribute("Burst"));
        myData._allowedFireModes[2] = bool.Parse(_e.GetAttribute("Full-Auto"));
        //_ammoType = (AmmoTypeEnum)int.Parse(_e.GetAttribute("AmmoType"));
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        
    }


    private void Start()
    {
        
    }

    private void Update()
    {
        HiddenGameObjects = new List<GameObject>();
        var hidden = FindObjectsOfType<GameObject>();
        for (int i = 0; i < hidden.Length; i++)
        {
            if(hidden[i].hideFlags != HideFlags.None)
                HiddenGameObjects.Add(hidden[i]);
        }
    }
    
    void OnGUI()
    {
        // pos x,y, size x,y
        if (GUI.Button(new Rect(15, 50, 100, 35), "Save"))
        {
            TogglePhysicsSystemForSaving(false);
            SaveData();
            TogglePhysicsSystemForSaving(true);

        }

        if (GUI.Button(new Rect(15, 100, 100, 35), "Load"))
        {
            LoadData();
        }

        if (GUI.Button(new Rect(15, 150, 100, 35), "Destroy All Entities"))
        {
            if(World.All.Count<1)
                return;
            
            em.CompleteAllJobs();
            
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        }
        
    }
    
    const string unityobjectsAsset = "Assets/UnityObjects.asset";
    string saveLocation, _FileName, _FileNameJSON, _FileNameYAML;
    private EntityManager em;
    
    // old
    
    private void OnEnable()
    {
        saveLocation = Application.dataPath + "\\" + "Saves"; 
        _FileName     = "SaveData.xml";
        _FileNameJSON = "SaveData.json";
        _FileNameYAML = "World.yaml";
        
        if (World.All.Count == 0)
            return;
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        Save();
        saveOnStart = false;
    }

    private static void TogglePhysicsSystemForSaving(bool enabled)
    {
        if(!Application.isPlaying)
            return;
        var x = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
        x.Enabled = enabled;
    }

    private void SaveData()
    {
        // DOTS Save world
        if(World.All.Count<1) 
            return;
        
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");
        
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        // Path for saving world
        var binaryWorldPath =  saveLocation + "\\" + "DefaultWorld.world"; // path backslash for system access
        var binaryWriter    = new StreamBinaryWriter(binaryWorldPath);
        
        // Save whole world
        SerializeUtilityHybrid.Serialize(em, binaryWriter, out ReferencedUnityObjects referencedUnityObjects);
        
#if UNITY_EDITOR
        // Create an asset from the output. This doesnt work during runtime.
        AssetDatabase.CreateAsset(referencedUnityObjects, unityobjectsAsset);
#endif
        
        binaryWriter.Dispose();
    }

    private void LoadData()
    {
        if(World.All.Count<1) 
            return;
        
        // To generate the file we'll test against
        var binaryPath =  saveLocation + "\\" + "DefaultWorld.world";
            
        // need an empty world to do this
        var loadingWorld = new World("SavingWorld");
        var em           = loadingWorld.EntityManager;
        
        using (var reader = new StreamBinaryReader(binaryPath)) //GetFullPathByName(fileName))
        {
            var objectRefAsset        = AssetDatabase.LoadAssetAtPath<ReferencedUnityObjects>(unityobjectsAsset);
            // Load objects as binary file
            SerializeUtilityHybrid.Deserialize(em, reader, objectRefAsset);
        }
        
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        World.DefaultGameObjectInjectionWorld.EntityManager.MoveEntitiesFrom(em);
    }
    private void Save()
    {
        
    }
}


