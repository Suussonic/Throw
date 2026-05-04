using ECS.Authorings.Enemy.Balloon;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authorings.Enemy.Balloon
{
    public class BalloonTargetAuthoring : MonoBehaviour
    {
        private Entity _targetEntity;
        private EntityManager _entityManager;
        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new BalloonTargetPosition
            {
                Value = transform.position
            });
        }

        private void Update()
        {
            if (_entityManager != default && _entityManager.Exists(_targetEntity))
            {
                _entityManager.SetComponentData(_targetEntity, new BalloonTargetPosition
                {
                    Value = transform.position
                });
            }
        }
        
        private void OnDestroy()
        {
            if (_entityManager != default && _entityManager.Exists(_targetEntity))
            {
                _entityManager.DestroyEntity(_targetEntity);
            }
        }
    }
}