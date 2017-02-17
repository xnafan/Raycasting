using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting.MapMakers
{
    public interface IMapMaker
    {
         IMap CreateMaze(int cols, int rows, int maxTileIndex = 100);
    }
}
