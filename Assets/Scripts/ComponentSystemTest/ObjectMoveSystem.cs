using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// 继承JobComponentSystem来进行并行处理，下方有主线程的ComponentSystem版本
// 这里无论是Jobfied还是普通版本，都会维护一个符合要求的ComponentGroup集合，每次Update对这个集合包含的Entity执行操作
// * 一个ComponentSystem可以自己维护多个ComponentGroup在集合中使用，而JobComponentSystem执行时一般只能使用定义IJobProcessComponentData时指定类型的
// * 一个JobComponentSystem中可以执行多个IJobProcessComponentData，来分别处理不同的工作
// * 多个IJobProcessComponentData需要有前后依赖关系，并将最后一个IJobProcessComponentData的JobHandle在OnUpdate中返回，不然会无法处理后续的System依赖而报错
class ObjectMoveSystem : JobComponentSystem
{
    //通过RequireComponentTagAttribute为JobComponentSystem添加额外的依赖项
    //[RequireComponentTagAttribute(typeof(Object))]
    //通过RequireSubtractiveComponentAttribute为JobComponentSystem添加额外的排除项
    [RequireSubtractiveComponentAttribute(typeof(ObjectSpawner))]  
    [Unity.Burst.BurstCompile]
    struct ObjectMove : IJobProcessComponentData<Position>//, Object>
    {
        public float dt;

        public void Execute(ref Position position)//, [ReadOnly]ref Object _object)
        {
            var _pos = position.Value;
            _pos += new float3(0, dt, 0);
            //Build a new position and scale
            position = new Position { Value = _pos };
        }
    }
    [Unity.Burst.BurstCompile]
    struct ObjectRotate : IJobProcessComponentData<Rotation>
    {
        public float dt;

        public void Execute(ref Rotation rotation)
        {
            rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), 5 * dt));
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job1 = new ObjectMove() { dt = UnityEngine.Time.deltaTime };
        var handle1 = job1.Schedule(this, inputDeps);
        var job2 = new ObjectRotate() { dt = UnityEngine.Time.deltaTime };
        var handle2 = job2.Schedule(this, handle1);
        return handle2;
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
