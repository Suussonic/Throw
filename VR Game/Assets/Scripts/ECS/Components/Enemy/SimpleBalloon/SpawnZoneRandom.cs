using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.Enemy.SimpleBalloon
{
    public struct SpawnZoneRandom : IComponentData
    {
        public Random Value;
    }
}