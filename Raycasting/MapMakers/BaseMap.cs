using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Raycasting.MapMakers
{
    public class BaseMap : IMap
    {
        public float PlayersInitialViewingDirection { get; set; }
        public Vector2 PlayerStartingPoint { get; set; }
        public int[,] Tiles { get; set; }
    }
}
