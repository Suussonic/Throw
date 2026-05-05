using ECS.Components;
using ECS.Components.Enemy.SimpleBalloon;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems.Enemy.SimpleBalloon
{
    public partial struct SpawnBalloonSystem : ISystem
    {
        private ComponentLookup<BalloonRiseRate> _balloonRiseRateLookup;
        private EntityQuery _spawnPointQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnZoneProperties>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            
            _balloonRiseRateLookup = state.GetComponentLookup<BalloonRiseRate>(true);

            _spawnPointQuery = new EntityQueryBuilder(Allocator.Persistent)
                .WithAll<BalloonSpawnData>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _balloonRiseRateLookup.Update(ref state);

            // On récupère dynamiquement les points de spawn posés via SpawnPointSystem
            var spawnPoints = _spawnPointQuery.ToComponentDataArray<BalloonSpawnData>(Allocator.Temp);
            
            if (spawnPoints.Length == 0) return;

            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Cette boucle est l'équivalent de ton IJobEntity (Optimisée Burst)
            foreach (var (spawnZoneProperties, balloonSpawnTimer, spawnZoneRandom) in
                     SystemAPI.Query<RefRO<SpawnZoneProperties>, RefRW<BalloonSpawnTimer>, RefRW<SpawnZoneRandom>>())
            {
                var props = spawnZoneProperties.ValueRO;

                balloonSpawnTimer.ValueRW.Value -= deltaTime;
                if (balloonSpawnTimer.ValueRW.Value > 0f)
                {
                    continue;
                }

                var newBalloon = ecb.Instantiate(props.BasicBalloonPrefab);

                var random = spawnZoneRandom.ValueRW.Value;
                var spawnPointIndex = random.NextInt(0, spawnPoints.Length);
                spawnZoneRandom.ValueRW.Value = random; // Appliquer le nouvel état du random
                
                var spawnData = spawnPoints[spawnPointIndex];

                ecb.SetComponent(newBalloon, new LocalTransform
                {
                    Position = spawnData.SpawnPosition,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                var prefabRiseRate = _balloonRiseRateLookup[props.BasicBalloonPrefab];
                ecb.SetComponent(newBalloon, new BalloonRiseRate
                {
                    Value = prefabRiseRate.Value,
                    TargetHeight = prefabRiseRate.TargetHeight
                });

                balloonSpawnTimer.ValueRW.Value = props.BalloonSpawnRate;
            }
            
            spawnPoints.Dispose(); // Libération vitale du tableau Temp
        }
    }
}
