using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct SpawnZoneRandom : IComponentData
    {
        public Random Value;
    }
}