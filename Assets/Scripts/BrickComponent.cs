using Unity.Entities;
using Unity.Mathematics;

//brick data component
struct Brick : IComponentData
{
    public uint ID;
    public float offset;
    public int bIncrease;
    public float moveSpeed;
    public float3 oriPosition;
}
