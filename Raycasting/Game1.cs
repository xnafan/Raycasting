using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Raycasting.Input;
using System.Reflection;
using Raycasting.ImageGetters;

namespace Raycasting
{
    public class Game1 : Game
    {

        //commandline parameter NSFW http://www.jake.dk/Download/a3.zip
        #region Variables
        public static SpriteBatch SpriteBatch;
        public static ContentManager ContentManager;
        public static GraphicsDevice CurrentGraphicsDevice;
        Renderer _renderer;
        GraphicsDeviceManager _graphics;

        int[,] _maze = new int[31, 31];
        Player _player;
        Random _rnd = new Random();
        KeyboardState _currentKeyboardState, _oldKeyboardState;
        private bool _exiting;
        IPlayerMover _playerMover;
        #endregion

        #region Constructor and related
        public Game1()
        {

            SendToTool.AddSendToShortcutIfNotPresent("Raycasting Slideshow", "Show images in maze", Assembly.GetExecutingAssembly().Location);
            ContentManager = Content;
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.IsFullScreen = true;
        }

        protected override void LoadContent()
        {
            CurrentGraphicsDevice = GraphicsDevice;
            Game1.SpriteBatch = new SpriteBatch(GraphicsDevice);
            CreateMaze();
            _player = new Player(_maze);
            _player.Position = new Vector2(2.5f, 3.5f);
            _maze[(int)_player.Position.X, (int)_player.Position.Y] = 0;
            _maze[(int)_player.Position.X + 1, (int)_player.Position.Y] = 0;
            _maze[(int)_player.Position.X, (int)_player.Position.Y + 1] = 0;
            _maze[(int)_player.Position.X + 1, (int)_player.Position.Y + 1] = 0;

            //_maze[(int)_player.Position.X -1, (int)_player.Position.Y-1] = 1;
            //_maze[(int)_player.Position.X -1, (int)_player.Position.Y ] = 1;
            //_maze[(int)_player.Position.X -1, (int)_player.Position.Y + 1] = 1;
            //_maze[(int)_player.Position.X - 1, (int)_player.Position.Y +2] = 1;

            //_maze[(int)_player.Position.X +2, (int)_player.Position.Y - 1] = 1;
            //_maze[(int)_player.Position.X +2, (int)_player.Position.Y] = 1;
            //_maze[(int)_player.Position.X +2, (int)_player.Position.Y + 1] = 1;
            //_maze[(int)_player.Position.X + 2, (int)_player.Position.Y + 2] = 1;

            //_maze[(int)_player.Position.X , (int)_player.Position.Y - 1] = 1;
            //_maze[(int)_player.Position.X + 1, (int)_player.Position.Y -1] = 1;

            //_maze[(int)_player.Position.X, (int)_player.Position.Y + 2] = 1;
            //_maze[(int)_player.Position.X + 1, (int)_player.Position.Y + 2] = 1;


            _player.ViewingAngle = 0;
            _playerMover = new AutonomousPathfinderMover(_player);
            Sounds.Instance.Bump = Content.Load<SoundEffect>("Bump");
            _renderer = new Renderer(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, _player, _maze);
            GetTextures();
        }

        private void GetTextures()
        {
            string parameter = null;

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                parameter = Environment.GetCommandLineArgs()[1];
                parameter = parameter.Replace("\"", "");
            }

            if (parameter == null || Directory.Exists(parameter) || ((File.Exists(parameter) && Path.GetExtension(parameter) != ".zip")))
            {
                Console.WriteLine("Starting from folder or file");
                if (File.Exists(parameter) && Path.GetExtension(parameter) == ".url")
                {
                    new Thread(() =>
                    {
                        var imageGetter = new ImageGetterFromOnlineZipFiles(GetUrlFromLinkFile(parameter));
                        imageGetter.TextureLoadedEvent += (obj, e) => _renderer.AddTexture(e.Texture);
                        imageGetter.GetImages(GraphicsDevice, _renderer.Textures, ref _exiting);
                    }).Start();
                }
                else
                {
                    new Thread(() =>
                    {
                        ITextureGetter imageGetter = new ImageGetterFromFolderOrSingleNonZipFile(parameter);
                        imageGetter.TextureLoadedEvent += (obj, e) => _renderer.AddTexture(e.Texture);
                        imageGetter.GetImages(GraphicsDevice, _renderer.Textures, ref _exiting);

                        imageGetter = new ImageGetterFromZipFiles(parameter);
                        imageGetter.TextureLoadedEvent += (obj, e) => _renderer.AddTexture(e.Texture);
                        imageGetter.GetImages(GraphicsDevice, _renderer.Textures, ref _exiting);
                    }).Start();
                }
            }
            else if (parameter.Trim().Substring(0, 4).ToLower().Equals("http"))
            {
                new Thread(() =>
                {
                    var imageGetter = new ImageGetterFromOnlineZipFiles(parameter);
                    imageGetter.TextureLoadedEvent += (obj, e) => _renderer.AddTexture(e.Texture);
                    imageGetter.GetImages(GraphicsDevice, _renderer.Textures, ref _exiting);
                }).Start();
            }
            else if (Path.GetExtension(parameter) == ".zip")
            {
                Console.WriteLine("Starting from zip file");
                new Thread(() =>
                {
                    var imageGetter = new ImageGetterFromZipFiles(parameter);
                    imageGetter.TextureLoadedEvent += (obj, e) => _renderer.AddTexture(e.Texture);
                    imageGetter.GetImages(GraphicsDevice, _renderer.Textures, ref _exiting);
                }).Start();
            }
            else
            {
                Console.WriteLine("Error parsing '" + parameter + "' to ZIP file or directory");
                Exit();
            }
        }

        private static void StartFromFile(string file)
        { }

        private string GetUrlFromLinkFile(string parameter)
        {
            var lines = File.ReadAllLines(parameter);
            var urlLine = lines.ToList().First(line => line.Substring(0, 4) == "URL=");
            return urlLine.Substring(4);
        }

        private void CreateMaze()
        {
            int maxLength = 100;
            var tilesInAll = _maze.GetLength(0) * _maze.GetLength(1);
            float fillPercentage = .3f;
            for (int x = 0; x <= _maze.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= _maze.GetUpperBound(1); y++)
                {
                    if (y == 0 || x == 0 || y == _maze.GetUpperBound(1) || x == _maze.GetUpperBound(0) || _rnd.NextDouble() < fillPercentage)
                    {
                        _maze[x, y] = 1 + _rnd.Next(maxLength);
                    }
                }
            }

            int tilesToFill = tilesInAll / 3;
            for (int i = 0; i < tilesToFill; i++)
            {

            }
        }
        #endregion

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            GetInput();
            _playerMover.Update(gameTime, _player);
            _renderer.Update(gameTime);
        }

        private void GetInput()
        {
            _currentKeyboardState = Keyboard.GetState();
            if (_currentKeyboardState.IsKeyDown(Keys.Escape)) { _exiting = true; this.Exit(); }


            if (_currentKeyboardState.IsKeyDown(Keys.NumLock) && _oldKeyboardState.IsKeyUp(Keys.NumLock) && _renderer.Textures.Count > 0)
            {
                if (_currentKeyboardState.IsKeyDown(Keys.LeftShift) || _currentKeyboardState.IsKeyDown(Keys.RightShift))
                { _renderer.PreviousTextureSet(); }
                else { _renderer.NextTextureSet(); }
            }

            if (_currentKeyboardState.IsKeyDown(Keys.P) && _oldKeyboardState.IsKeyUp(Keys.P))
            {
                _renderer.PsychedelicMode = !_renderer.PsychedelicMode;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.F10) && _oldKeyboardState.IsKeyUp(Keys.F10))
            {
                _renderer.NextRenderSliceMethod();
            }
            if (_currentKeyboardState.IsKeyDown(Keys.F1) && _oldKeyboardState.IsKeyUp(Keys.F1))
            { _renderer.ShowHelp = !_renderer.ShowHelp; }
            if (_currentKeyboardState.IsKeyDown(Keys.F11) && _oldKeyboardState.IsKeyUp(Keys.F11))
            {
                _graphics.ToggleFullScreen();
            }
            if (_playerMover is AutonomousPathfinderMover)
            {
                if (_currentKeyboardState.IsKeyDown(Keys.Left) || _currentKeyboardState.IsKeyDown(Keys.Right) || _currentKeyboardState.IsKeyDown(Keys.Up) || _currentKeyboardState.IsKeyDown(Keys.Down))
                {
                    _playerMover = new KeyboardPlayerMover();
                }
            }

            _oldKeyboardState = _currentKeyboardState;
        }

        protected override void Draw(GameTime gameTime)
        {
            _renderer.Draw(gameTime);
        }
    }
}