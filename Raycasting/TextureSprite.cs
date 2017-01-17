using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public class TextureSprite
    {
        public Vector2 Position { get; set; }
        public Vector2 Movement { get; set; }
        public Texture2D Texture{ get; set; }
        public Rectangle DestinationRectangle {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, 200, 200);
            }
        }
        public void Update(GameTime gameTime)
        {
            Position += Movement * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        public void Draw(GameTime gameTime)
        {
            Game1.SpriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }

    }
}
