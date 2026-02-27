using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Components
{
    public readonly partial struct BalloonRiseAspect : IAspect
    {
        public readonly Entity Entity;
        
        private readonly RefRW<LocalTransform> _transform;
        private readonly RefRO<BalloonRiseRate> _balloonRiseRate;
        
        public void Rise(float deltaTime)
        {
            _transform.ValueRW.Position += math.up() * _balloonRiseRate.ValueRO.Value * deltaTime;
        }
        
        public bool IsAboveGround => _transform.ValueRO.Position.y >= _balloonRiseRate.ValueRO.TargetHeight;

        public void SetAtGroundLevel()
        {
            var position = _transform.ValueRW.Position;
            position.y = _balloonRiseRate.ValueRO.TargetHeight;
            _transform.ValueRW.Position = position;
        }
    }
}