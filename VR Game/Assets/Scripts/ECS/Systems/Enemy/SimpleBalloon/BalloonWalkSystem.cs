using ECS.Components;
using ECS.Components.Balloon;
using Unity.Burst;
using Unity.Entities;

namespace ECS.Systems.Enemy.SimpleBalloon
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BalloonTargetSystem))]
    public partial struct BalloonWalkSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BalloonWalkProperties>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            new BalloonWalkJob
            {
                DeltaTime = deltaTime,
                StopDistanceSq = 0.5f * 0.5f,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(BalloonWalkProperties))]
    public partial struct BalloonWalkJob : IJobEntity
    {
        public float DeltaTime;
        public float StopDistanceSq;
        public EntityCommandBuffer.ParallelWriter ECB;
        
        private void Execute(BalloonWalkAspect balloon, [EntityIndexInQuery] int sortKey)
        {
            balloon.Walk(DeltaTime);
            
            if (balloon.IsInStoppingRange(balloon.Heading, StopDistanceSq))
            {
                ECB.SetComponentEnabled<BalloonWalkProperties>(sortKey, balloon.Entity, false);
            }
        }
    }
}