using ECS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems
{
    public partial struct SpawnBalloonSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnZoneProperties>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var spawnZoneEntity = SystemAPI.GetSingletonEntity<SpawnZoneProperties>();
            var spawnZone = SystemAPI.GetAspect<SpawnZoneAspect>(spawnZoneEntity);

            if (spawnZone.BalloonSpawnPoints.Length == 0) return;

            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            new SpawnBalloonJob
            {
                DeltaTime = deltaTime,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                BalloonSpawnPoints = spawnZone.BalloonSpawnPoints,
                SpawnZoneEntity = spawnZoneEntity,
                BalloonRiseRateLookup = state.GetComponentLookup<BalloonRiseRate>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct SpawnBalloonJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        [ReadOnly] public NativeArray<BalloonSpawnData> BalloonSpawnPoints;
        public Entity SpawnZoneEntity;
        [ReadOnly] public ComponentLookup<BalloonRiseRate> BalloonRiseRateLookup;

        private void Execute(in SpawnZoneProperties spawnZoneProperties, [EntityIndexInQuery] int entityIndexInQuery, RefRW<BalloonSpawnTimer> balloonSpawnTimer, RefRW<SpawnZoneRandom> spawnZoneRandom)
        {
            balloonSpawnTimer.ValueRW.Value -= DeltaTime;
            if (balloonSpawnTimer.ValueRW.Value > 0f) return;
            if (BalloonSpawnPoints.Length == 0) return;

            balloonSpawnTimer.ValueRW.Value = spawnZoneProperties.BalloonSpawnRate;
            var newBalloon = ECB.Instantiate(entityIndexInQuery, spawnZoneProperties.BasicBalloonPrefab);

            var spawnPointIndex = spawnZoneRandom.ValueRW.Value.NextInt(0, BalloonSpawnPoints.Length);
            var spawnData = BalloonSpawnPoints[spawnPointIndex];

            var newBalloonTransform = new LocalTransform
            {
                Position = spawnData.SpawnPosition,
                Rotation = quaternion.identity,
                Scale = 1f
            };
            ECB.SetComponent(entityIndexInQuery, newBalloon, newBalloonTransform);

            // Read both riseRate and targetHeight from the prefab (baked by GoblinBaker from the Balloon inspector)
            var prefabRiseRate = BalloonRiseRateLookup[spawnZoneProperties.BasicBalloonPrefab];
            ECB.SetComponent(entityIndexInQuery, newBalloon, new BalloonRiseRate
            {
                Value = prefabRiseRate.Value,
                TargetHeight = prefabRiseRate.TargetHeight
            });
        }
    }
}
