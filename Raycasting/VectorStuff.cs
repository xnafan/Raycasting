using System;
using Microsoft.Xna.Framework;

namespace Util
{
    public static class VectorStuff
    {

        private const double PiTimesTwo = 2 * Math.PI;

        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }
        public static Vector2 AngleToVector2(float angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static float ToAngle(this Vector2 vector)
        {
            return (float)Math.Atan2(vector.X, -vector.Y);
        }

        public static float XnaVectorToAngle(this Vector2 vector)
        {
            return (float)Math.Atan2(-vector.Y, vector.X);
        }

        public static float VectorToAnglePositive(Vector2 vector)
        {
            return ToAngle(vector) + (float)PiTimesTwo;
        }

        public static float VectorToAngleSmallestPositive(Vector2 vector)
        {
            float angle = VectorToAnglePositive(vector);
            while (angle > PiTimesTwo)
            {
                angle -= (float)PiTimesTwo;
            }
            return angle;
        }

        public static Vector2 ProjectVector(Vector2 source, Vector2 target)
        {
            float dotProduct = Vector2.Dot(source, target);

            Vector2 projectedVector = (dotProduct / target.LengthSquared()) * target;
            return projectedVector;
        }

        public static Vector2 GetNearestToOrigo(Vector2 vec1, Vector2 vec2, Vector2 origo)
        {
            if ((vec1 - origo).LengthSquared() < (vec2 - origo).LengthSquared())
                return vec1;
            return vec2;
        }

        

        /// <summary>
        /// Returns true if testVector is between vector1 and vector2
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="testVector"></param>
        /// <returns></returns>
        public static bool IsBetween(this Vector2 testVector, Vector2 vector1, Vector2 vector2)
        {
            //Vinklerne beregnes vi lægger 2 PI til for at undgå negative vinkler
            double angle1 = ToAngle(vector1);
            double angle2 = ToAngle(vector2);
            double testAngle = ToAngle(testVector);

            if (angle2 > angle1 && angle2 - angle1 > Math.PI)
            {
                angle1 = angle1 + PiTimesTwo;
            }
            else if (angle1 - angle2 > Math.PI)
            {
                angle2 = angle2 + PiTimesTwo;
            }

            double firstAngle = Math.Min(angle1, angle2);
            double lastAngle = Math.Max(angle1, angle2);

            if (IsBetween(firstAngle, lastAngle, testAngle))
                return true;

            if (IsBetween(firstAngle, lastAngle, testAngle + PiTimesTwo))
                return true;

            if (IsBetween(firstAngle, lastAngle, testAngle - PiTimesTwo))
                return true;

            return false;
        }

        private static bool IsBetween(double angle1, double angle2, double testAngle)
        {
            if (testAngle < angle1)
                return false;

            if (testAngle > angle2)
                return false;

            return true;
        }

        public static Vector2 GetPositionOfSatellite(Vector2 center, float distance, float directionInRadians)
        {
            float yDifference = (float)Math.Sin(directionInRadians);
            float xDifference = (float)Math.Cos(directionInRadians);
            Vector2 direction = new Vector2(xDifference, yDifference);
            Vector2 precisePositionOfSatellite = center + direction * distance;
            return precisePositionOfSatellite;
        }
    }
}
