using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Scenes;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SerializeComponent))]
[CanEditMultipleObjects]
public class XMLEditor : Editor
{
    string _FileLocation, _FileName, _FileNameJSON, _FileNameYAML;
    private string jsonpath;
    private string xmlpath;
    private string yamlpath;
    private EntityManager em;
    

    
    private void OnEnable()
    {
        _FileLocation = Application.dataPath; 
        _FileName     = "SaveData.xml";
        _FileNameJSON = "SaveData.json";
        _FileNameYAML = "World.yaml";
        xmlpath  = _FileLocation + "\\" + _FileName;
        jsonpath = _FileLocation + "\\" + _FileNameJSON;
        yamlpath = _FileLocation + "\\" + _FileNameYAML;
        if (World.All.Count == 0) 
            return;
        
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        
    }

    private void OnSceneGUI()
    {
        var node = target as SerializeComponent;
    }

    public override void OnInspectorGUI()
    {
        var script = target as SerializeComponent;
        if (script.saveOnStart)
        {
            Save();
            script.saveOnStart = false;
        }

        //timescale = GUILayout.HorizontalScrollbar(timescale, 1.0f, 0.0f, 10.0f);
        GUILayout.HorizontalSlider(1, 0, 1);
        
        if (GUILayout.Button("Destroy All Entities"))
        {
            if(World.All.Count<1)
                return;
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        }
                
        if (GUILayout.Button("Load"))
        {
            LoadData();
        }
        
        if (GUILayout.Button("Save"))
        {
            TogglePhysicsSystemForSaving(false);
            SaveData();
            TogglePhysicsSystemForSaving(true);

            //SaveJsonYamlXml(script);

            Debug.Log("Saved");
        }

        // Save gameobjects to binary file
        if (GUILayout.Button("Destroy GameObjects"))
        {
            
            var allGameObjects = FindObjectsOfType<GameObject>().ToList();

            script.SavedGameObjects = allGameObjects;

            GameObject ignore = null;
            foreach (var go in allGameObjects)
            {
                if (go == script.gameObject)
                {
                    ignore = go;
                }
            }
            allGameObjects.Remove(ignore);
            Debug.Log("Removing editor from destroy list");
            
            // Save objects as binary file
            var referencedObjectsPath =  "Assets/BinaryObjectsSaveFile.binary";// path forward slash for asset access
            BinarySerialization.WriteToBinaryFile(referencedObjectsPath, allGameObjects);
            Debug.Log("Saved all gameobjects to binary file");

            for (int i = 0; i < allGameObjects.Count; i++)
            {
                Destroy(allGameObjects[i]);
            }
        }
        if (GUILayout.Button("Create GameObjects"))
        {
            
            // Load objects from binary file
            var path =  "Assets/BinaryObjectsSaveFile.binary";// path forward slash for asset access
            var list = BinarySerialization.ReadFromBinaryFile<List<GameObject>>(path);
            Debug.Log("Read all gameobjects from binary file");
            
            foreach (var go in list)
            {
                Instantiate(go, go.transform.position, go.transform.rotation);
            }
        }

        DrawDefaultInspector();
    }

    private void SaveJsonYamlXml(SerializeComponent script)
    {
        {
            //xml
            var serializer = new XmlSerializer(typeof(MyData));
            var textWriter = new StreamWriter(xmlpath);
            serializer.Serialize(textWriter, script.myData);
            textWriter.Close();
        }
        
        {
            // json
            var jsondata = JsonUtility.ToJson(script.myData, true);
            File.WriteAllText(jsonpath, jsondata);
        }

        {
            // yaml dots
            if (World.All.Count < 1) return;
            using (var writer = new StreamWriter(yamlpath))
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                // Save world to yaml
                writer.NewLine = "\n";
                SerializeUtility.SerializeWorldIntoYAML(em, writer, false); // is yaml just debugging?
            }
            /*
            {
                // json make asset map json file
                var path = _FileLocation + "\\" + "assetmap.json";;
                var jsondata = JsonUtility.ToJson(AssetMapUtilities.UpdateAddressables(), true);
                File.WriteAllText(path, jsondata);
            }*/
        }
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
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");
        
        var formatter = new BinaryFormatter();
        //var objects = new List<System.Object>();
        var saveFile = File.Create("Saves/Save.bin");
        AssetDatabase.Refresh();
        
        // DOTS Save world
        if(World.All.Count<1) return;
        //using (var writer = new StreamWriter(yamlpath))
        //{
            // Path for saving world
            var binaryWorldPath =  _FileLocation + "\\" + "DefaultWorld.world"; // path backslash for system access
            var binaryWriter    = new StreamBinaryWriter(binaryWorldPath);
            
            // Save whole world
            string referencedObjectsPath =  "Assets/ReferencedUnityWorldObjects.asset";// path forward slash for asset access
            SerializeUtilityHybrid.Serialize(em, binaryWriter, out ReferencedUnityObjects referencedUnityObjects);
            //SerializeUtility.SerializeWorld(em, binaryWriter, out var objectReferences);

            for (int i = 0; i < referencedUnityObjects.Array.Length; i++)
            {
                Debug.Log(referencedUnityObjects.Array[i].name);
            }
            
            // more serialization: note serializeHybrid always gives unityobject types
            /*{
                // gather referenced objects
                WObjects worldObjects = new WObjects
                {
                    Objects = new UObject[objectReferences.Array.Length]
                };
                for (int i = 0; i < objectReferences.Array.Length; i++)
                {
                    UnityEngine.Object obj = objectReferences.Array[i];
                    //ignore if not unity object
                    //if(referencedObject is UnityEngine.Object uObject) //if (!(myVariable is SomeType))
                    string key = AssetMapUtilities.GetKey(obj);
                    string address = null;

                    Dictionary<string, string> serializedAssetMap = AssetMapUtilities.GetSavedAssetMap().AssetMapping;
                    if (serializedAssetMap.ContainsKey(key))
                    {
                        Debug.Log("found match");
                    }
                }
            }*/
            foreach (var obj in referencedUnityObjects.Array)
            {
                Debug.Log(obj.GetType());
                
            }
            var rm = new RenderMesh();
            Debug.Log(rm.GetType());
            
            // For debugging: log all referenced objects which are saved QueryReferences.Query(objectReferences);
            AssetDatabase.CreateAsset(referencedUnityObjects, referencedObjectsPath);
            binaryWriter.Dispose();
            //objects.Add(objectReferences);
            
        //}
        
        // objects is not a serializable thing
        //formatter.Serialize(saveFile, objects);
    }

    private void LoadData()
    {
        if(!File.Exists("Saves/Save.bin")) 
            return;
        if(World.All.Count<1) 
            return;
        
        //var formatter = new BinaryFormatter();
        //var saveFile = File.Open("Saves/Save.bin", FileMode.Open);
        //var deserializedObject = formatter.Deserialize(saveFile);
        //var objects = deserializedObject as List<Object>;
        //var refObjects = objects[0] as ReferencedUnityObjects;
        
        // To generate the file we'll test against
        var binaryPath =  _FileLocation + "\\" + "DefaultWorld.world";
            
        // need an empty world to do this
        var loadingWorld = new World("SavingWorld");
        var em           = loadingWorld.EntityManager;
        
        using (var reader = new StreamBinaryReader(binaryPath)) //GetFullPathByName(fileName))
        {
            var referencedObjectsPath =  "Assets/ReferencedUnityWorldObjects.asset";// path forward slash for asset access
            var objectRefAsset        = AssetDatabase.LoadAssetAtPath<ReferencedUnityObjects>(referencedObjectsPath);
            // Load objects as binary file
            SerializeUtilityHybrid.Deserialize(em, reader, objectRefAsset);
            
        }
            
            
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        World.DefaultGameObjectInjectionWorld.EntityManager.MoveEntitiesFrom(em);
        
        Debug.Log("Loaded");
    }
    private void Save()
    {
        
    }


    /*
    // Finally our save and load methods for the file itself 
    void CreateXML() 
    { 
        // Where we want to save and load to and from 
        _FileLocation = Application.dataPath; 
        _FileName     = "SaveData.xml";
        
        StreamWriter writer; 
        FileInfo     t = new FileInfo(_FileLocation+"\\"+ _FileName); 
        if(!t.Exists) 
        { 
            writer = t.CreateText(); 
        } 
        else 
        { 
            t.Delete(); 
            writer = t.CreateText(); 
        } 
        
        writer.Write(_data); 
        writer.Close(); 
        Debug.Log("File written."); 
    } */
    
    /*
     
    private void SaveData()
    {
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");
        
        var formatter = new BinaryFormatter();
        var objects = new List<System.Object>();
        var saveFile = File.Create("Saves/Save.bin");
        
        // DOTS Save world
        if(World.All.Count<1) return;
        using (var writer = new StreamWriter(yamlpath))
        {
            // Path for saving world
            var binaryWorldPath =  _FileLocation + "\\" + "DefaultWorld.world"; // path backslash for system access
            var binaryWriter    = new StreamBinaryWriter(binaryWorldPath);
                    
            // Save whole world
            // not hybrid SerializeUtility.SerializeWorld(em, binaryWriter, out var objectReferences);
            var referencedObjectsPath =  "Assets/ReferencedUnityWorldObjects.asset";// path forward slash for asset access
            SerializeUtilityHybrid.Serialize(em, binaryWriter, out ReferencedUnityObjects objectReferences);
            binaryWriter.Dispose();
            
            // For debugging: log all referenced objects which are saved QueryReferences.Query(objectReferences);
            AssetDatabase.CreateAsset(objectReferences, referencedObjectsPath);
            objects.Add(objectReferences);
            
            /*var zx   = GameObject.Find("test");
            var xm   = zx.GetComponent<MeshFilter>();
            var m    = xm.sharedMesh;
            var code = m.GetHashCode();
        }
                
        formatter.Serialize(saveFile,objects);
        } 
     
     
     
    void SaveToXml()
    {            
        string path = EditorUtility.SaveFilePanel("Save the database to XML file.", "", "ItemDatabase.xml", ".xml");            
 
        if (path == "")
            return;
           
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        XmlWriter writer = XmlWriter.Create(path, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("ItemDatabase");
        writer.WriteAttributeString("version", saveXMLVersion.ToString());
 
        for (int i = 0; i < database.Count; i++)
        {
            database.Get(i).writeToXML(writer);
        }
 
        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();
        writer.Close();
    }
 
    void LoadFromXml()
    {
        string path = EditorUtility.OpenFilePanel("Select the XML file to load the database from", "", "xml");
 
        if (path == "")
            return;
 
        XmlDocument doc = new XmlDocument();
        try
        {
            doc.Load(path);
            XmlElement  root = (XmlElement)doc.GetElementsByTagName("ItemDatabase").Item(0);
            XmlNodeList xl   = root.GetElementsByTagName("Item");
            database.Clear();
 
            foreach (XmlElement e in xl)
            {
                ISObject obj = new ISObject();
                obj.readFromXML(e);
                database.Add(obj);
            }
 
            GenerateWeaponTags();
            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    
    
    
    
    
    
    
    
    */
}


