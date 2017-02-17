using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Raycasting.Input
{
    public class DirectionData : IEquatable<DirectionData>
    {

        static Random _rnd = new Random();
        public static DirectionData[] AllDirections { private set; get; }
        public static DirectionData Up = new DirectionData() {RelativePosition = new Vector2(0,-1), DirectionInDegrees = 90 };
        public static DirectionData Down = new DirectionData() { RelativePosition = new Vector2(0, 1), DirectionInDegrees = 270 };

        public static DirectionData Left = new DirectionData() { RelativePosition = new Vector2(-1, 0), DirectionInDegrees = 180 };
        public static DirectionData Right = new DirectionData() { RelativePosition = new Vector2(1, 0), DirectionInDegrees = 0 };

        static DirectionData()
        {
            AllDirections = new DirectionData[] { Up, Down, Left, Right };
        }

        public Vector2 RelativePosition { set; get; }
        public float DirectionInDegrees { set; get; }

        public static DirectionData GetRandomDirection() { return AllDirections.GetRandomElement(); }
        public static IEnumerable<DirectionData> GetRightAndLeftInRandomOrder(DirectionData direction)
        {
            List<DirectionData> rightAndLeft;

            if (direction == Up || direction == Down)
            {
                rightAndLeft = new List<DirectionData>() { Left, Right };
            }
            else
            {
                rightAndLeft = new List<DirectionData>() { Up, Down};
            }
            if (_rnd.Next(2) == 0)
            {rightAndLeft.Reverse(); }
            return rightAndLeft;
        }

        public static DirectionData DegreesToDirectionData(int angle)
        {
            while (angle < 0){angle += 360;}
            while (angle > 360){angle -= 360;}
            if ((angle >= 315 && angle <= 360) || (angle >= 0 && angle <= 45)) { return Right; }
            if (angle > 45 && angle <= 135) { return Up; }
            if (angle > 135 && angle <= 225) { return Left; }
            else { return Down; }
        }

        public static DirectionData GetOppositeDirection(DirectionData direction)
        {
            if (direction.RelativePosition == Up.RelativePosition)
            { return Down; }
            if (direction.RelativePosition == Down.RelativePosition)
            { return Up; }
            if (direction.RelativePosition == Right.RelativePosition)
            { return Left; }
            else return Right; 

        }

        public bool Equals(DirectionData other)
        {
            return this.DirectionInDegrees == other.DirectionInDegrees;
        }
    }
}