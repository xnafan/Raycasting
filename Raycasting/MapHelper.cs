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

        //public static float GetDistanceToObstacle(this int[,] map, Vector2 position, float angleInRadians)
        //{
        //    var collisionPosition = GetCollisionPoint(map, position, angleInRadians);
        //    if (collisionPosition.HasValue)
        //    {
        //        return Vector2.Distance(position, collisionPosition.Value);
        //    }
        //    else
        //    { return 1000; }
        //}

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

        public static Vector2? GetCollisionPointImproved(this int[,] map, Vector2 position, float angleInRadians, float maxDistance)
        {
            var closestHorizontalCollision = GetFirstHorizontalCoordinateIntersection(map, position, angleInRadians, maxDistance);
            
            var closestVerticalCollision = GetFirstVerticalCoordinateIntersection(map, position, angleInRadians, maxDistance);

            if (closestHorizontalCollision.HasValue && closestVerticalCollision.HasValue)
            {
                var distanceToHorizontalLine = Vector2.DistanceSquared(position, closestHorizontalCollision.Value);
                var distanceToVerticalLine = Vector2.DistanceSquared(position, closestVerticalCollision.Value);

                if (distanceToHorizontalLine < distanceToVerticalLine) { return closestHorizontalCollision; }
                else { return closestVerticalCollision; }
            }
            else if (closestVerticalCollision.HasValue) { return closestVerticalCollision; }
            else if (closestHorizontalCollision.HasValue) { return closestHorizontalCollision; }
            return null;
        }

        //public static Vector2? GetCollisionPointByOnlyCheckingWhenCrossingBoundariesBetweenSquares(this int[,] map, Player player)
        //{
        //    float maxDistance = 20;
        //    Vector2 directionOfRay = GetAngleAsVector(angleInRadians);
        //    Vector2 farthestVisiblePositionForRay = position + directionOfRay * maxDistance;
        //    Vector2? firstVerticalWallIntersection = GetFirstVerticalCoordinateIntersection(map, player, farthestVisiblePositionForRay, directionOfRay);

        //    return firstVerticalWallIntersection;
        //}

        public static Vector2? GetFirstVerticalCoordinateIntersection(this int[,] map, Vector2 position, float directionInRadian, float maxDistance)
        {
            int firstXCoordinateToCheck = 0;
            int lastXCoordinateToCheck = 0;
            if (directionInRadian == 0) { return new Vector2((float)Math.Ceiling(position.X), position.Y); }
            if (directionInRadian == 180) { return new Vector2((float)Math.Floor(position.X), position.Y); }
            if (directionInRadian == 90 || directionInRadian == 270) { return null; }

            

            Vector2 directionAsVector = directionInRadian.AngleAsVector();
            Vector2 endingPosition = position + (directionAsVector * maxDistance);
            Vector2 directionOfRay = new Vector2(Math.Sign(directionAsVector.X), (float)Math.Tan(directionInRadian));
            int xChange = (int)directionOfRay.X;
            float xTestOffset = xChange * .1f;
            int yChange = Math.Sign(directionOfRay.Y);
            float yTestOffset = yChange * .1f;

            if (directionAsVector.X == 0) { return null; }
            else if (directionAsVector.X < 0)
            {
                firstXCoordinateToCheck = (int)Math.Floor(position.X);
                lastXCoordinateToCheck = (int)Math.Ceiling(endingPosition.X);
            }
            else
            {
                firstXCoordinateToCheck = (int)Math.Ceiling(position.X);
                lastXCoordinateToCheck = (int)Math.Floor(endingPosition.X);
            }

            int numberOfXCoordinatesToCheck = Math.Abs(lastXCoordinateToCheck - firstXCoordinateToCheck) + 1;
            float xFraction = firstXCoordinateToCheck - position.X;
            float firstY = xFraction * directionOfRay.Y + position.Y;

            for (int deltaX = 0; deltaX < numberOfXCoordinatesToCheck; deltaX++)
            {
                float yCoordinate = firstY + directionOfRay.Y * deltaX;
                int xCoordinate = firstXCoordinateToCheck + deltaX * xChange;
                Vector2 coordinateToCheck = new Vector2(xCoordinate + xTestOffset, yCoordinate + yTestOffset);
                if (!map.Contains((int)coordinateToCheck.X, (int)coordinateToCheck.Y)) return null;
                if (map[(int)coordinateToCheck.X, (int)coordinateToCheck.Y] != 0)
                {
                    return new Vector2(xCoordinate, yCoordinate);
                }
            }
            return null;
        }

        public static Vector2? GetFirstHorizontalCoordinateIntersection(this int[,] map, Vector2 position, float directionInRadian, float maxDistance)
        {
            int firstYCoordinateToCheck = 0;
            int lastYCoordinateToCheck = 0;
            if (directionInRadian == 90) { return new Vector2(position.X, (float)Math.Floor(position.Y)); }
            if (directionInRadian == 270) { return new Vector2(position.X, (float)Math.Ceiling(position.Y)); }
            if (directionInRadian == 0 || directionInRadian == 180) { return null; }

            Vector2 directionAsVector = new Vector2((float)Math.Cos(directionInRadian), -(float)Math.Sin(directionInRadian));
            Vector2 endingPosition = position + (directionAsVector * maxDistance);
            Vector2 directionOfRay = new Vector2(Math.Sign(directionAsVector.X), -(float)Math.Tan(directionInRadian));
            int xChange = (int)directionOfRay.X;
            float xTestOffset = xChange * .1f;
            int yChange = Math.Sign(directionOfRay.Y);
            float yTestOffset = yChange * .1f;

            if (directionAsVector.Y == 0) { return null; }
            else if (directionAsVector.Y < 0)
            {
                firstYCoordinateToCheck = (int)Math.Floor(position.Y);
                lastYCoordinateToCheck = (int)Math.Ceiling(endingPosition.Y);
            }
            else
            {
                firstYCoordinateToCheck = (int)Math.Ceiling(position.Y);
                lastYCoordinateToCheck = (int)Math.Floor(endingPosition.Y);
            }

            float xPerY = 1 / directionOfRay.Y;
            int numberOfYCoordinatesToCheck = Math.Abs(lastYCoordinateToCheck - firstYCoordinateToCheck) + 1;
            float yFraction = firstYCoordinateToCheck - position.Y;

            for (int deltaY = 0; deltaY < numberOfYCoordinatesToCheck; deltaY++)
            {
                
                int yCoordinate = firstYCoordinateToCheck + deltaY * yChange;
                float yDifference = yCoordinate - position.Y;
                float xCoordinate = position.X + yDifference * xPerY;
                Vector2 coordinateToCheck = new Vector2(xCoordinate + xTestOffset, yCoordinate + yTestOffset);
                if (!map.Contains((int)coordinateToCheck.X, (int)coordinateToCheck.Y)) return null;
                if (map[(int)coordinateToCheck.X, (int)coordinateToCheck.Y] != 0)
                {
                    return new Vector2(xCoordinate, yCoordinate);
                }
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
