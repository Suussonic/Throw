using ECS.Components.Enemy.AgressiveBalloon;
using ECS.Components.Enemy.SimpleBalloon;
using UnityEngine;

namespace ECS.Authorings.Enemy.Balloon
{
    public class Balloon : MonoBehaviour
    {
        public float riseRate;
        public float walkSpeed;
        public int hasTarget;
        public int canWalk;
        public float targetHeight;
    }
    
    public class GoblinBaker : Unity.Entities.Baker<Balloon>
    {
        public override void Bake(Balloon authoring)
        {
            var entity = GetEntity(Unity.Entities.TransformUsageFlags.Dynamic);
            AddComponent(entity, new BalloonRiseRate
            {
                Value = authoring.riseRate,
                TargetHeight = authoring.targetHeight
            });
            if (authoring.hasTarget == 1 || authoring.canWalk == 1)
            {
                AddComponent(entity, new BalloonWalkProperties
                {
                    WalkSpeed = authoring.walkSpeed
                });
                SetComponentEnabled<BalloonWalkProperties>(entity, false);
                AddComponent(entity, new BalloonHeading());
                SetComponentEnabled<BalloonHeading>(entity, false);
            }
        }
    }
}