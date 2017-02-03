using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Raycasting.Input
{
    public class KeyboardPlayerMover : IPlayerMover
    {

        public void Update(GameTime gameTime, Player player)
        {
            var currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.Up)) { player.MoveForward(); }
            if (currentKeyboardState.IsKeyDown(Keys.Down)) { player.MoveBackwards(); }
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {

                if (currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift))
                {
                    player.MoveLeft();
                }
                else
                {
                    player.TurnLeft();
                }
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                if (currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift))
                {
                    player.MoveRight();
                }
                else
                {
                    player.TurnRight();
                }
            }
        }
    }
}
