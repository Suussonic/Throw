using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.Enemy.AgressiveBalloon
{
    public struct BalloonWalkProperties : IComponentData , IEnableableComponent
    {
        public float WalkSpeed;
    }
    
    public struct BalloonHeading : IComponentData , IEnableableComponent
    {
        public float3 Value;
    }
}