using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Authorings.Enemy.Balloon
{
    public struct BalloonTargetPosition : IComponentData
    {
        public float3 Value;
    }
}