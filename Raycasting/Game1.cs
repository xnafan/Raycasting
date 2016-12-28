using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Collections.Generic;

namespace Raycasting
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        int[,] _maze = new int[15, 15];
        Texture2D[] _textures;
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
            _textures = GetImages();
            
            
            for (int x = 0; x <= _maze.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= _maze.GetUpperBound(1); y++)
                {
                    if (y == 0 || x == 0 || y == _maze.GetUpperBound(1) || x == _maze.GetUpperBound(0))
                    {
                        _maze[x, y] = 1+_rnd.Next(_textures.Length);
                    }
                }
            }
            for (int i = 0; i < 50; i++)
            {
                _maze[_rnd.Next(_maze.GetUpperBound(0)+1), _rnd.Next(_maze.GetUpperBound(1)+1)] = 1 + _rnd.Next(_textures.Length);
            }

            _maze[1, 1] = 2;
            _player = new Player(_maze);
            _pixelsPerDegreeOfViewingAngleFromSourceBitmap = _textures[0].Width / _viewingAngle;
            _pixelsPerDegreeOfViewingAngleFromViewArea = _widthOfViewingField / (_viewingAngle*_raysPerDegreeOrResolutionIfYoudRatherCallItThat);
            _player.Position = new Vector2(4.5f, 4.5f);
            _maze[(int)_player.Position.X, (int)_player.Position.Y] = 0;
            _player.ViewingAngle = 0;
            _font = Content.Load<SpriteFont>("DefaultFont");
            Sounds.Instance.Bump = Content.Load<SoundEffect>("Bump");
        }

        private Texture2D[] GetImages()
        {
            List<Texture2D> textures = new List<Texture2D>();
            var files = Directory.GetFiles(@"D:\Dropbox\Programming\XNA\Raycasting\Raycasting\Images");
            foreach (var item in files)
            {
                
                using (FileStream fileStream = new FileStream(item, FileMode.Open))
                {
                   textures.Add(Texture2D.FromStream(GraphicsDevice, fileStream));
                }
            }
            return textures.ToArray();
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

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            base.Draw(gameTime);
            //_spriteBatch.Draw(_wallImage, Vector2.One * 64,  Color.White);
            for (float deltaAngle = 0; deltaAngle < _viewingAngle; deltaAngle += 1/(float)_raysPerDegreeOrResolutionIfYoudRatherCallItThat)
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

                var textureIndex = _maze[(int)collisionPosition.Value.X, (int)collisionPosition.Value.Y] - 1;
                var destinationHeight = _heightOfViewingField / distanceToCollision / fisheyeCompensation;

                var sourceRectangle = new Rectangle((int)(sourceBitmapPositionToGrabFrom * _textures[textureIndex].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, _textures[textureIndex].Height);

                float percentageOfWidth = deltaAngle / _viewingAngle;
                var destinationRectangle = new Rectangle((int)( _widthOfViewingField * percentageOfWidth), (int)((_heightOfViewingField - destinationHeight) / 2), _pixelsPerDegreeOfViewingAngleFromViewArea , (int)destinationHeight);
                
                _spriteBatch.Draw(_textures[textureIndex], destinationRectangle, sourceRectangle, Color.White);
            }

            _spriteBatch.DrawString(_font, _player.ToString(), Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }

    public class Player
    {
        public Vector2 Position { get; set; }
        public float ViewingAngle { get; set; }
        public int[,] Map { get; set; }

        public Player(int[,] map)
        {
            Map = map;
        }

        internal void MoveBackwards()
        {
            var tempStep = ((float)MathHelper.ToRadians(this.ViewingAngle)).GetAngleAsVector() * .07f;
            var newPosition = this.Position - tempStep;
            PerformMoveIfNotBlocked(newPosition);
        }

        private void PerformMoveIfNotBlocked(Vector2 newPosition)
        {

            if (!Map.IsBlocked(newPosition))
            {
                this.Position = newPosition;
            }
            else
            {
                var positionHorizontal = new Vector2(newPosition.X, this.Position.Y);
                if (!Map.IsBlocked(positionHorizontal))
                {
                    this.Position = positionHorizontal;
                }
                else
                {
                    var positionVertical = new Vector2(this.Position.X, newPosition.Y);
                    if (!Map.IsBlocked(positionVertical))
                    {
                        this.Position = positionVertical;
                    }
                }
                Sounds.Instance.Bump.Play();
            }
        }

        internal void MoveForward()
        {
            var tempStep = ((float)MathHelper.ToRadians(this.ViewingAngle)).GetAngleAsVector() * .07f;
            var newPosition = this.Position + tempStep;
            PerformMoveIfNotBlocked(newPosition);
        }

        public override string ToString()
        {
            return "Pos: " + this.Position + ", viewangle: " + this.ViewingAngle;
        }

        internal void TurnRight()
        {
            this.ViewingAngle += 3f;
        }

        internal void TurnLeft()
        {
            this.ViewingAngle -= 3f;
        }
    }

    public static class MapHelper
    {
        public static float GetDistanceToObstacle(this int[,] map, Vector2 position, float angleInRadians)
        {
            var collisionPosition = GetCollisionPoint(map, position, angleInRadians);
            if (collisionPosition.HasValue)
            {
                return Vector2.Distance(position, collisionPosition.Value);
            }
            else
            { return 1000; }
        }

        public static Vector2? GetCollisionPoint(this int[,] map, Vector2 position, float angleInRadians)
        {
            float stepSize = 0.01f;
            float maxDistance = 20;
            Vector2 directionOfRay = GetAngleAsVector(angleInRadians);
            for (float distance = stepSize; distance < maxDistance; distance += stepSize)
            {
                Vector2 positionToTest = position + directionOfRay * distance;
                if (map[(int)positionToTest.X, (int)positionToTest.Y] != 0)
                    return positionToTest;
            }
            return null;
        }

        public static Vector2 GetAngleAsVector(this float angle)
        {
            var newDirection = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            newDirection.Normalize();
            return newDirection;
        }
        public static bool IsBlocked(this int[,] map, Vector2 position)
        {
            return map[(int)position.X, (int)position.Y] != 0;
        }

        public static float GetPositionOnWall(this Vector2 position)
        {
            float xResulting = 0;
            var xFraction = (position.X - (int)position.X);
            if (xFraction > .5f) { xResulting = Math.Abs(1 - xFraction); }
            else { xResulting = xFraction; }

            float yResulting = 0;
            var yFraction = (position.Y - (int)position.Y);
            if (yFraction > .5f) { yResulting = Math.Abs(1 - yFraction); }
            else { yResulting = yFraction; }

            if (xResulting > yResulting) { return xFraction; }
            else { return yFraction; }
        }
    }
}

