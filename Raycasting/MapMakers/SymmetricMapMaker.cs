using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting.MapMakers
{
    class SymmetricMapMaker : IMapMaker
    {
        private static Random _rnd = new Random();
        public IMap CreateMaze(int cols, int rows, int maxTileIndex = 100)
        {
            var tiles = new int[cols, rows];
            int centerRow = (int)(rows / 2);
            int centerColumn = (int)(cols / 2);
            Vector2 playerPosition = new Vector2( 1.5f,  centerRow - .5f);
            var map = new BaseMap() { PlayersInitialViewingDirection = 0, PlayerStartingPoint = playerPosition, Tiles = tiles };
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (y == 0 || x == 0 || y == rows-1 || x == cols-1 || (x % 2==0 && y %2 == 0))
                    {
                        tiles[x, y] = (x + y) % maxTileIndex;
                    }
                }
            }
            
            tiles[centerColumn, 1] = (centerColumn) % maxTileIndex;
            tiles[centerColumn, cols-2] = (centerColumn) % maxTileIndex;
            tiles[1, centerRow] = (centerRow) % maxTileIndex;
            tiles[cols-2, centerRow] = (centerRow) % maxTileIndex;
            return map;
        }
    }
}
