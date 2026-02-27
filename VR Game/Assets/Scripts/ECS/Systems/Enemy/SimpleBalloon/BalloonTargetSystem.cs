using ECS.Components;
using ECS.Components.Balloon;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace ECS.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BalloonRiseSystem))]
    [UpdateBefore(typeof(Enemy.SimpleBalloon.BalloonWalkSystem))]
    public partial struct BalloonTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BalloonHeading>();
        }

        public void OnUpdate(ref SystemState state)
        {
            
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
