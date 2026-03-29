
using Unity.Mathematics;

namespace ECS.Helpers
{
    public static class MathHelpers
    {
        /// <summary>
        /// Calculates the heading in radians from objectPosition to targetPosition.
        /// </summary>
        public static float GetHeading(float3 objectPosition, float3 targetPosition)
        {
            var x = objectPosition.x - targetPosition.x;
            var y = objectPosition.z - targetPosition.z;
            return math.atan2(y, x) + math.PI;
        }
    }
}