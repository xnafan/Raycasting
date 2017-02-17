using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting.MapMakers
{
    public interface IMap
    {
        Vector2 PlayerStartingPoint { get;  }
        float PlayersInitialViewingDirection { get; }
        int[,] Tiles { get; }
    }
}
