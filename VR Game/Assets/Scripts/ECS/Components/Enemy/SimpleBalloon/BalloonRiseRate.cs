using Unity.Entities;

namespace ECS.Components
{
    public struct BalloonRiseRate : IComponentData, IEnableableComponent
    {
        public float Value;
        public float TargetHeight;
    }
}