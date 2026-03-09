using ECS.Components.Enemy.AgressiveBalloon;
using Player;
using Unity.Entities;

namespace ECS.Systems.Enemy.AgressiveBalloon
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BalloonWalkSystem))]
    public partial class BalloonDamageSystem : SystemBase
    {
        private const int DamagePerBalloon = 10;

        protected override void OnCreate()
        {
            RequireForUpdate<BalloonReachedTarget>();
        }

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);

            foreach (var (_, entity) in SystemAPI.Query<BalloonReachedTarget>().WithEntityAccess())
            {
                var playerHealth = PlayerHealthRef.Instance;
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    playerHealth.TakeDamage(DamagePerBalloon);
                }

                ecb.DestroyEntity(entity);
            }
        }
    }
}
