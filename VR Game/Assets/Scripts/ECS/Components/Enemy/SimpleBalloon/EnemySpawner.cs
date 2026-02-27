using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct EnemySpawner : IComponentData
    {
        public Entity EnemyPrefab;
        public float Timer;
        public float SpawnRate;
        public float3 SpawnPosition;
    }
}