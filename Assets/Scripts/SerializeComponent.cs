using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DOTS.Serialization;
using ReferencedObjects;
using Unity.Assertions;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Physics.Systems;
using Unity.Scenes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
            em.CompleteAllJobs();
            TogglePhysicsSystemForSaving(false);
            SaveData(out var objects);
            TogglePhysicsSystemForSaving(true);
            
            
            NewAssetUtility.CreateJson(objects);

        }

        if (GUI.Button(new Rect(15, 100, 100, 35), "Load"))
        {
            em.CompleteAllJobs();
            TogglePhysicsSystemForSaving(false);

            var instance = NewAssetUtility.LoadJsonToUnity();
            
            StartCoroutine(Wait());
            
            LoadData(instance);
            
            TogglePhysicsSystemForSaving(true);
            
            //NewAssetUtility.LoadJsonToUnity();
        }

        if (GUI.Button(new Rect(15, 150, 100, 35), "Destroy All Entities"))
        {
            if(World.All.Count<1)
                return;
            
            World.DefaultGameObjectInjectionWorld.EntityManager.CompleteAllJobs();
            
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        }
        
    }
    
    const string unityobjectsAsset = "Assets/UnityObjects.asset";
    string saveLocation, _FileName, _FileNameJSON, _FileNameYAML;
    private EntityManager em;
    
    // old
    
    private void OnEnable()
    {
        saveLocation = Application.persistentDataPath + "\\" + "Saves";
        
        if (!Directory.Exists(saveLocation))
            Directory.CreateDirectory(saveLocation);
        
        
        _FileName     = "SaveData.xml";
        _FileNameJSON = "SaveData.json";
        _FileNameYAML = "World.yaml";
        
        if (World.All.Count == 0)
            return;
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        saveOnStart = false;
    }

    private static void TogglePhysicsSystemForSaving(bool enabled)
    {
        if(!Application.isPlaying)
            return;
        
        var x = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
        x.Enabled = enabled;
        var y = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<StepPhysicsWorld>();
        y.Enabled = enabled;
        var z = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ExportPhysicsWorld>();
        z.Enabled = enabled;
    }

    private void SaveData(out ReferencedUnityObjects objects)
    {
        objects = null;
        
        // DOTS Save world
        if (World.All.Count < 1)
        {
            return;
        }
        
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");
        
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        // Path for saving world
        var binaryWorldPath =  saveLocation + "\\" + "DefaultWorld.world"; // path backslash for system access
        
        // Ensure not locked
        if(FileHelper.IsFileLocked(new FileInfo(binaryWorldPath)))
            return;
        
        var binaryWriter    = new StreamBinaryWriter(binaryWorldPath);
        
        // Save whole world
        SerializeUtilityHybrid.Serialize(em, binaryWriter, out ReferencedUnityObjects referencedUnityObjects);
        
#if UNITY_EDITOR
        // Create an asset from the output. This doesnt work during runtime.
        AssetDatabase.CreateAsset(referencedUnityObjects, unityobjectsAsset);
#endif
        
        objects = referencedUnityObjects;
        binaryWriter.Dispose();
    }

    private void LoadData(ReferencedUnityObjects referencedUnityObjects)
    {
        if(World.All.Count<1) 
            return;
        
        // To generate the file we'll test against
        var binaryPath =  saveLocation + "\\" + "DefaultWorld.world";
        
        // Ensure not locked
        if(FileHelper.IsFileLocked(new FileInfo(binaryPath)))
            return;
        
        // need an empty world to do this
        var loadingWorld = new World("SavingWorld");
        var em           = loadingWorld.EntityManager;
        
        using (var reader = new StreamBinaryReader(binaryPath)) //GetFullPathByName(fileName))
        {
            //var referencedUnityObjects = AssetDatabase.LoadAssetAtPath<ReferencedUnityObjects>(unityobjectsAsset);
            
            // Load objects as binary file
            SerializeUtilityHybrid.Deserialize(em, reader, referencedUnityObjects);
        }
        
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        World.DefaultGameObjectInjectionWorld.EntityManager.MoveEntitiesFrom(em);
    }

    IEnumerator Wait()
    {
 
//returning 0 will make it wait 1 frame
        yield return 0;
 
//code goes here
 
 
    }
}


