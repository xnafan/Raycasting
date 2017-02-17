using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xnafan.MazeCreator;

namespace Raycasting.MapMakers
{
    public class MazeMapMaker : IMapMaker
    {
        public IMap CreateMaze(int cols, int rows, int maxTileIndex = 100)
        {
            MazeCreator creator = new MazeCreator(cols, rows, new System.Drawing.Point(1, 1));

            int[,] tiles = null;
            do
            {
                tiles = new MazeCreator(cols, rows, new System.Drawing.Point(1, 1)).CreateMaze(); 
            } while (tiles[2, 1]!= 0);
            Vector2 playerPosition = new Vector2(1.5f, 1.5f);
            var map = new BaseMap() { PlayersInitialViewingDirection = 0, PlayerStartingPoint = playerPosition, Tiles = tiles };
            return map;
        }
    }
}
