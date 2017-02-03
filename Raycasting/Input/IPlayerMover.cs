using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting.Input
{
    interface IPlayerMover
    {
        void Update(GameTime gameTime, Player player);
    }
}
