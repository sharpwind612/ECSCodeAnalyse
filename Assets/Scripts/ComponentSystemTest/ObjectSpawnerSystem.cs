using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// ComponentSystems run on the main thread. Use these when you have to do work that cannot be called from a job.
public class ObjectSpawnerSystem : ComponentSystem
{
    ComponentGroup m_Spawners;

    protected override void OnCreateManager()
    {
        m_Spawners = GetComponentGroup(typeof(ObjectSpawner), typeof(Position));
    }

    protected override void OnUpdate()
    {
        // Get all the spawners in the scene.
        using (var spawners = m_Spawners.ToEntityArray(Allocator.TempJob))
        {
            foreach (var spawner in spawners)
            {
                var spawnerData = EntityManager.GetSharedComponentData<ObjectSpawner>(spawner);
                var lastSpawnTime = spawnerData.lastSpawnTime;
                if (lastSpawnTime > 1.0f)
                {
                    var count = spawnerData.spawnNumber;
                    if (count > 0)
                    {
                        // Create an entity from the prefab set on the spawner component.
                        var prefab = spawnerData.prefab;
                        var entity = EntityManager.Instantiate(prefab);

                        // Copy the position of the spawner to the new entity.
                        var position = EntityManager.GetComponentData<Position>(spawner);
                        var index = spawnerData.curIndex;
                        position.Value += new float3(1.1f * index, 0, 0);
                        EntityManager.SetComponentData(entity, position);
                        spawnerData.curIndex = ++index;
                    }
                    count--;
                    if (count <= 0)
                    {
                        // Destroy the spawner
                        EntityManager.DestroyEntity(spawner);
                        return;
                    }
                    // Write back
                    spawnerData.spawnNumber = count;
                    lastSpawnTime = 0f;
                }
                lastSpawnTime += UnityEngine.Time.deltaTime;
                spawnerData.lastSpawnTime = lastSpawnTime;
                EntityManager.SetSharedComponentData(spawner, spawnerData);
            }
        }
    }
}
