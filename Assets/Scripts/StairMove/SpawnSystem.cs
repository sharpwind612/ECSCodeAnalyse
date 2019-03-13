using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;

class SpawnSystem : ComponentSystem
{
    List<SpawnInfo> _uniques = new List<SpawnInfo>();
    ComponentGroup _group;

    //Actor archetype used for instantiation
    EntityArchetype _actorArchetype;
    // Instance counter used for generating actor IDs
    static uint _counter;
    // max value used for actor horizontal position
    static int _maxValue = 10;

    //collect the SpawnInfo component and create an entity archetype
    protected override void OnCreateManager()
    {
        _group = GetComponentGroup(typeof(SpawnInfo));

        _actorArchetype = EntityManager.CreateArchetype(
            typeof(Brick), typeof(Position), typeof(Scale), typeof(RenderMesh)
        );
    }

    protected override void OnUpdate()
    {
        // Enumerate all the buffers.
        EntityManager.GetAllUniqueSharedComponentData<SpawnInfo>(_uniques);
        int tempValue = -_maxValue;
        int heightCount = 0;
        float randomSpeed = 0f;
        Random random = new Random((uint)System.DateTime.Now.Ticks);
        for (var i = 0; i < _uniques.Count; i++)
        {
            _group.SetFilter(_uniques[i]);
            // Get a copy of the entity array.
            // Don't directly use the iterator -- we're going to remove
            // the buffer components, and it will invalidate the iterator.
            var iterator = _group.GetEntityArray();
            var entities = new NativeArray<Entity>(iterator.Length, Allocator.Temp);
            iterator.CopyTo(entities);
            // Instantiate actors along with the buffer entities.
            for (var j = 0; j < entities.Length; j++)
            {
                // Create the first voxel.
                var voxel = EntityManager.CreateEntity(_actorArchetype);
                EntityManager.SetComponentData(voxel, new Brick { ID = _counter++});
                EntityManager.SetSharedComponentData(voxel, _uniques[i].RendererSettings);
                //EntityManager.SetSharedComponentData(voxel, rm);

                // Make clones from the first voxel.
                var cloneCount = _uniques[i].MaxCount - 1;
                if (cloneCount > 0)
                {
                    var clones = new NativeArray<Entity>(cloneCount, Allocator.Temp);
                    EntityManager.Instantiate(voxel, clones);
                    randomSpeed = random.NextFloat(0f, 2f);
                    //RenderMesh rm = GetUniqueMaterial(heightCount, _uniques[i].RendererSettings);
                    for (var k = 0; k < cloneCount; k++)
                    {
                        EntityManager.SetComponentData(clones[k], new Position { Value = new float3(tempValue, heightCount, heightCount) });
                        EntityManager.SetComponentData(clones[k], new Scale { Value = new float3(1,1,1)});                       
                        EntityManager.SetComponentData(clones[k], new Brick { ID = _counter++, offset = 0, bIncrease = 1, moveSpeed = randomSpeed, oriPosition = new float3(tempValue, heightCount, heightCount) });//,                                               
                        //TODO:give each brick a different Material value seems to be impossible,i will do it in the Shader
                        //EntityManager.SetSharedComponentData(clones[k], rm);

                        tempValue = tempValue + 1;
                        if (tempValue > _maxValue)
                        {
                            heightCount++;
                            tempValue = -_maxValue;
                            randomSpeed = random.NextFloat(0f, 0.2f);
                            //rm = GetUniqueMaterial(heightCount, _uniques[i].RendererSettings);
                        }
                    }
                    clones.Dispose();
                }

                // Remove the buffer component from the entity.
                EntityManager.RemoveComponent(entities[j], typeof(SpawnInfo));
            }
            entities.Dispose();
        }
        _uniques.Clear();
    }

    //private RenderMesh GetUniqueMaterial(int index, RenderMesh oriRenderMesh)
    //{
    //    UnityEngine.Material mat = oriRenderMesh.material;
    //    float tempValue = (index % 256) / 254f;
    //    mat.SetFloat("_Value", tempValue);
    //    oriRenderMesh.material = mat;
    //    return oriRenderMesh;
    //}
}
