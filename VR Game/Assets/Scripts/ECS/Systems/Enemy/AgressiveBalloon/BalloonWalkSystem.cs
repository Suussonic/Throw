using ECS.Authorings.Enemy.Balloon;
using ECS.Components.Balloon;
using ECS.Components.Enemy.AgressiveBalloon;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using BalloonWalkAspect = ECS.Components.Enemy.AgressiveBalloon.BalloonWalkAspect;

namespace ECS.Systems.Enemy.AgressiveBalloon
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimpleBalloon.BalloonRiseSystem))]
    public partial struct BalloonWalkSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BalloonWalkProperties>();
            state.RequireForUpdate<BalloonTargetPosition>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var deltaTime = SystemAPI.Time.DeltaTime;
            var targetPosition = SystemAPI.GetSingleton<BalloonTargetPosition>().Value;
            
            new BalloonWalkJob
            {
                DeltaTime = deltaTime,
                StopDistanceSq = 0.5f,
                TargetPosition = targetPosition,
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
        public float3 TargetPosition;
        public EntityCommandBuffer.ParallelWriter ECB;
        
        private void Execute(BalloonWalkAspect balloon, [EntityIndexInQuery] int sortKey)
        {
            balloon.SetHeading(TargetPosition);
            
            balloon.Walk(DeltaTime);
            
            if (balloon.IsInStoppingRange(balloon.Heading, StopDistanceSq))
            {
                ECB.AddComponent<BalloonReachedTarget>(sortKey, balloon.Entity);
            }
        }
    }
}