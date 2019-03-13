using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

// Jobified stair move system
[UpdateAfter(typeof(SpawnSystem))]
class StairMoveSystem : JobComponentSystem
{
    [Unity.Burst.BurstCompile]
    struct StairMove : IJobProcessComponentData<Brick, Position, Scale>
    {
        public float dt;

        public void Execute(ref Brick actor, ref Position position, ref Scale scale)
        {
            // Extract the current position/scale.
            var _pos = position.Value;
            var _scale = scale.Value;
            var _id = actor.ID;
            var _offset = actor.offset;
            var _bIncrease = actor.bIncrease;
            var _moveSpeed = actor.moveSpeed;
            var _oriPosition = actor.oriPosition;

            // Move
            _offset += dt * _bIncrease * _moveSpeed;

            if (_offset > 5f)
                _bIncrease = -1;
            if (_offset < -5f)
                _bIncrease = 1;

            _pos = _oriPosition + new float3(_offset, 0, 0);

            //Build a new position and scale
            position = new Position { Value = _pos };
            scale = new Scale { Value = _scale };
            actor = new Brick { ID = _id, offset = _offset, bIncrease = _bIncrease, moveSpeed = _moveSpeed, oriPosition = _oriPosition };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new StairMove() { dt = UnityEngine.Time.deltaTime };
        var handle = job.Schedule(this, inputDeps);
        return handle;
    }
}
