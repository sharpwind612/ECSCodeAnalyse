using Unity.Entities;
using Unity.Rendering;

// Spwan info component
// Instantiates lots of voxels for instanced mesh renderers.

[System.Serializable]
struct SpawnInfo : ISharedComponentData
{
    public int MaxCount;
    public RenderMesh RendererSettings;
}

class SpawnInfoComponent : SharedComponentDataProxy<SpawnInfo> { }
