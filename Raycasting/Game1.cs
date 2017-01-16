using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace Raycasting
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        int[,] _maze = new int[31, 31];
        List<Action<CollisionInfo?, Rectangle>> _renderMethods = new List<Action<CollisionInfo?, Rectangle>>() ;
        int _renderMethodIndex;
        Texture2D[][] _textures;
        int _textureSetIndex = 0;
        Player _player;
        float _msForNextSlide, _msBetweenSlides = 3000;
        int _slideIndex = 0;
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
        bool _useOldCollisionFinder = false;
        RenderTarget2D _target;
        private bool _psychedelicMode;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferHeight = _heightOfViewingField;
            _graphics.PreferredBackBufferWidth = _widthOfViewingField;
            Content.RootDirectory = "Content";
            _halfWidthOfViewingField = _widthOfViewingField / 2;
            _raysPerDegreeOrResolutionIfYoudRatherCallItThat = _widthOfViewingField / _viewingAngle;
            _renderMethods.Add(Render);
            _renderMethods.Add(RenderSlideShow);

            _graphics.IsFullScreen = false;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _textures = new ImageGetterFromZipFiles().GetTextures(GraphicsDevice);

            CreateMaze();

            _player = new Player(_maze);
            _pixelsPerDegreeOfViewingAngleFromSourceBitmap = _textures[0][0].Width / _viewingAngle;
            _pixelsPerDegreeOfViewingAngleFromViewArea = _widthOfViewingField / (_viewingAngle*_raysPerDegreeOrResolutionIfYoudRatherCallItThat);
            _player.Position = new Vector2(2.5f, 3.5f);
            _maze[(int)_player.Position.X, (int)_player.Position.Y] = 0;
            _player.ViewingAngle = 0;
            _font = Content.Load<SpriteFont>("DefaultFont");
            Sounds.Instance.Bump = Content.Load<SoundEffect>("Bump");
            _target = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
              GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        private void CreateMaze()
        {
            int maxLength = _textures.Max(item => item.Count());

            for (int x = 0; x <= _maze.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= _maze.GetUpperBound(1); y++)
                {
                    if (y == 0 || x == 0 || y == _maze.GetUpperBound(1) || x == _maze.GetUpperBound(0))
                    {
                        _maze[x, y] = 1 + _rnd.Next(maxLength);
                    }
                }
            }
            var tilesInAll = _maze.GetLength(0) * _maze.GetLength(1);
            int tilesToFill = tilesInAll / 4;
            for (int i = 0; i < tilesToFill; i++)
            {
                _maze[_rnd.Next(_maze.GetUpperBound(0) + 1), _rnd.Next(_maze.GetUpperBound(1) + 1)] = 1 + _rnd.Next(maxLength);
            }
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
            if (kbd.IsKeyDown(Keys.F11) && _oldKeyboardState.IsKeyUp(Keys.F11)) {
                _graphics.ToggleFullScreen();
            }
            if (kbd.IsKeyDown(Keys.P) && _oldKeyboardState.IsKeyUp(Keys.P))
            {
                _psychedelicMode = !_psychedelicMode;
            }
            if (kbd.IsKeyDown(Keys.F10) && _oldKeyboardState.IsKeyUp(Keys.F10))
            {
                _renderMethodIndex++;
                _renderMethodIndex %= _renderMethods.Count;
            }
            if (kbd.IsKeyDown(Keys.F1) && _oldKeyboardState.IsKeyUp(Keys.F1))
            { _useOldCollisionFinder = !_useOldCollisionFinder; }
                _oldKeyboardState = kbd;
            _msForNextSlide -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_msForNextSlide <= 0)
            {
                _slideIndex++;
                _msForNextSlide = _msBetweenSlides;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_useOldCollisionFinder){ DrawUnOptimized(gameTime);  }
            else{DrawOptimized(gameTime); }
        }

        protected void DrawOptimized(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Console.WriteLine(_player);
            if (_psychedelicMode)
            { RenderAllToTarget(gameTime, _renderMethods[_renderMethodIndex]); }
            else
            {RenderAll(gameTime, _renderMethods[_renderMethodIndex]);}
        }

        private void RenderAll(GameTime gameTime, Action<CollisionInfo?, Rectangle> renderMethod)
        {
            _spriteBatch.Begin();
            base.Draw(gameTime);
            float degreePerPixel = _viewingAngle / _widthOfViewingField;
            for (float deltaAngle = 0; deltaAngle < _viewingAngle; deltaAngle += 1 / (float)_raysPerDegreeOrResolutionIfYoudRatherCallItThat
                )
            {

                var realAngle = deltaAngle - _viewingAngle / 2;

                var fisheyeCompensation = Math.Cos(MathHelper.ToRadians(realAngle));
                var absoluteAngle = _player.ViewingAngle + realAngle;

                CollisionInfo? collisionPosition = _maze.GetCollisionPointImproved(_player.Position, absoluteAngle, 100);

                float distanceToCollision = 1000;

                if (collisionPosition.HasValue)
                {
                    distanceToCollision = Vector2.Distance(_player.Position, collisionPosition.Value.CollisionPoint);
                }
                else continue;

                var destinationHeight = _heightOfViewingField / distanceToCollision / fisheyeCompensation;

                float percentageOfWidth = deltaAngle / _viewingAngle;
                var destinationRectangle = new Rectangle((int)(_widthOfViewingField * percentageOfWidth), (int)((_heightOfViewingField - destinationHeight) / 2),
                    _pixelsPerDegreeOfViewingAngleFromViewArea
                    , (int)destinationHeight);

                renderMethod(collisionPosition, destinationRectangle);
            }

            DrawHelp();
            _spriteBatch.End();
        }

        private void RenderAllToTarget(GameTime gameTime, Action<CollisionInfo?, Rectangle> renderMethod)
        {
            GraphicsDevice.SetRenderTarget(_target);
            RenderAll(gameTime, _renderMethods[_renderMethodIndex]);
            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin();
            var halfScreen = new Vector2(_widthOfViewingField / 2, _heightOfViewingField / 2);
            var scale = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            scale = 1.4f +Math.Abs(scale);
            _spriteBatch.Draw(_target, halfScreen, null, Color.White, 0,halfScreen ,scale, SpriteEffects.None,0 );
            _spriteBatch.End();
            
            RenderAll(gameTime, RenderFromTarget);
        }

        private void RenderAllToTargetTwice(GameTime gameTime, Action<CollisionInfo?, Rectangle> renderMethod)
        {
            GraphicsDevice.SetRenderTarget(_target);
            RenderAllToTarget(gameTime, _renderMethods[_renderMethodIndex]);
            GraphicsDevice.SetRenderTarget(null);
            RenderAllToTarget(gameTime, RenderFromTarget);
            RenderAll(gameTime, RenderFromTarget);
        }


        private void RenderFromTarget(CollisionInfo? collisionPosition, Rectangle destinationRectangle)
        {
            var sourceRectangle1 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _widthOfViewingField), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _heightOfViewingField);
            _spriteBatch.Draw(_target, destinationRectangle, sourceRectangle1, Color.White);
        }

        private void Render(CollisionInfo? collisionPosition, Rectangle destinationRectangle)
        {
            int textureIndex1 = _maze[collisionPosition.Value.TileHit.X, collisionPosition.Value.TileHit.Y] - 1 ;
            textureIndex1 %= _textures[_textureSetIndex].Length;

            var sourceRectangle1 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _textures[_textureSetIndex][textureIndex1].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _textures[_textureSetIndex][textureIndex1].Height);
            float transparency = _msForNextSlide / _msBetweenSlides;
            _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex1], destinationRectangle, sourceRectangle1, Color.White);
        }

     

        private void RenderSlideShow(CollisionInfo? collisionPosition, Rectangle destinationRectangle)
        {
            int textureIndex1 = _maze[collisionPosition.Value.TileHit.X, collisionPosition.Value.TileHit.Y] - 1 + _slideIndex;
            textureIndex1 %= _textures[_textureSetIndex].Length;

            int textureIndex2 = (textureIndex1 + 1) % _textures[_textureSetIndex].Length;

            var sourceRectangle1 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _textures[_textureSetIndex][textureIndex1].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _textures[_textureSetIndex][textureIndex1].Height);
            var sourceRectangle2 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _textures[_textureSetIndex][textureIndex2].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _textures[_textureSetIndex][textureIndex2].Height);

            float transparency = _msForNextSlide / _msBetweenSlides;
            _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex1], destinationRectangle, sourceRectangle1, Color.White);
            _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex2], destinationRectangle, sourceRectangle2, Color.White * (1 - transparency));
        }


        protected void DrawUnOptimized(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            base.Draw(gameTime);
            //_spriteBatch.Draw(_wallImage, Vector2.One * 64,  Color.White);
            float degreePerPixel = _viewingAngle / _widthOfViewingField;
            for (float deltaAngle = 0; deltaAngle < _viewingAngle; deltaAngle += 1 / (float)_raysPerDegreeOrResolutionIfYoudRatherCallItThat)
            {
                var realAngle = deltaAngle - _viewingAngle / 2;
                var fisheyeCompensation = Math.Cos(MathHelper.ToRadians(realAngle));
                var absoluteAngle = _player.ViewingAngle + realAngle;

                float sourceBitmapPositionToGrabFrom = 0;
                var collisionPosition = _maze.GetCollisionPoint(_player.Position, MathHelper.ToRadians(absoluteAngle));

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
                var destinationRectangle = new Rectangle((int)(_widthOfViewingField * percentageOfWidth), (int)((_heightOfViewingField - destinationHeight) / 2), _pixelsPerDegreeOfViewingAngleFromViewArea, (int)destinationHeight);

                _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex], destinationRectangle, sourceRectangle, Color.White);
            }

            DrawHelp();
            
            _spriteBatch.End();
        }
        private void DrawHelp()
        {
            _spriteBatch.DrawString(_font, _player.ToString(), Vector2.Zero, Color.White);
            Vector2 top = Vector2.UnitY * (_heightOfViewingField - 80);
            Vector2 interval = Vector2.UnitY * 20;
            _spriteBatch.DrawString(_font, "[F10] to cycle presentationmodes", top, Color.White);
            top += interval;
            Color optimizationColor = _useOldCollisionFinder ? Color.Red : Color.Green;
            _spriteBatch.DrawString(_font, "[F1] to toggle drawoptimization. Currently: * " + (_useOldCollisionFinder ? "UN" : "") + "OPTIMIZED *", top, optimizationColor);
            top += interval;
            _spriteBatch.DrawString(_font, "[F11] to toggle fullscreen", top, Color.White);
            top += interval;
            _spriteBatch.DrawString(_font, "[NUMLOCK] to switch textures", top, Color.White);
            top += interval;
            _spriteBatch.DrawString(_font, "[P] to toggle psychedelic mode", top, Color.White);
        }
    }

  
}


