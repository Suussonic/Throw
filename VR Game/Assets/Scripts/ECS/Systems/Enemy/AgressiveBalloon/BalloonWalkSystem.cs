using ECS.Authorings.Enemy.Balloon;
using ECS.Components.Enemy.AgressiveBalloon;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems.Enemy.AgressiveBalloon
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimpleBalloon.BalloonRiseSystem))]
    public partial struct BalloonWalkSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BalloonWalkProperties>();
            state.RequireForUpdate<BalloonTargetPosition>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var deltaTime = SystemAPI.Time.DeltaTime;
            var targetPosition = SystemAPI.GetSingleton<BalloonTargetPosition>().Value;
            
            new BalloonWalkJob
            {
                DeltaTime = deltaTime,
                StopDistanceSq = 0.5f,
                TargetPosition = targetPosition,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(BalloonWalkProperties))]
    [WithNone(typeof(BalloonReachedTarget))] // <-- ESSENTIEL: Filtre pour ignorer ceux qui ont déjà touché la cible
    public partial struct BalloonWalkJob : IJobEntity
    {
        public float DeltaTime;
        public float StopDistanceSq;
        public float3 TargetPosition;
        public EntityCommandBuffer.ParallelWriter ECB;
        
        private void Execute(Entity entity, ref LocalTransform transform, in BalloonWalkProperties properties, ref BalloonHeading heading, [EntityIndexInQuery] int sortKey)
        {
            heading.Value = TargetPosition;
            
            // 2. Vérification de la distance d'arrêt AVANT le déplacement
            // On vérifie d'abord si on est au contact du joueur pour arrêter de le traquer
            if (math.distancesq(TargetPosition, transform.Position) <= StopDistanceSq)
            {
                ECB.AddComponent<BalloonReachedTarget>(sortKey, entity);
                return; // On arrête là pour cette frame
            }

            // 3. Déplacement (Walk)
            float3 direction = math.normalizesafe(heading.Value - transform.Position);
            if (math.lengthsq(direction) > 0.001f)
            {
                transform.Position += direction * properties.WalkSpeed * DeltaTime;
            }
        }
    }
}
