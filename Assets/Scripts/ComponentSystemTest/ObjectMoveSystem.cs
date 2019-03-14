using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// 继承JobComponentSystem来进行并行处理，下方有主线程的ComponentSystem版本
// 这里无论是Jobfied还是普通版本，都会维护一个符合要求的ComponentGroup集合，每次Update对这个集合包含的Entity执行操作
class ObjectMoveSystem : JobComponentSystem
{
    [Unity.Burst.BurstCompile]
    struct ObjectMove : IJobProcessComponentData<Position,Rotation>
    {
        public float dt;

        public void Execute(ref Position position,ref Rotation rotation)
        {
            var _pos = position.Value;
            _pos += new float3(0, dt, 0);
            //Build a new position and scale
            position = new Position { Value = _pos };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ObjectMove() { dt = UnityEngine.Time.deltaTime };
        var handle = job.Schedule(this, inputDeps);
        return handle;
    }
}


// 一个使用正常ComponentSystem实现的例子，所有Component会在主线程调用，所以一般批量化的工作会使用JobComponentSystem
// ComponentSystems run on the main thread. Use these when you have to do work that cannot be called from a job.
//public class ObjectMoveSystem : ComponentSystem
//{
//    ComponentGroup m_Objects;

//    protected override void OnCreateManager()
//    {
//        //做一个筛选，选择含有Postion但不含ObjectSpawner的Entity，包括所有生成出的Cube和Sphere
//        var query = new EntityArchetypeQuery
//        {
//            Any = new ComponentType[] { typeof(Position)},
//            None = new ComponentType[] { typeof(ObjectSpawner) }
//        };
//        m_Objects = GetComponentGroup(query);
//    }

//    protected override void OnUpdate()
//    {
//        // Get all the objects in the scene.
//        using (var objects = m_Objects.ToEntityArray(Allocator.TempJob))
//        {
//            foreach (var _object in objects)
//            {
//                var position = EntityManager.GetComponentData<Position>(_object);
//                position.Value += new float3(0, UnityEngine.Time.deltaTime, 0);
//                EntityManager.SetComponentData(_object, position);
//            }
//        }
//    }
//}
