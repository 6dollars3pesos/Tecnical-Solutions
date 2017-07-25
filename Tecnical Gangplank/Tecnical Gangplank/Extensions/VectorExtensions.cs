using Aimtec;
using Aimtec.SDK.Extensions;

namespace TecnicalGangplank.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 ReduceToMaxDistance(this Vector3 middle, Vector3 other, float maxDistance)
        {
            return middle.Distance(other) <= maxDistance ? other : middle.Extend(other, maxDistance);
        }

        public static Vector3 ReduceToMaxDistance(this GameObject middle, GameObject other, float maxDistance)
        {
            return ReduceToMaxDistance(middle.Position, other.Position, maxDistance);
        }
    }
}