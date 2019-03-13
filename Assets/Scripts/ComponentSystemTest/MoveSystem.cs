using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// ComponentSystems run on the main thread. Use these when you have to do work that cannot be called from a job.
public class MoveSystem : ComponentSystem
{
    ComponentGroup m_Objects;

    protected override void OnCreateManager()
    {
        //做一个筛选，选择含有Postion但不含ObjectSpawner的Entity，包括所有生成出的Cube和Sphere
        var query = new EntityArchetypeQuery
        {
            Any = new ComponentType[] { typeof(Position)},
            None = new ComponentType[] { typeof(ObjectSpawner) }
        };
        m_Objects = GetComponentGroup(query);
    }

    protected override void OnUpdate()
    {
        // Get all the objects in the scene.
        using (var objects = m_Objects.ToEntityArray(Allocator.TempJob))
        {
            foreach (var _object in objects)
            {
                var position = EntityManager.GetComponentData<Position>(_object);
                position.Value += new float3(0, UnityEngine.Time.deltaTime, 0);
                EntityManager.SetComponentData(_object, position);
            }
        }
    }
}
