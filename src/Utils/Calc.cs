using UnityEngine;

namespace HairLetLoose
{
    public static class Calc
    {
        public static float Roll(Quaternion q)
        {
            return Mathf.Rad2Deg * Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);
        }

        public static float Pitch(Quaternion q)
        {
            return Mathf.Rad2Deg * Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
        }

        //This is used to scale pitch effect by roll angle's distance from 90/-90 = node is sideways
        //-> if node is sideways, pitch related adjustments have less effect
        public static float RollFactor(float roll)
        {
            return (90 - Mathf.Abs(roll)) / 90;
        }

        public static float UprightFactor(float rollFactor, float pitch, float angleOffset)
        {
            float pitchFactor = (180 - pitch) / 180;
            return rollFactor + pitchFactor / (rollFactor + pitchFactor);
            //if (pitch < angleOffset)
            //{
            //    return 1;
            //}

            //return (270 - pitch - angleOffset) / (270 - 2 * angleOffset);

            // roll = 90
            // pitch -90 .. 0 .. 90
            // same effect at -90 and 90 as at 0...
            //
        }

        public static float UpsideDownFactor(float rollFactor, float pitch, float angleOffset)
        {
            return rollFactor * (180 - pitch) / 180;
            //if(pitch < angleOffset)
            //{
            //    return 1;
            //}

            //return (270 - pitch - angleOffset) / (270 - 2 * angleOffset);
        }

        public static float RoundToDecimals(float value, float roundFactor)
        {
            return Mathf.Round(value * roundFactor) / roundFactor;
        }
    }
}
