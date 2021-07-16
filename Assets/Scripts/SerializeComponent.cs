using System;
using System.Collections.Generic;
using System.IO;
using Unity.Assertions;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;
using UnityEngine;
using Utility;

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
    //public MyData myData;
    public List<GameObject> HiddenGameObjects;
    

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
            ToggleSystems(false);
            SaveData(out var objects);
            ToggleSystems(true);
            
            
            JsonMap.CreateJson(objects);

        }

        if (GUI.Button(new Rect(15, 100, 100, 35), "Load"))
        {
            em.CompleteAllJobs();
            ToggleSystems(false);

            var instance = JsonMap.LoadJsonToUnity();
            
            LoadData(instance);
            
            ToggleSystems(true);
            
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
    string saveLocation;//, _FileName, _FileNameJSON;//, _FileNameYAML;
    private EntityManager em;
    
    // old
    
    private void OnEnable()
    {
        saveLocation = Application.persistentDataPath + "\\" + "Saves";
        
        if (!Directory.Exists(saveLocation))
            Directory.CreateDirectory(saveLocation);
        
        
        //_FileName     = "SaveData.xml";
        //_FileNameJSON = "SaveData.json";
        //_FileNameYAML = "World.yaml";
        
        if (World.All.Count == 0)
            return;
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        saveOnStart = false;
    }

    private static void ToggleSystems(bool enabled)
    {
        if(!Application.isPlaying)
            return;

        var world = World.DefaultGameObjectInjectionWorld;
        var systems = world.Systems;

        //Debug.Log(systems.Count);
        foreach (var system in systems)
        {
            system.Enabled = enabled;
        }
        
    }

    private void SaveData(out ReferencedUnityObjects objects)
    {
        ReferencedUnityObjects unityObjects;
        Debug.Log("SaveData");
        
        // DOTS Save world
        if (World.All.Count < 1)
        {
            objects = null;
            Debug.Log("Count < 1");
            return;
        }

        if (!Directory.Exists("Saves"))
        {
            Debug.Log("CreateDirectory");
            Directory.CreateDirectory("Saves");
        }
        
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        // Path for saving world
        var binaryWorldPath =  saveLocation + "\\" + "DefaultWorld.world"; // path backslash for system access
        
        using (var binaryWriter = new StreamBinaryWriter(binaryWorldPath))
        {
            // Save whole world
            SerializeUtilityHybrid.Serialize(em, binaryWriter, out ReferencedUnityObjects referencedUnityObjects);
            unityObjects = referencedUnityObjects;
        } // file is automatically closed after reaching the end of the using block

        
        var yamlpath     = saveLocation + "\\" + "DefaultWorld.yaml";
        using (var streamWriter =  new StreamWriter(yamlpath))
        {
            streamWriter.NewLine = "\n";
            // Save whole world
            SerializeUtility.SerializeWorldIntoYAML(em, streamWriter, false);
        }

#if UNITY_EDITOR
        // Create an asset from the output. This doesnt work during runtime.
        AssetDatabase.CreateAsset(unityObjects, unityobjectsAsset);
#endif
        
        objects = unityObjects;

        Assert.IsNotNull(unityObjects);
    }

    private void LoadData(ReferencedUnityObjects referencedUnityObjects)
    {
        if(World.All.Count<1) 
            return;
        
        Debug.Log("LoadData");
        
        // To generate the file we'll test against
        var binaryPath =  saveLocation + "\\" + "DefaultWorld.world";
        
        // need an empty world to do this
        var loadingWorld = new World("SavingWorld");
        var em           = loadingWorld.EntityManager;
        
        using (var reader = new StreamBinaryReader(binaryPath)) //GetFullPathByName(fileName))
        {
            // Load objects as binary file
            SerializeUtilityHybrid.Deserialize(em, reader, referencedUnityObjects);
        }
        
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        World.DefaultGameObjectInjectionWorld.EntityManager.MoveEntitiesFrom(em);
    }


    private void SaveJsonYamlXml(SerializeComponent script)
    {
        {
            //xml
            /*var serializer = new XmlSerializer(typeof(MyData));
            var textWriter = new StreamWriter(saveLocation + "\\" + "DefaultWorld.xml");
            serializer.Serialize(textWriter, script.myData);
            textWriter.Close();*/
        }
        
        {
            // json
            //var jsondata = JsonUtility.ToJson(script.myData, true);
            //File.WriteAllText(saveLocation + "\\" + "DefaultWorld.json", jsondata);
        }

        {
            // yaml dots
            if (World.All.Count < 1) return;
            using (var writer = new StreamWriter(saveLocation + "\\" + "DefaultWorld.yaml"))
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                // Save world to yaml
                writer.NewLine = "\n";
                SerializeUtility.SerializeWorldIntoYAML(em, writer, false); // is yaml just debugging?
            }
        }
    }
}


