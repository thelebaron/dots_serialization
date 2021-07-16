using System;
using Unity.Collections;
using Unity.Entities;

[Serializable]
public struct SaveEntity : IComponentData
{
    public FixedString32 Value;
}

[Serializable]
public struct SerializeableTag : IComponentData { }

[Serializable]
public struct MissingRenderMeshTag : IComponentData { }

[Serializable]
public struct MissingPhysicsColliderTag : IComponentData { }

[Serializable]
public struct MyBlobComponent : IComponentData
{
    public struct MyBlobData
    {
        public int Data;
    }
    
    public BlobAssetReference<MyBlobData> Value;
}

[Serializable]
public struct MissingMyBlobComponentTag : IComponentData { }