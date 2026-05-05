using ECS.Components.Enemy.SimpleBalloon;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems.Enemy.SimpleBalloon
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct SpawnPointSystem : ISystem
    {
        private const float SafetyRadius = 20f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnZoneProperties>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var spawnZoneEntity = SystemAPI.GetSingletonEntity<SpawnZoneProperties>();
            var zoneTransform = SystemAPI.GetComponentRO<LocalTransform>(spawnZoneEntity).ValueRO;
            var props = SystemAPI.GetComponentRO<SpawnZoneProperties>(spawnZoneEntity).ValueRO;
            var random = SystemAPI.GetComponentRW<SpawnZoneRandom>(spawnZoneEntity);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            const float undergroundOffset = 2f;

            for (var i = 0; i < props.NumberSpawnPoints; i++)
            {
                var newSpawnPoint = ecb.Instantiate(props.EnemySpawnPrefab);
                var newSpawnPointTransform = GetRandomSpawnPointTransform(zoneTransform, props, ref random.ValueRW.Value);
                ecb.SetComponent(newSpawnPoint, newSpawnPointTransform);

                ecb.AddComponent(newSpawnPoint, new BalloonSpawnData
                {
                    SpawnPosition = newSpawnPointTransform.Position + new float3(0f, -undergroundOffset, 0f),
                    TargetHeight = newSpawnPointTransform.Position.y
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static LocalTransform GetRandomSpawnPointTransform(in LocalTransform zoneTransform, in SpawnZoneProperties props, ref Random random)
        {
            return new LocalTransform
            {
                Position = GetRandomPosition(zoneTransform.Position, props.FieldDimensions, ref random),
                Rotation = quaternion.RotateY(random.NextFloat(-0.25f, 0.25f)),
                Scale = random.NextFloat(0.5f, 1f)
            };
        }

        private static float3 GetRandomPosition(float3 center, float2 fieldDimensions, ref Random random)
        {
            var halfDimensions = new float3(fieldDimensions.x * 0.5f, 0f, fieldDimensions.y * 0.5f);
            var minCorner = center - halfDimensions;
            var maxCorner = center + halfDimensions;

            float3 randomPosition;
            do
            {
                randomPosition = random.NextFloat3(minCorner, maxCorner);
            } while (math.distancesq(center, randomPosition) < SafetyRadius);

            return randomPosition;
        }
    }
}
