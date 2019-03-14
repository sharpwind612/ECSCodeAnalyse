using System;
using Unity.Entities;

[Serializable]
public struct Object : IComponentData
{
    public int index;
}

public class ObjectProxy : ComponentDataProxy<Object> { }
