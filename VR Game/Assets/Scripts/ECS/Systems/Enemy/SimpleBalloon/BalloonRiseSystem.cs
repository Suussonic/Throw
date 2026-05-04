using ECS.Components;
using ECS.Components.Enemy.AgressiveBalloon;
using ECS.Components.Enemy.SimpleBalloon;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems.Enemy.SimpleBalloon
{
    [BurstCompile]
    [UpdateAfter(typeof(SpawnBalloonSystem))]
    public partial struct BalloonRiseSystem : ISystem
    {
        private ComponentLookup<BalloonWalkProperties> _canWalkLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _canWalkLookup = state.GetComponentLookup<BalloonWalkProperties>(true);
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            _canWalkLookup.Update(ref state);
            new BalloonRiseSystemJob
            {
                DeltaTime = deltaTime,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                CanWalk = _canWalkLookup,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct BalloonRiseSystemJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        [ReadOnly] public ComponentLookup<BalloonWalkProperties> CanWalk;

        private void Execute(Entity entity, ref LocalTransform transform, in BalloonRiseRate riseRate, [EntityIndexInQuery] int sortKey)
        {
            // 1. Rise
            transform.Position += math.up() * riseRate.Value * DeltaTime;

            // 2. Vérification de la limite de hauteur (IsAboveLimit)
            if (transform.Position.y >= riseRate.TargetHeight)
            {
                ECB.RemoveComponent<BalloonRiseRate>(sortKey, entity);
                if (CanWalk.HasComponent(entity))
                {
                    ECB.SetComponentEnabled<BalloonWalkProperties>(sortKey, entity, true);
                    ECB.SetComponentEnabled<BalloonHeading>(sortKey, entity, true);
                }
                else
                {
                    ECB.DestroyEntity(sortKey, entity);
                }
            }
        }
    }
}
