using Unity.Entities;

namespace ECS.Components
{
    public struct BalloonRiseRate : IComponentData
    {
        public float Value;
        public float TargetHeight;
    }
}