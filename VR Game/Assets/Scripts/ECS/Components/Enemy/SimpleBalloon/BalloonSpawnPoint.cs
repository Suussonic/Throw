using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct BalloonSpawnData
    {
        public float3 SpawnPosition;  
        public float TargetHeight;   
    }
    
    [ChunkSerializable]
    public struct BalloonSpawnPoint : IComponentData
    {
        public NativeArray<BalloonSpawnData> Value;
    }
}
