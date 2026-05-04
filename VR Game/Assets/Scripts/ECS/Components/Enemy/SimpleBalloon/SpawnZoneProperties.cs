using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.Enemy.SimpleBalloon
{
    public struct SpawnZoneProperties : IComponentData
    {
        public float2 FieldDimensions;
        public int NumberSpawnPoints;
        public Entity EnemySpawnPrefab;
        public Entity BasicBalloonPrefab;
        public float BalloonSpawnRate;
    }

    public struct BalloonSpawnTimer : IComponentData
    {
        public float Value;
    }
}