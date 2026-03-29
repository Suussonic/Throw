using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ECS.Authoring
{
    public class SpawnZone : MonoBehaviour
    {
        public float2 fieldDimensions;
        public int numberSpawnPoints;
        public GameObject spawnPrefab;
        public uint randomSeed;
        public GameObject basicBalloonPrefab;
        public float balloonSpawnRate = 2f;
    }
    
    public class SpawnZoneBaker : Baker<SpawnZone>
    {
        public override void Bake(SpawnZone authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpawnZoneProperties
            {
                FieldDimensions = authoring.fieldDimensions,
                NumberSpawnPoints = authoring.numberSpawnPoints,
                EnemySpawnPrefab = GetEntity(authoring.spawnPrefab, TransformUsageFlags.Dynamic),
                BasicBalloonPrefab = GetEntity(authoring.basicBalloonPrefab, TransformUsageFlags.Dynamic),
                BalloonSpawnRate = authoring.balloonSpawnRate,
            });
            AddComponent(entity, new SpawnZoneRandom
            {
                Value = Random.CreateFromIndex(authoring.randomSeed),
            });
            AddComponent<BalloonSpawnPoint>(entity);
            AddComponent<BalloonSpawnTimer>(entity);
        }
    }
}