using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.IO;

namespace Raycasting
{
    public class Game1 : Game
    {

        public static SpriteBatch SpriteBatch;

        #region Variables
        GraphicsDeviceManager _graphics;
        bool _showHelp;
        SpriteBatch _spriteBatch;
        int[,] _maze = new int[31, 31];
        List<Action<CollisionInfo?, Rectangle>> _renderSliceMethods = new List<Action<CollisionInfo?, Rectangle>>();
        int _renderMethodIndex;
        List<Texture2D[]> _textures = new List<Texture2D[]>();
        int _textureSetIndex = 0;
        Player _player;
        float _msForNextSlide, _msBetweenSlides = 5000;
        int _slideIndex = 0;
        int _widthOfViewingFieldInPixels = 1200;
        int _halfWidthOfViewingFieldInPixels;
        int _heightOfViewingField = 800;
        int _widthOfViewingArcInDegrees = 60;
        int _raysPerDegreeOrResolutionIfYoudRatherCallItThat;
        SpriteFont _font;
        Random _rnd = new Random();
        KeyboardState _oldKeyboardState;
        RenderTarget2D _target;
        private bool _psychedelicMode;
        Vector2 _screen, _halfScreen;
        Texture2D _white;
        private bool _exiting;
        List<TextureSprite> _sprites = new List<TextureSprite>();
        #endregion

        #region Constructor and related
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferHeight = _heightOfViewingField;
            _graphics.PreferredBackBufferWidth = _widthOfViewingFieldInPixels;
            Content.RootDirectory = "Content";
            _halfWidthOfViewingFieldInPixels = _widthOfViewingFieldInPixels / 2;
            _raysPerDegreeOrResolutionIfYoudRatherCallItThat = _widthOfViewingFieldInPixels / _widthOfViewingArcInDegrees;

            _renderSliceMethods.Add(RenderSliceWithDistanceBasedLighting);
            _renderSliceMethods.Add(RenderSliceForSlideShow);
            _renderSliceMethods.Add(RenderSlice);
            
            _screen = new Vector2(_widthOfViewingFieldInPixels , _heightOfViewingField );
            _halfScreen = _screen / 2;
            _graphics.IsFullScreen = false;
            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Game1.SpriteBatch = _spriteBatch;
            CreateMaze();
            _player = new Player(_maze);
            _player.Position = new Vector2(2.5f, 3.5f);
            _maze[(int)_player.Position.X, (int)_player.Position.Y] = 0;
            _maze[(int)_player.Position.X+1, (int)_player.Position.Y] = 0;
            _player.ViewingAngle = 0;
            _font = Content.Load<SpriteFont>("DefaultFont");
            Sounds.Instance.Bump = Content.Load<SoundEffect>("Bump");
            _target = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
              GraphicsDevice.PresentationParameters.BackBufferHeight);
            _white = Content.Load<Texture2D>("white");
            GetTextures();
        }

        private void GetTextures()
        {
            Console.WriteLine(Environment.GetCommandLineArgs());

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                string commandLineParameter = Environment.GetCommandLineArgs()[1];
                commandLineParameter = commandLineParameter.Replace("\"", "");
                if (Path.GetExtension(commandLineParameter) == ".zip")
                {
                    Console.WriteLine("Starting from zip file");
                    new Thread(() =>
                    {
                        new ImageGetterFromZipFiles(commandLineParameter).GetImages(GraphicsDevice, _textures, ref _exiting);
                    }).Start();
                }
                else if (Directory.Exists(commandLineParameter))
                {
                    Console.WriteLine("Starting from folder");
                    new Thread(() =>
                    {
                        IImageGetter imageGetter =
                        new ImageGetterFromFolder(commandLineParameter);
                        imageGetter.TextureLoadedEvent += ImageGetter_TextureLoadedEvent;
                        imageGetter.GetImages(GraphicsDevice, _textures, ref _exiting);
                        imageGetter =
                        new ImageGetterFromZipFiles(commandLineParameter);
                        imageGetter.TextureLoadedEvent += ImageGetter_TextureLoadedEvent;
                        imageGetter.GetImages(GraphicsDevice, _textures, ref _exiting);
                    }).Start();
                }
                else
                {
                    Console.WriteLine("Error parsing '" + commandLineParameter + "' to ZIP file or directory");
                    Exit();
                }
            }
            else
            {
                new Thread(() =>
                  {
                      IImageGetter imageGetter = new ImageGetterFromFolder();
                      imageGetter.TextureLoadedEvent += ImageGetter_TextureLoadedEvent;
                      imageGetter.GetImages(GraphicsDevice, _textures, ref _exiting);
                      imageGetter = new ImageGetterFromZipFiles();
                      imageGetter.TextureLoadedEvent += ImageGetter_TextureLoadedEvent;
                      imageGetter.GetImages(GraphicsDevice, _textures, ref _exiting);
                  }).Start();
            }
        }

        private void ImageGetter_TextureLoadedEvent(object sender, TextureEventArgs e)
        {
            lock (_sprites)
            {
                _sprites.Add(new TextureSprite() { Texture = e.Texture, Movement = Vector2.UnitX / 3, Position = new Vector2(-20, _screen.Y - 200) });
            }
        }

        private void CreateMaze()
        {
            int maxLength = 100;

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
            int tilesToFill = tilesInAll / 5;
            for (int i = 0; i < tilesToFill; i++)
            {
                _maze[_rnd.Next(_maze.GetUpperBound(0) + 1), _rnd.Next(_maze.GetUpperBound(1) + 1)] = 1 + _rnd.Next(maxLength);
            }
        }
        #endregion

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            GetInput();
            _msForNextSlide -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_msForNextSlide <= 0)
            {
                _slideIndex++;
                _msForNextSlide = _msBetweenSlides;
            }
            lock(_sprites)
            {
                _sprites.ForEach(sprite => sprite.Update(gameTime));
            }
            
        }

        private void GetInput()
        {
            var kbd = Keyboard.GetState();
            if (kbd.IsKeyDown(Keys.Escape)) { _exiting = true; this.Exit(); }
            if (kbd.IsKeyDown(Keys.Up)) { _player.MoveForward(); }
            if (kbd.IsKeyDown(Keys.Down)) { _player.MoveBackwards(); }
            if (kbd.IsKeyDown(Keys.Left)) { _player.TurnLeft(); }
            if (kbd.IsKeyDown(Keys.Right)) { _player.TurnRight(); }

            if (kbd.IsKeyDown(Keys.NumLock) && _oldKeyboardState.IsKeyUp(Keys.NumLock) && _textures.Count > 0)
            {

                int modifier = (kbd.IsKeyDown(Keys.LeftShift) || kbd.IsKeyDown(Keys.RightShift)) ? -1 : 1;
                _textureSetIndex += modifier;
                _textureSetIndex += _textures.Count;
                _textureSetIndex %= _textures.Count;
            }

            if (kbd.IsKeyDown(Keys.P) && _oldKeyboardState.IsKeyUp(Keys.P))
            {
                _psychedelicMode = !_psychedelicMode;
            }
            if (kbd.IsKeyDown(Keys.F10) && _oldKeyboardState.IsKeyUp(Keys.F10))
            {
                NextRenderSliceMethod();
            }
            if (kbd.IsKeyDown(Keys.F1) && _oldKeyboardState.IsKeyUp(Keys.F1))
            { _showHelp = !_showHelp; }
            if (kbd.IsKeyDown(Keys.F11) && _oldKeyboardState.IsKeyUp(Keys.F11))
            {
                _graphics.ToggleFullScreen();
            }
            _oldKeyboardState = kbd;
        }

        private void NextRenderSliceMethod()
        {
            if (_textures.Count > 0)
            {
                _renderMethodIndex++;
                _renderMethodIndex %= _renderSliceMethods.Count;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            var renderSliceMethod = GetCurrentSliceRenderMethod();
            if (_psychedelicMode){ RenderAllToTarget(gameTime, renderSliceMethod); }
            else {RenderAll(gameTime, renderSliceMethod);}
            RenderSprites(gameTime);
            _spriteBatch.Begin();
            DrawHelp();
            _spriteBatch.End();
        }

        private void RenderSprites(GameTime gameTime)
        {
            _spriteBatch.Begin();
            lock (_sprites)
            {
                _sprites.ForEach(sprite => sprite.Draw(gameTime));
            }
            _spriteBatch.End();
        }

        private Action<CollisionInfo?, Rectangle> GetCurrentSliceRenderMethod()
        {
            if (_textures.Count > 0)
            { return _renderSliceMethods[_renderMethodIndex]; }
            else
            {
                return RenderWhiteSlice;
            }
        }

        private void RenderAll(GameTime gameTime, Action<CollisionInfo?, Rectangle> renderMethod)
        {
            _spriteBatch.Begin();
            base.Draw(gameTime);
            float degreePerPixel = _widthOfViewingArcInDegrees / (float)_widthOfViewingFieldInPixels;
            float halfWidthOfViewingArcInDegrees = _widthOfViewingArcInDegrees / 2;
            float absoluteAngleOfLeftMostPeripheralVision = _player.ViewingAngle - halfWidthOfViewingArcInDegrees;
            for (int pixel = 0; pixel < _widthOfViewingFieldInPixels; pixel++)
            {
                var realAngle = absoluteAngleOfLeftMostPeripheralVision + degreePerPixel * pixel;
                var angleFromCenter = Math.Abs(_player.ViewingAngle - realAngle);
                var fisheyeCompensation = (float)Math.Cos(MathHelper.ToRadians(angleFromCenter));

                CollisionInfo? collisionPosition = _maze.GetCollisionPointImproved(_player.Position, realAngle, 100);

                float distanceToCollision = 1000;

                if (collisionPosition.HasValue)
                {
                    distanceToCollision = Vector2.Distance(_player.Position, collisionPosition.Value.CollisionPoint);
                }
                else continue;
                distanceToCollision = distanceToCollision * fisheyeCompensation;
                var destinationHeight = _heightOfViewingField / distanceToCollision;

                float percentageOfWidth = pixel / _widthOfViewingArcInDegrees;
                var destinationRectangle = new Rectangle(pixel, (int)((_heightOfViewingField - destinationHeight) / 2),
                    1, (int)destinationHeight);

                renderMethod(collisionPosition, destinationRectangle);
            }
            
            _spriteBatch.End();
        }

        private void RenderAllToTargetTwice(GameTime gameTime, Action<CollisionInfo?, Rectangle> renderSliceMethod)
        {
            GraphicsDevice.SetRenderTarget(_target);
            RenderAllToTarget(gameTime, renderSliceMethod);
            GraphicsDevice.SetRenderTarget(null);
            RenderAllToTarget(gameTime, RenderSliceFromTarget);
            RenderAll(gameTime, RenderSliceFromTarget);
        }

        private void RenderAllToTarget(GameTime gameTime, Action<CollisionInfo?, Rectangle> renderSliceMethod)
        {
            GraphicsDevice.SetRenderTarget(_target);
            RenderAll(gameTime, renderSliceMethod);
            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin();

            var scale = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            scale = 1.4f + Math.Abs(scale);
            _spriteBatch.Draw(_target, _halfScreen, null, Color.White, 0, _halfScreen, scale, SpriteEffects.None, 0);
            _spriteBatch.End();

            RenderAll(gameTime, RenderSliceFromTarget);
        }

        private void RenderWhiteSlice(CollisionInfo? collisionPosition, Rectangle destinationRectangle)
        {
            var sourceRectangle1 = new Rectangle(0,0,1,1);
            float maxDistanceSquared = 7;
            float distanceSquared = Vector2.Distance(collisionPosition.Value.CollisionPoint, _player.Position);
            float opacity = (1 - (float)(distanceSquared / maxDistanceSquared));
            _spriteBatch.Draw(_white, destinationRectangle, sourceRectangle1, Color.White * opacity);
        }

        private void RenderSliceFromTarget(CollisionInfo? collisionPosition, Rectangle destinationRectangle)
        {
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = _target.Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _widthOfViewingFieldInPixels), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _heightOfViewingField);
            _spriteBatch.Draw(_target, destinationRectangle, sourceRectangle1, Color.White);
        }

        private void RenderSlice(CollisionInfo? collisionPosition, Rectangle destinationRectangle)
        {
            int textureIndex1 = _maze[collisionPosition.Value.TileHit.X, collisionPosition.Value.TileHit.Y] - 1;
            textureIndex1 %= _textures[_textureSetIndex].Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = _textures[_textureSetIndex][textureIndex1].Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _textures[_textureSetIndex][textureIndex1].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _textures[_textureSetIndex][textureIndex1].Height);
            _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex1], destinationRectangle, sourceRectangle1, Color.White);
        }

        private void RenderSliceWithDistanceBasedLighting(CollisionInfo? collisionPosition, Rectangle destinationRectangle)
        {
            int textureIndex1 = _maze[collisionPosition.Value.TileHit.X, collisionPosition.Value.TileHit.Y] - 1;
            textureIndex1 %= _textures[_textureSetIndex].Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = _textures[_textureSetIndex][textureIndex1].Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _textures[_textureSetIndex][textureIndex1].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _textures[_textureSetIndex][textureIndex1].Height);
            float maxDistanceSquared = 7;
            float distanceSquared = Vector2.Distance(collisionPosition.Value.CollisionPoint, _player.Position);
            float opacity = (1 - (float)(distanceSquared / maxDistanceSquared));
            _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex1], destinationRectangle, sourceRectangle1, Color.White * opacity);
        }

        float BezierBlend(float t) {return (float)Math.Pow(t, 2) * (3.0f - 2.0f * t);}

        private void RenderSliceForSlideShow(CollisionInfo? collisionPosition, Rectangle destinationRectangle)
        {
            int textureIndex1 = _maze[collisionPosition.Value.TileHit.X, collisionPosition.Value.TileHit.Y] - 1 + _slideIndex;
            textureIndex1 %= _textures[_textureSetIndex].Length;

            int textureIndex2 = (textureIndex1 + 1) % _textures[_textureSetIndex].Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap1 = _textures[_textureSetIndex][textureIndex1].Width / _widthOfViewingArcInDegrees;

            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap2 = _textures[_textureSetIndex][textureIndex2].Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _textures[_textureSetIndex][textureIndex1].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap1, _textures[_textureSetIndex][textureIndex1].Height);
            var sourceRectangle2 = new Rectangle((int)(collisionPosition.Value.PositionOnWall * _textures[_textureSetIndex][textureIndex2].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap2, _textures[_textureSetIndex][textureIndex2].Height);

            float transparency = _msForNextSlide / _msBetweenSlides;
            transparency = BezierBlend(transparency);
            float maxDistanceSquared = 7;
            float distanceSquared = Vector2.Distance(collisionPosition.Value.CollisionPoint, _player.Position);
            float opacity = (1 - (float)(distanceSquared / maxDistanceSquared));

            _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex1], destinationRectangle, sourceRectangle1, Color.White);
            _spriteBatch.Draw(_textures[_textureSetIndex][textureIndex2], destinationRectangle, sourceRectangle2, Color.White * (1 - transparency));
        }

        private void DrawHelp()
        {

            Vector2 top = Vector2.UnitY * (_heightOfViewingField - 80);
            Vector2 interval = Vector2.UnitY * 20;
            var textureText = "loading first imageset";
            if(_textures.Count >0)
            {
                textureText = "imageset " + (_textureSetIndex + 1) + " of " + _textures.Count;
            }
            
            var textureTextPosition = _screen - _font.MeasureString(textureText) - Vector2.UnitX*10;
            _spriteBatch.DrawString(_font, textureText, textureTextPosition, Color.White);

            if (!_showHelp)
            {
                top.Y = _heightOfViewingField - 20;
                _spriteBatch.DrawString(_font, "[F1] for Help", top, Color.White);
                return;
            }
            _spriteBatch.DrawString(_font, _player.ToString(), Vector2.Zero, Color.White);
            _spriteBatch.DrawString(_font, "[F10] to cycle presentationmodes", top, Color.White);
            top += interval;
            _spriteBatch.DrawString(_font, "[F11] to toggle fullscreen", top, Color.White);
            top += interval;
            _spriteBatch.DrawString(_font, "[NUMLOCK] to switch textures", top, Color.White);
            top += interval;
            _spriteBatch.DrawString(_font, "[P] to toggle psychedelic mode", top, Color.White);
        }
    }
}