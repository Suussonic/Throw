using ECS.Authorings.Enemy.Balloon;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authorings.Enemy.Balloon
{
    public class BalloonTargetAuthoring : MonoBehaviour
    {
        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new BalloonTargetPosition
            {
                Value = transform.position
            });
        }
    }
}