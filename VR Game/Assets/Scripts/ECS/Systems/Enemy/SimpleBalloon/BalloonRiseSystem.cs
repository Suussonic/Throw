using ECS.Components;
using ECS.Components.Balloon;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

namespace ECS.Systems
{
    [BurstCompile]
    [UpdateAfter(typeof(SpawnBalloonSystem))]
    public partial struct BalloonRiseSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            new BalloonRiseSystemJob
            {
                DeltaTime = deltaTime,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                CanWalk = state.GetComponentLookup<BalloonWalkProperties>(true),
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct BalloonRiseSystemJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        [ReadOnly] public ComponentLookup<BalloonWalkProperties> CanWalk;
        
        private void Execute(BalloonRiseAspect balloon, [EntityIndexInQuery] int sortKey)
        {
            balloon.Rise(DeltaTime);
            
            if (balloon.IsAboveLimit)
            {
                ECB.RemoveComponent<BalloonRiseRate>(sortKey, balloon.Entity);
                if (CanWalk.HasComponent(balloon.Entity))
                {
                    ECB.SetComponentEnabled<BalloonWalkProperties>(sortKey, balloon.Entity, true);
                }
                else
                {
                    ECB.DestroyEntity(sortKey, balloon.Entity);
                }
            }
            
        }
    }
}