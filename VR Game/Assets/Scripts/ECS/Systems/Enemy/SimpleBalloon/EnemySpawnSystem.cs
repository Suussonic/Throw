using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using ECS.Components;

namespace ECS.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemySpawnSystem : ISystem

    {
    public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            EntityCommandBuffer ecb =
                new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var spawner in
                     SystemAPI.Query<RefRW<EnemySpawner>>())
            {
                spawner.ValueRW.Timer += dt;

                if (spawner.ValueRO.Timer >= spawner.ValueRO.SpawnRate)
                {
                    spawner.ValueRW.Timer = 0f;

                    Entity enemy =
                        ecb.Instantiate(spawner.ValueRO.EnemyPrefab);

                    ecb.SetComponent(enemy, new LocalTransform
                    {
                        Position = spawner.ValueRO.SpawnPosition,
                        Rotation = Unity.Mathematics.quaternion.identity,
                        Scale = 1f
                    });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}