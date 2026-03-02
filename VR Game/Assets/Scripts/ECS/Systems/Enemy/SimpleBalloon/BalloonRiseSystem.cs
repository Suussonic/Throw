using ECS.Components;
using ECS.Components.Balloon;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

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

        private void Execute(BalloonRiseAspect balloon, [EntityIndexInQuery] int sortKey)
        {
            balloon.Rise(DeltaTime);

            if (balloon.IsAboveLimit)
            {
                ECB.RemoveComponent<BalloonRiseRate>(sortKey, balloon.Entity);
                if (CanWalk.HasComponent(balloon.Entity))
                {
                    ECB.SetComponentEnabled<BalloonWalkProperties>(sortKey, balloon.Entity, true);
                    ECB.SetComponentEnabled<BalloonHeading>(sortKey, balloon.Entity, true);
                }
                else
                {
                    ECB.DestroyEntity(sortKey, balloon.Entity);
                }
            }
        }
    }
}
