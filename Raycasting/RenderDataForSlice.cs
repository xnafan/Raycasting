using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public class RenderDataForSlice
    {
        public CollisionInfo? CollisionInfo { set; get; }
        public Rectangle DestinationRectangle { set; get; }
    }
}
