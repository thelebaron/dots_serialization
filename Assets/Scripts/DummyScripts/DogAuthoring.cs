using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class DogAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public bool AddClassComponent;
        
        // not converted to ecs data
        [SerializeReference] public List<RootNode>  m_Trees;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Dog {GoodGirl = true});
            if(AddClassComponent) dstManager.AddComponentObject(entity, new DogClass {GoodGirl = true, SetNumber = 234});
        }
    }

    public interface INode{  }
 
    [Serializable]
    public class RootNode : INode
    {
        [SerializeReference] public INode left;
        [SerializeReference] public INode right;
    }
 
    [Serializable]
    class SubNode : RootNode
    {
        [SerializeReference] INode parent;
    }
 
    [Serializable]
    class LeafNode : INode
    {
        [SerializeReference] public INode parent;
    }


    public struct Dog : IComponentData
    {
        public bool GoodGirl;
    }
    
    [Serializable]
    public class DogClass : IComponentData
    {
        public bool GoodGirl;
        public int Number;
        public int SetNumber;
    }
}