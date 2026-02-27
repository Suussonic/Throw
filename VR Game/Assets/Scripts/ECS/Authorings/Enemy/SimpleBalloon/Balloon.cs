using ECS.Components;
using ECS.Components.Balloon;
using UnityEngine;

namespace ECS.Authoring
{
    public class Balloon : MonoBehaviour
    {
        public float riseRate;
        public float walkSpeed;
        public Transform target;
        public int hasTarget;
        public int canWalk;
        public float targetHeight;
    }
    
    public class GoblinBaker : Unity.Entities.Baker<Balloon>
    {
        public override void Bake(Balloon authoring)
        {
            var entity = GetEntity(Unity.Entities.TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECS.Components.BalloonRiseRate
            {
                Value = authoring.riseRate,
                TargetHeight = authoring.targetHeight
            });
            if (authoring.canWalk == 1)
            {
                AddComponent(entity, new BalloonWalkProperties
                {
                    WalkSpeed = authoring.walkSpeed
                });
                SetComponentEnabled<BalloonWalkProperties>(entity, false); // Désactivé par défaut, sera activé après le Rise
            }
            if (authoring.hasTarget == 1) {
                AddComponent<BalloonHeading>(entity);
            }
        }
    }
}