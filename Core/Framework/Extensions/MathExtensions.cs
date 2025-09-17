using UnityEngine;

namespace Remedy.Framework
{
	public static class MathExtensions
	{
		public static void MoveAtAngle(this Vector2 vector, float angle)
		{
			vector.x += (int)Mathf.Cos(angle);
			vector.y += (int)Mathf.Sin(angle);
		}

		// Function to set the value of a specific bit based on the angle
		public static int SetBit(this int integer, int index, int value)
		{
			// Set or clear the bit at the calculated index
			if (value == 1)
				integer |= 1 << index; // Set bit
			else
				integer &= ~(1 << index); // Clear bit
			return integer;
		}


        public static float ClampAngle(this float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

		public static Vector3 RandomRange(this Vector3 vector, float min, float max)
		{
			return new(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
		}
    }
}