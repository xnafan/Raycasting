using Microsoft.Xna.Framework;
using Raycasting.MapMakers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public class Player
    {
        public Vector2 Position { get; set; }
        float _viewingAngle;
        public float Speed { get; set; } = 0.01f;
        public float ViewingAngle
        {
            get { return _viewingAngle; }
            set
            {
                while (value < 0)
                {
                    value += 360;
                }
                _viewingAngle = value % 360;
            }
        }

        public IMap Map { get; set; }

        public Player(IMap map)
        {
            Map = map;
            Position = map.PlayerStartingPoint;
            ViewingAngle = map.PlayersInitialViewingDirection;
        }
        private void PerformMoveIfNotBlocked(Vector2 newPosition)
        {

            if (!Map.Tiles.IsBlocked(newPosition))
            {
                this.Position = newPosition;
            }
            else
            {
                var positionHorizontal = new Vector2(newPosition.X, this.Position.Y);
                if (!Map.Tiles.IsBlocked(positionHorizontal))
                {
                    this.Position = positionHorizontal;
                }
                else
                {
                    var positionVertical = new Vector2(this.Position.X, newPosition.Y);
                    if (!Map.Tiles.IsBlocked(positionVertical))
                    {
                        this.Position = positionVertical;
                    }
                }
                Sounds.Instance.Bump.Play();
            }
        }

        internal void MoveForward(float amount = 0.01f)
        {
            var tempStep = ((float)MathHelper.ToRadians(this.ViewingAngle)).AngleAsVector() * amount;
            var newPosition = this.Position + tempStep;
            PerformMoveIfNotBlocked(newPosition);
        }

        internal void MoveBackwards()
        {
            var tempStep = ((float)MathHelper.ToRadians(this.ViewingAngle)).AngleAsVector() * Speed;
            var newPosition = this.Position - tempStep;
            PerformMoveIfNotBlocked(newPosition);
        }

        internal void MoveLeft()
        {
            var tempStep = ((float)MathHelper.ToRadians(this.ViewingAngle - 90)).AngleAsVector() * Speed;
            var newPosition = this.Position + tempStep;
            PerformMoveIfNotBlocked(newPosition);
        }

        internal void MoveRight()
        {
            var tempStep = ((float)MathHelper.ToRadians(this.ViewingAngle + 90)).AngleAsVector() * Speed;
            var newPosition = this.Position + tempStep;
            PerformMoveIfNotBlocked(newPosition);
        }



        public override string ToString()
        {
            return "[Player] Position: " + this.Position + ", viewangle: " + this.ViewingAngle;
        }

        internal void TurnRight(float amount = 3)
        {
            this.ViewingAngle += amount;
        }

        internal void TurnLeft(float amount = 3)
        {
            this.ViewingAngle -= amount;
        }
    }

}
