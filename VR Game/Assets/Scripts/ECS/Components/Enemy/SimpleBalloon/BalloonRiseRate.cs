using Unity.Entities;

namespace ECS.Components.Enemy.SimpleBalloon
{
    public struct BalloonRiseRate : IComponentData, IEnableableComponent
    {
        public float Value;
        public float TargetHeight;
    }
}