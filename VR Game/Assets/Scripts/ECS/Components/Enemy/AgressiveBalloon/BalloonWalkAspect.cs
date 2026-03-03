using ECS.Components.Balloon;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Components.Enemy.AgressiveBalloon
{
    public readonly partial struct BalloonWalkAspect : IAspect
    {
        public  readonly Entity Entity;
        private readonly RefRW<LocalTransform> _transform;
        private readonly RefRO<BalloonWalkProperties> _balloonWalkSpeed;
        private readonly RefRW<BalloonHeading> _balloonHeading;
        
        private float WalkSpeed => _balloonWalkSpeed.ValueRO.WalkSpeed;
        public float3 Heading => _balloonHeading.ValueRO.Value;
        public float3 Position => _transform.ValueRO.Position;
        
        public void SetHeading(float3 target)
        {
            _balloonHeading.ValueRW.Value = target;
        }
        
        public void Walk(float deltaTime)
        {
        float3 direction = math.normalizesafe(Heading - _transform.ValueRO.Position);
            
            if (math.lengthsq(direction) > 0.001f)
            {
                _transform.ValueRW.Position += direction * WalkSpeed * deltaTime;
                
            }
        }
        
        public bool IsInStoppingRange(float3 targetPosition, float stoppingDistance)
        {
            return math.distancesq(targetPosition, _transform.ValueRO.Position) <= stoppingDistance ; 
        }
    }
}