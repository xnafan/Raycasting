using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting.MapMakers
{
    public class RandomMapMaker : IMapMaker
    {
        private static Random _rnd = new Random();
        public IMap CreateMaze(int cols, int rows, int maxTileIndex = 100)
        {
            var tiles = new int[cols, rows];
            Vector2 playerPosition = new Vector2(2.5f, 3.5f);
            tiles[(int)playerPosition.X, (int)playerPosition.Y] = 0;
            tiles[(int)playerPosition.X + 1, (int)playerPosition.Y] = 0;
            tiles[(int)playerPosition.X, (int)playerPosition.Y + 1] = 0;
            tiles[(int)playerPosition.X + 1, (int)playerPosition.Y + 1] = 0;
            var map = new BaseMap() { PlayersInitialViewingDirection = 0, PlayerStartingPoint = playerPosition, Tiles = tiles };
            var tilesInAll = cols * rows;
            float fillPercentage = .3f;
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (y == 0 || x == 0 || y == rows-1 || x == cols-1 || _rnd.NextDouble() < fillPercentage)
                    {
                        tiles[x, y] = 1 + _rnd.Next(maxTileIndex);
                    }
                }
            }
            return map;
        }
    }
}
