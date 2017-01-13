using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Collections.Generic;
using System.Configuration;

namespace Raycasting
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        int[,] _maze = new int[15, 15];
        Texture2D[][] _textures;
        int _textureSetIndex = 0;
        Player _player;
        int _widthOfViewingField = 1200;
        int _halfWidthOfViewingField;
        int _heightOfViewingField = 800;
        int _viewingAngle = 60;
        int _raysPerDegreeOrResolutionIfYoudRatherCallItThat;
        int _pixelsPerDegreeOfViewingAngleFromSourceBitmap;
        int _pixelsPerDegreeOfViewingAngleFromViewArea;
        SpriteFont _font;
        Random _rnd = new Random();
        KeyboardState _oldKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferHeight = _heightOfViewingField;
            _graphics.PreferredBackBufferWidth = _widthOfViewingField;
            Content.RootDirectory = "Content";
            _halfWidthOfViewingField = _widthOfViewingField / 2;
            _raysPerDegreeOrResolutionIfYoudRatherCallItThat = _widthOfViewingField / _viewingAngle;
            //_graphics.IsFullScreen = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _textures = new ImageGetterFromZipFiles().GetTextures(GraphicsDevice);

            int maxLength = _textures.Max(item => item.Count());
            
            for (int x = 0; x <= _maze.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= _maze.GetUpperBound(1); y++)
                {
                    if (y == 0 || x == 0 || y == _maze.GetUpperBound(1) || x == _maze.GetUpperBound(0))
                    {
                        _maze[x, y] = 1+_rnd.Next(maxLength);
                    }
                }
            }
            for (int i = 0; i < 50; i++)
            {
                _maze[_rnd.Next(_maze.GetUpperBound(0)+1), _rnd.Next(_maze.GetUpperBound(1)+1)] = 1 + _rnd.Next(maxLength);
            }

            _player = new Player(_maze);
            _pixelsPerDegreeOfViewingAngleFromSourceBitmap = _textures[0][0].Width / _viewingAngle;
            _pixelsPerDegreeOfViewingAngleFromViewArea = _widthOfViewingField / (_viewingAngle*_raysPerDegreeOrResolutionIfYoudRatherCallItThat);
            _player.Position = new Vector2(4.5f, 4.5f);
            _maze[(int)_player.Position.X, (int)_player.Position.Y] = 0;
            _player.ViewingAngle = 0;
            _font = Content.Load<SpriteFont>("DefaultFont");
            Sounds.Instance.Bump = Content.Load<SoundEffect>("Bump");
        }
        
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var kbd = Keyboard.GetState();
            if (kbd.IsKeyDown(Keys.Escape)) { this.Exit(); }
            if (kbd.IsKeyDown(Keys.Up)) { _player.MoveForward(); }
            if (kbd.IsKeyDown(Keys.Down)) { _player.MoveBackwards(); }
            if (kbd.IsKeyDown(Keys.Left)) { _player.TurnLeft(); }
            if (kbd.IsKeyDown(Keys.Right)) { _player.TurnRight(); }
            if (kbd.IsKeyDown(Keys.NumLock) && _oldKeyboardState.IsKeyUp(Keys.NumLock)) { _textureSetIndex++; _textureSetIndex %= _textures.Length; }
            _oldKeyboardState = kbd;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            base.Draw(gameTime);
            //_spriteBatch.Draw(_wallImage, Vector2.One * 64,  Color.White);
            float degreePerPixel = _viewingAngle /_widthOfViewingField;
            for (float deltaAngle = 0; deltaAngle < _viewingAngle; deltaAngle += 1/(float)_raysPerDegreeOrResolutionIfYoudRatherCallItThat)
            {
                var realAngle = deltaAngle - _viewingAngle / 2;
                var fisheyeCompensation = Math.Cos(MathHelper.ToRadians(realAngle));
                var absoluteAngle = _player.ViewingAngle + realAngle;

                float sourceBitmapPositionToGrabFrom = 0;
                //var collisionPosition = _maze.GetCollisionPoint(_player.Position, MathHelper.ToRadians(absoluteAngle));
                var collisionPosition = _maze.GetCollisionPointImproved(_player,100);

                float distanceToCollision = 1000;
                if (collisionPosition.HasValue)
                {
                    distanceToCollision = Vector2.Distance(_player.Position, collisionPosition.Value);
                    sourceBitmapPositionToGrabFrom = collisionPosition.Value.GetPositionOnWall();
                }
                else continue;

                int textureIndex = _maze[(int)collisionPosition.Value.X, (int)collisionPosition.Value.Y] - 1;
                textureIndex %= _textures[_textureSetIndex].Length;
                var destinationHeight = _heightOfViewingField / distanceToCollision / fisheyeCompensation;

                //TODO: remove texturehack when math is okay
                if (textureIndex < 0) textureIndex = 0;

                var sourceRectangle = new Rectangle((int)(sourceBitmapPositionToGrabFrom * _textures[_textureSetIndex][textureIndex].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _textures[_textureSetIndex][textureIndex].Height);

                float percentageOfWidth = deltaAngle / _viewingAngle;
                var destinationRectangle = new Rectangle((int)( _widthOfViewingField * percentageOfWidth), (int)((_heightOfViewingField - destinationHeight) / 2), _pixelsPerDegreeOfViewingAngleFromViewArea , (int)destinationHeight);
                
                _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex], destinationRectangle, sourceRectangle, Color.White);
            }

            _spriteBatch.DrawString(_font, _player.ToString(), Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }

  
}

