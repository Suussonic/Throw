using ECS.Components.Balloon; 
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Components
{
    public readonly partial struct BalloonWalkAspect : IAspect
    {
        public  readonly Entity Entity;
        private readonly RefRW<LocalTransform> _transform;
        private readonly RefRO<BalloonWalkProperties> _balloonWalkSpeed;
        private readonly RefRO<BalloonHeading> _balloonHeading;
        
        private float WalkSpeed => _balloonWalkSpeed.ValueRO.WalkSpeed;
        public float3 Heading => _balloonHeading.ValueRO.Value;
        public float3 Position => _transform.ValueRO.Position;
        
        public void Walk(float deltaTime)
        {
            // Calcule la direction vers le heading (cible)
            float3 direction = math.normalizesafe(Heading - _transform.ValueRO.Position);
            
            // Si la direction est valide, on avance vers elle
            if (math.lengthsq(direction) > 0.001f)
            {
                // Déplace le gobelin vers la direction cible
                _transform.ValueRW.Position += direction * WalkSpeed * deltaTime;
                
                // Fait tourner le gobelin pour qu'il regarde vers la direction de déplacement
                _transform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());
            }
        }
        
        public bool IsInStoppingRange(float3 targetPosition, float stoppingDistance)
        {
            return math.distancesq(targetPosition, _transform.ValueRO.Position) <= stoppingDistance ; 
        }
    }
}