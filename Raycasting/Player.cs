using Microsoft.Xna.Framework;
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
        public float ViewingAngle { get; set; }
        public int[,] Map { get; set; }

        public Player(int[,] map)
        {
            Map = map;
        }

        internal void MoveBackwards()
        {
            var tempStep = ((float)MathHelper.ToRadians(this.ViewingAngle)).AngleAsVector() * .07f;
            var newPosition = this.Position - tempStep;
            PerformMoveIfNotBlocked(newPosition);
        }

        private void PerformMoveIfNotBlocked(Vector2 newPosition)
        {

            if (!Map.IsBlocked(newPosition))
            {
                this.Position = newPosition;
            }
            else
            {
                var positionHorizontal = new Vector2(newPosition.X, this.Position.Y);
                if (!Map.IsBlocked(positionHorizontal))
                {
                    this.Position = positionHorizontal;
                }
                else
                {
                    var positionVertical = new Vector2(this.Position.X, newPosition.Y);
                    if (!Map.IsBlocked(positionVertical))
                    {
                        this.Position = positionVertical;
                    }
                }
                Sounds.Instance.Bump.Play();
            }
        }

        internal void MoveForward()
        {
            var tempStep = ((float)MathHelper.ToRadians(this.ViewingAngle)).AngleAsVector() * .07f;
            var newPosition = this.Position + tempStep;
            PerformMoveIfNotBlocked(newPosition);
        }

        public override string ToString()
        {
            return "Pos: " + this.Position + ", viewangle: " + this.ViewingAngle;
        }

        internal void TurnRight()
        {
            this.ViewingAngle += 3f;
        }

        internal void TurnLeft()
        {
            this.ViewingAngle -= 3f;
        }
    }

}
