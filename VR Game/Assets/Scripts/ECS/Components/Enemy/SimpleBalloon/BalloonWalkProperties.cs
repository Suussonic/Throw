using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.Balloon
{
    public struct BalloonWalkProperties : IComponentData , IEnableableComponent
    {
        public float WalkSpeed;
    }
    
    public struct BalloonHeading : IComponentData
    {
        public float3 Value;
    }
}