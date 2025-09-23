using System;
using UnityEngine;

namespace Remedy.Framework
{
    /// <summary>
    /// Allocation-free wrapper around Physics casts that applies
    /// common filtering logic and returns structured results.
    /// </summary>
    public static class RaycastUtility
    {
        public const int MaxHitCapacity = 32;

        /// <summary>
        /// Holds results of a single cast (fixed capacity).
        /// Note: Results are only valid until the next RaycastUtility call
        /// on the same thread, since they come from a shared scratch buffer.
        /// </summary>
        public struct RaycastResult
        {
            public int HitCount;
            public int ClosestHitIndex; // -1 if no valid hit
            public bool HasHit;

            public bool TryGetClosestHit(out RaycastHit hit)
            {
                if (!HasHit || ClosestHitIndex > HitCount) { hit = default; return false; }
                hit = s_ScratchBuffer[ClosestHitIndex];
                return true;
            }

            /// <summary>
            /// Exposes the underlying hits array.
            /// WARNING: only valid until the next RaycastUtility call.
            /// </summary>
            public ReadOnlySpan<RaycastHit> Hits
                => new ReadOnlySpan<RaycastHit>(s_ScratchBuffer, 0, HitCount);

            /// <summary>Copies valid hits into a caller-provided array.</summary>
            public void CopyHits(RaycastHit[] target)
            {
                int len = Mathf.Min(HitCount, target.Length);
                Array.Copy(s_ScratchBuffer, target, len);
            }
        }

        // Shared scratch buffer reused per thread.
        [ThreadStatic]
        private static RaycastHit[] s_ScratchBuffer;

        static RaycastUtility()
        {
            s_ScratchBuffer = new RaycastHit[MaxHitCapacity];
        }

        /// <summary>
        /// Perform a sphere cast with angle & distance filtering.
        /// </summary>
        public static RaycastResult SphereCast(
            Transform source,
            Vector3 origin,
            Vector3 direction,
            float radius,
            float distance,
            LayerMask mask,
            float minAngleDeg = -1f,
            float maxAngleDeg = -1f,
            float validDistance = Mathf.Infinity)
        {
            if (s_ScratchBuffer == null)
                s_ScratchBuffer = new RaycastHit[MaxHitCapacity];

            RaycastResult result = default;
            result.ClosestHitIndex = -1;

            int hitCount = Physics.SphereCastNonAlloc(
                origin -= direction * (radius * 0.5f),
                radius,
                direction,
                s_ScratchBuffer,
                distance,
                mask
            );

            float minDot = (minAngleDeg > 0) ? Mathf.Cos(minAngleDeg * Mathf.Deg2Rad) : -1f;
            float maxDot = (maxAngleDeg > 0) ? Mathf.Cos(maxAngleDeg * Mathf.Deg2Rad) : 1f;

            float closestDist = float.MaxValue;
            int validCount = 0;

            for (int i = 0; i < hitCount && validCount < MaxHitCapacity; i++)
            {
                var hit = s_ScratchBuffer[i];

                if (hit.transform == null || hit.transform == source) continue;
                if (hit.distance > validDistance) continue;
                if (hit.point == default) continue;

                float dot = Vector3.Dot(hit.normal, direction);
                if (minAngleDeg > 0 && dot < minDot) continue;
                if (maxAngleDeg > 0 && dot > maxDot) continue;

                // Keep track of closest
                if (hit.distance < closestDist)
                {
                    closestDist = hit.distance;
                    result.ClosestHitIndex = validCount;
                    result.HasHit = true;
                }

                // Store valid hit compacted in front
                s_ScratchBuffer[validCount++] = hit;
            }

            result.HitCount = validCount;
            return result;
        }
    }
}
