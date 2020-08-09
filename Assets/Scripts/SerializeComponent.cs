using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Unity.Entities;
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
        if (GUI.Button(new Rect(100, 100, 500, 500), "Click"))
            Debug.Log("Clicked the button with an image");

        if (GUI.Button(new Rect(100, 700, 500, 300), "Click"))
            Debug.Log("Clicked the button with text");
    }
}


