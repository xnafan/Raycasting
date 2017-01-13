using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public struct CollisionInfo
    {
        public Vector2 CollisionPoint { get; set; }
        public Point TileHit { get; set; }
        public float PositionOnWall { get; set; }
    }
}
