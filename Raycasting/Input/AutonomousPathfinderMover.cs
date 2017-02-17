using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Util;

namespace Raycasting.Input
{
    public class AutonomousPathfinderMover : IPlayerMover
    {
        private bool _reversing = false;
        private PathFinderInfo _lastTargetReached;
        public static PathFinderInfo? _nextTarget;
        float _amountLeftToTurn = 0;
        public struct PathFinderInfo
        {
            public Vector2 Target { get; set; }
            public DirectionData Direction { get; set; }
        }

        Random _rnd = new Random();

        public AutonomousPathfinderMover(Player player)
        {
            TeleportPlayerToCenterOfCurrentTile(player);
            _lastTargetReached = new Input.AutonomousPathfinderMover.PathFinderInfo()
            {
                Direction = DirectionData.Right,
                Target = player.Position
            };
           GetNextTarget(player);
        }

        private void TeleportPlayerToCenterOfCurrentTile(Player player)
        {
            player.Position = new Vector2((int)player.Position.X + .5f, (int)player.Position.Y + .5f);
        }

        public void Update(GameTime gameTime, Player player)
        {
            if (!_nextTarget.HasValue)
            {
                GetNextTarget(player);
            }

            if (HasDestinationTileBeenReached(player))
            {
                var turn =  GetAmountToTurn(_lastTargetReached.Direction, _nextTarget.Value.Direction) ;
                _reversing = (turn == 180);
                _amountLeftToTurn += turn;

                _lastTargetReached = _nextTarget.Value;
                _nextTarget = null;
            }
            if (_amountLeftToTurn != 0)
            {
                var amountToTurn = 1;// (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 6);// Math.Abs(_amountLeftToTurn) > 90 ? 3 : 1;
                if (_amountLeftToTurn < 0) { player.ViewingAngle -= amountToTurn; _amountLeftToTurn += amountToTurn; }
                else { player.ViewingAngle += amountToTurn; _amountLeftToTurn -= amountToTurn; }
               if (Math.Abs(_amountLeftToTurn) < 15)
                    NudgePathTowardsCenterOfTiles(player);
            }
            else
            {
                if (_reversing) { _reversing = false; }
            }
            if (!_reversing) {player.MoveForward();}
        }

        private void NudgePathTowardsCenterOfTiles(Player player)
        {
            if(_lastTargetReached.Direction == DirectionData.Left || _lastTargetReached.Direction == DirectionData.Right)
            {
                var fractionOfY = player.Position.Y - (int)player.Position.Y;
                var changeNeeded = .5f - fractionOfY;
                if(changeNeeded < .01f) { player.Position += Vector2.UnitY * changeNeeded; }
                else
                {
                    player.Position += Vector2.UnitY * .04f;
                }
                
            }
            else
            {
                    var fractionOfX = player.Position.X - (int)player.Position.X;
                    var changeNeeded = .5f - fractionOfX;
                    if (changeNeeded < .01f) { player.Position += Vector2.UnitX * changeNeeded; }
                    else
                    {
                        player.Position += Vector2.UnitX * .1f;
                    }
            }
        }

        private bool HasDestinationTileBeenReached(Player player)
        {
            var currentTile = new Point((int)player.Position.X, (int)player.Position.Y);
            var destination = new Point((int)_nextTarget.Value.Target.X, (int)_nextTarget.Value.Target.Y);
            return currentTile == destination;
        }

        DirectionData GetRandomOpenNeighbor(Player player)
        {
            List<DirectionData> directions = new List<Input.DirectionData>();
            var currentTile = new Point((int)player.Position.X, (int)player.Position.Y);
            if (player.Map.Tiles[currentTile.X-1, currentTile.Y] == 0) { directions.Add(DirectionData.Left); }
            if (player.Map.Tiles[currentTile.X + 1, currentTile.Y] == 0) { directions.Add(DirectionData.Right); }
            if (player.Map.Tiles[currentTile.X, currentTile.Y -1] == 0) { directions.Add(DirectionData.Up); }
            if (player.Map.Tiles[currentTile.X , currentTile.Y +1] == 0) { directions.Add(DirectionData.Down); }
            return directions.GetRandomElement();
        }
           private void GetNextTarget(Player player)
        {
            var nextTargetToUse = MoveCandidateForwardUntilBlocked(_lastTargetReached, player.Map.Tiles);
            _nextTarget = AddBestTurnDirection(nextTargetToUse, player.Map.Tiles);
        }


        private PathFinderInfo AddBestTurnDirection(PathFinderInfo info, int[,] map)
        {
            var candidates = DirectionData.GetRightAndLeftInRandomOrder(_lastTargetReached.Direction).ToList();
            candidates.Add(DirectionData.GetOppositeDirection(_lastTargetReached.Direction));
            foreach (var directionCandidate in candidates)
            {
                var pointToTest = info.Target + directionCandidate.RelativePosition;
                var tileToTest = new Point((int)pointToTest.X, (int)pointToTest.Y);
                if (IsTileUnblocked(tileToTest, map))
                {
                    return new PathFinderInfo() { Direction = directionCandidate, Target = info.Target };
                }
            }
            throw new ArgumentException("Unable to find way out of tile: " + info.Target + " going direction " + info.Direction);
         
        }

        private PathFinderInfo MoveCandidateForwardUntilBlocked(PathFinderInfo startingPointInfo, int[,] map)
        {
            var direction = startingPointInfo.Direction;
            var lastUnblocked = new Point((int)startingPointInfo.Target.X, (int)startingPointInfo.Target.Y);
            while (IsTileUnblocked(new Point(lastUnblocked.X + (int)direction.RelativePosition.X, lastUnblocked.Y + (int)direction.RelativePosition.Y), map))
            {
                lastUnblocked = new Point(lastUnblocked.X + (int)direction.RelativePosition.X, lastUnblocked.Y + (int)direction.RelativePosition.Y);
            }
            return new PathFinderInfo() { Direction = direction, Target = new Vector2(lastUnblocked.X + .5f, lastUnblocked.Y + .5f) };

        }

        private bool IsTileUnblocked(Vector2 position, int[,] map)
        {
            return map[(int)position.X, (int)position.Y] == 0;
        }
        private bool IsTileUnblocked(Point position, int[,] map)
        {
            return map[position.X, position.Y] == 0;
        }

        float GetAmountToTurn(DirectionData currentDirection, DirectionData newDirection)
        {
            if (currentDirection == DirectionData.Up)
            {
                if (newDirection == DirectionData.Up) { return 0; }
                else if (newDirection == DirectionData.Left) { return -90; }
                else if (newDirection == DirectionData.Right) { return 90; }
                else if (newDirection == DirectionData.Down) { return 180; }
            }
            else if (currentDirection == DirectionData.Down)
            {
                if (newDirection == DirectionData.Up) { return 180; }
                else if (newDirection == DirectionData.Left) { return 90; }
                else if (newDirection == DirectionData.Right) { return -90; }
                else if (newDirection == DirectionData.Down) { return 0; }
            }
            else if (currentDirection == DirectionData.Right)
            {
                if (newDirection == DirectionData.Left) { return 180; }
                else if (newDirection == DirectionData.Down) { return 90; }
                else if (newDirection == DirectionData.Up) { return -90; }
                else if (newDirection == DirectionData.Right) { return 0; }
            }
            else //(currentDirection == DirectionData.Left)
            {
                if (newDirection == DirectionData.Left) { return 0; }
                else if (newDirection == DirectionData.Down) { return -90; }
                else if (newDirection == DirectionData.Up) { return 90; }
                else return 180; //right
            }
            //WUT? always returns alue

            //OH WELL for now:
            return 0;
        }
    }
}
