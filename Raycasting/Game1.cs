using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Raycasting
{
    public class Game1 : Game
    {
        #region Variables
        public static SpriteBatch SpriteBatch;
        public static ContentManager ContentManager;
        public static GraphicsDevice CurrentGraphicsDevice;
        Renderer _renderer;
        GraphicsDeviceManager _graphics;

        int[,] _maze = new int[31, 31];
        Player _player;
        Random _rnd = new Random();
        KeyboardState _oldKeyboardState;
        private bool _exiting;
        int _width = 1200, _height = 800;
        #endregion

        #region Constructor and related
        public Game1()
        {
            ContentManager = Content;
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferHeight = _height;
            _graphics.PreferredBackBufferWidth = _width;
            _graphics.IsFullScreen = false;

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
            _player.ViewingAngle = 0;

            Sounds.Instance.Bump = Content.Load<SoundEffect>("Bump");
            _renderer = new Renderer(_width, _height, _player, _maze);
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

            if (parameter == null || Directory.Exists(parameter))
            {
                Console.WriteLine("Starting from folder");
                new Thread(() =>
                {
                    IImageGetter imageGetter =
                    new ImageGetterFromFolder(parameter);
                    imageGetter.TextureLoadedEvent += (obj, e) => _renderer.AddTexture(e.Texture);
                    imageGetter.GetImages(GraphicsDevice, _renderer.Textures, ref _exiting);

                    imageGetter = new ImageGetterFromZipFiles(parameter);
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
        _renderer.Update(gameTime);
    }

    private void GetInput()
    {
        var kbd = Keyboard.GetState();
        if (kbd.IsKeyDown(Keys.Escape)) { _exiting = true; this.Exit(); }
        if (kbd.IsKeyDown(Keys.Up)) { _player.MoveForward(); }
        if (kbd.IsKeyDown(Keys.Down)) { _player.MoveBackwards(); }
        if (kbd.IsKeyDown(Keys.Left)) { _player.TurnLeft(); }
        if (kbd.IsKeyDown(Keys.Right)) { _player.TurnRight(); }

        if (kbd.IsKeyDown(Keys.NumLock) && _oldKeyboardState.IsKeyUp(Keys.NumLock) && _renderer.Textures.Count > 0)
        {
            if (kbd.IsKeyDown(Keys.LeftShift) || kbd.IsKeyDown(Keys.RightShift))
            { _renderer.PreviousTextureSet(); }
            else { _renderer.NextTextureSet(); }
        }

        if (kbd.IsKeyDown(Keys.P) && _oldKeyboardState.IsKeyUp(Keys.P))
        {
            _renderer.PsychedelicMode = !_renderer.PsychedelicMode;
        }
        if (kbd.IsKeyDown(Keys.F10) && _oldKeyboardState.IsKeyUp(Keys.F10))
        {
            NextRenderSliceMethod();
        }
        if (kbd.IsKeyDown(Keys.F1) && _oldKeyboardState.IsKeyUp(Keys.F1))
        { _renderer.ShowHelp = !_renderer.ShowHelp; }
        if (kbd.IsKeyDown(Keys.F11) && _oldKeyboardState.IsKeyUp(Keys.F11))
        {
            _graphics.ToggleFullScreen();
        }
        _oldKeyboardState = kbd;
    }

    private void NextRenderSliceMethod()
    {
        if (_renderer.Textures.Count > 0)
        {
            _renderer.RenderMethodIndex++;
            _renderer.RenderMethodIndex %= _renderer._renderSliceMethods.Count;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        _renderer.Draw(gameTime);
    }
}
}