using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
// ISharedComponentData can have struct members with managed types.
public struct ObjectSpawner : ISharedComponentData
{
    public GameObject prefab;
    public int spawnNumber;
    public float lastSpawnTime;
    public int curIndex;
}

public class ObjectSpawnerProxy : SharedComponentDataProxy<ObjectSpawner> { }
