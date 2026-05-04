using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.Enemy.SimpleBalloon
{
    public struct BalloonSpawnData : IComponentData
    {
        public float3 SpawnPosition;  
        public float TargetHeight;   
    }
}