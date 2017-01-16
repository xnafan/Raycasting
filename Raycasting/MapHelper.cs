using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public static class MapHelper
    {

        public static float ToRadians(this Vector2 angleVector)
        {
            return (float)Math.Atan2(angleVector.X, -angleVector.Y);
        }

        public static Vector2? GetCollisionPoint(this int[,] map, Vector2 position, float angleInRadians)
        {
            float stepSize = 0.001f;
            float maxDistance = 20;
            Vector2 directionOfRay = AngleAsVector(angleInRadians);
            for (float distance = stepSize; distance < maxDistance; distance += stepSize)
            {
                Vector2 positionToTest = position + directionOfRay * distance;
                if (map[(int)positionToTest.X, (int)positionToTest.Y] != 0)
                    return positionToTest;
            }
            return null;
        }

        public static CollisionInfo? GetCollisionPointImproved(this int[,] map, Vector2 position, float angleInRadians, float maxDistance)
        {
            var closestHorizontalCollision = GetHorizontalCollision(map, position, angleInRadians, maxDistance);
            
            var closestVerticalCollision = GetVerticalCollision(map, position, angleInRadians, maxDistance);

            if (closestHorizontalCollision.HasValue && closestVerticalCollision.HasValue)
            {
                var distanceToHorizontalLine = Vector2.DistanceSquared(position, closestHorizontalCollision.Value.CollisionPoint);
                var distanceToVerticalLine = Vector2.DistanceSquared(position, closestVerticalCollision.Value.CollisionPoint);

                if (distanceToHorizontalLine < distanceToVerticalLine) { return closestHorizontalCollision; }
                else { return closestVerticalCollision; }
            }
            else if (closestVerticalCollision.HasValue) { return closestVerticalCollision; }
            else if (closestHorizontalCollision.HasValue) { return closestHorizontalCollision; }
            return null;
        }


        public static CollisionInfo? GetVerticalCollision(this int[,] map, Vector2 position, float directionInDegrees, float maxDistance)
        {
            if (directionInDegrees == 90 || directionInDegrees == 270) { return null; }

            int firstXCoordinateToCheck = 0;
            int lastXCoordinateToCheck = 0;
            var directionInRadian = MathHelper.ToRadians(directionInDegrees);
            Vector2 directionAsVector = directionInRadian.AngleAsVector();

            if (directionAsVector.X < 0)
            {
                firstXCoordinateToCheck = (int)Math.Floor(position.X);
                lastXCoordinateToCheck = 0;
            }
            else
            {
                firstXCoordinateToCheck = (int)Math.Ceiling(position.X);
                lastXCoordinateToCheck = map.GetLength(0)-1;
            }
            int deltaX = Math.Sign(lastXCoordinateToCheck - firstXCoordinateToCheck);
            var line = LineFormula.FromCoordinateAndDirection(position, directionInRadian);
            bool done = false;
            for (int x = firstXCoordinateToCheck; !done; x += deltaX)
            {
                float? y = 0;
                if (directionInDegrees == 0 || directionInDegrees == 180){y = position.Y;}
                else{y = line.GetInterSectWithVerticalLine(x);}
                
                var realX = x - (deltaX < 0 ? 1 : 0);
                if (!y.HasValue || !map.Contains(realX, (int)y.Value)) { return null; }
                if (map[realX, (int)y.Value] != 0)
                {
                    return new CollisionInfo()
                    {
                    CollisionPoint = new Vector2(x, y.Value),
                    PositionOnWall = y.Value - (int)y.Value,
                    TileHit = new Point(realX, (int)y.Value),
                };
                }
                if(x == lastXCoordinateToCheck) { done = true; }
            }
            return null;
        }

        public static CollisionInfo? GetHorizontalCollision(this int[,] map, Vector2 position, float directionInDegrees, float maxDistance)
        {
            
            int firstYCoordinateToCheck = 0;
            int lastYCoordinateToCheck = 0;
            
            if (directionInDegrees == 0 || directionInDegrees == 180) { return null; }
            float directionInRadian = (float) MathHelper.ToRadians(directionInDegrees);

            Vector2 directionAsVector = new Vector2((float)Math.Cos(directionInRadian), (float)Math.Sin(directionInRadian));

            if (directionAsVector.Y < 0)
            {
                firstYCoordinateToCheck = (int)Math.Floor(position.Y);
                lastYCoordinateToCheck = 0;
            }
            else
            {
                firstYCoordinateToCheck = (int)Math.Ceiling(position.Y);
                lastYCoordinateToCheck = map.GetLength(1)-1;
            }

            int deltaY = Math.Sign(lastYCoordinateToCheck - firstYCoordinateToCheck);
            var line = LineFormula.FromCoordinateAndDirection(position, directionInRadian);
            bool done = false;
            for (int y = firstYCoordinateToCheck; !done; y += deltaY)
            {
                float? x = 0;
                if (directionInDegrees == 90 || directionInDegrees == 270){x = position.X;}
                else{ x = line.GetInterSectWithHorizontalLine(y); }
                var realY = y - (deltaY < 0 ? 1 : 0);
                if (!x.HasValue || !map.Contains((int)x.Value,realY)) { return null; }
                if (map[(int)x.Value, realY] != 0)
                {
                    return new CollisionInfo()
                    {
                        CollisionPoint = new Vector2(x.Value, y),
                        PositionOnWall = x.Value - (int)x.Value,
                        TileHit = new Point((int)x.Value, realY)

                    };
                }
                if (y == lastYCoordinateToCheck) { done = true; }
            }
            return null;
        }

        public static Vector2 AngleAsVector(this float angle)
        {
            var newDirection = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            newDirection.Normalize();
            return newDirection;
        }
        public static bool IsBlocked(this int[,] map, Vector2 position)
        {
            return map[(int)position.X, (int)position.Y] != 0;
        }

        public static float GetPositionOnWall(this Vector2 position)
        {
            float xResulting = 0;
            var xFraction = (position.X - (int)position.X);
            if (xFraction > .5f) { xResulting = Math.Abs(1 - xFraction); }
            else { xResulting = xFraction; }

            float yResulting = 0;
            var yFraction = (position.Y - (int)position.Y);
            if (yFraction > .5f) { yResulting = Math.Abs(1 - yFraction); }
            else { yResulting = yFraction; }

            if (xResulting > yResulting) { return xFraction; }
            else { return yFraction; }
        }


        public static bool Contains(this int[,] map, int x, int y)
        {
            return x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
        }
    }




}
