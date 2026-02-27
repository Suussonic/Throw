using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Authoring
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        public GameObject enemyPrefab;
        public float spawnRate = 2f;

        class Baker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new EnemySpawner
                {
                    EnemyPrefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic),
                    SpawnRate =  authoring.spawnRate,
                    Timer =  0f,
                    SpawnPosition = authoring.transform.position
                });
            }
        }
    }
}