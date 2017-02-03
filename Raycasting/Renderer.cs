using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raycasting.ImageSources;
using Raycasting.Input;
using System;
using System.Collections.Generic;

namespace Raycasting
{
    public class Renderer
    {

        #region Variables
        public bool ShowHelp { get; set; }
        public List<IImageSource[]> Textures { get; set; } = new List<IImageSource[]>();
        public List<TextureSprite> Sprites = new List<TextureSprite>();
        public Player Player { get; set; }
        public List<Action<RenderDataForSlice>> _renderSliceMethods = new List<Action<RenderDataForSlice>>();
        public static List<AnimatedImageSource> AnimatedGifs = new List<AnimatedImageSource>();
        public bool PsychedelicMode { get; set; }
        public Rectangle ViewingField { get; set; }
        public int RenderMethodIndex;
        public int TexturesetIndex = 0;
        RenderTarget2D _target1, _target2;
        Vector2 _screenSize, _halfScreenSize;
        Texture2D _white;
        float _msForNextSlide, _msBetweenSlides = 10000;
        int _slideIndex = 0;
        int _halfWidthOfViewingFieldInPixels;
        int _widthOfViewingArcInDegrees = 60;
        SpriteFont _font;
        int[,] _maze;
        private RenderDataForSlice[] CompleteRenderData;
        float[] fisheyeCompensations;
        float degreePerPixel;

        public IImageSource[] CurrentTextureSet
        {
            get
            {
                if (Textures.Count == 0) { return null; }
                else { return Textures[TexturesetIndex]; }
            }
        }
        #endregion

        public Renderer(int width, int height, Player player, int[,] maze)
        {
            _renderSliceMethods.Add(RenderSliceWithDistanceBasedLighting);
            _renderSliceMethods.Add(RenderSliceForSlideShow);
            _renderSliceMethods.Add(RenderSliceWithGlide);
            _renderSliceMethods.Add(RenderSlice);

            _white = Game1.ContentManager.Load<Texture2D>("white");
            ViewingField = new Rectangle(0, 0, width, height);
            _halfWidthOfViewingFieldInPixels = ViewingField.Width / 2;
            _screenSize = new Vector2(ViewingField.Width, ViewingField.Height);
            _halfScreenSize = _screenSize / 2;
            _font = Game1.ContentManager.Load<SpriteFont>("DefaultFont");
            _target1 = new RenderTarget2D(Game1.CurrentGraphicsDevice, Game1.CurrentGraphicsDevice.PresentationParameters.BackBufferWidth, Game1.CurrentGraphicsDevice.PresentationParameters.BackBufferHeight);
            _target2 = new RenderTarget2D(Game1.CurrentGraphicsDevice, Game1.CurrentGraphicsDevice.PresentationParameters.BackBufferWidth, Game1.CurrentGraphicsDevice.PresentationParameters.BackBufferHeight);
            _maze = maze;
            Player = player;
            degreePerPixel = _widthOfViewingArcInDegrees / (float)ViewingField.Width;
            CompleteRenderData = new RenderDataForSlice[ViewingField.Width];

            PrecalculateFisheyeCompensations();
        }

        private void PrecalculateFisheyeCompensations()
        {
            fisheyeCompensations = new float[(int)_screenSize.X];
            for (int pixel = 0; pixel < ViewingField.Width; pixel++)
            {
                var realAngle = degreePerPixel * pixel - (_widthOfViewingArcInDegrees / 2);
                var angleFromCenter = Math.Abs(Player.ViewingAngle - realAngle);
                fisheyeCompensations[pixel] = (float)Math.Cos(MathHelper.ToRadians(angleFromCenter));
            }
        }

        public void Update(GameTime gameTime)
        {
            _msForNextSlide -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_msForNextSlide <= 0)
            {
                _slideIndex++;
                _msForNextSlide = _msBetweenSlides;
            }
            lock (Sprites)
            {
                for (int i = Sprites.Count - 1; i >= 0; i--)
                {
                    Sprites[i].Update(gameTime);
                    if (Sprites[i].Position.X > _screenSize.X + Sprites[i].DestinationRectangle.Width)
                    { Sprites.RemoveAt(i); }
                }
            }
            CalculateRenderData();
            AnimatedGifs.ForEach(gif => gif.Update(gameTime));
        }

        private void CalculateRenderData()
        {

            float halfWidthOfViewingArcInDegrees = _widthOfViewingArcInDegrees / 2;
            float absoluteAngleOfLeftMostPeripheralVision = Player.ViewingAngle - halfWidthOfViewingArcInDegrees;
            for (int pixel = 0; pixel < ViewingField.Width; pixel++)
            {
                var realAngle = absoluteAngleOfLeftMostPeripheralVision + degreePerPixel * pixel;
                var angleFromCenter = Math.Abs(Player.ViewingAngle - realAngle);
                CollisionInfo? collisionPosition = _maze.GetCollisionPointImproved(Player.Position, realAngle, 100);

                if (!collisionPosition.HasValue)
                {
                    CompleteRenderData[pixel] = null;
                    continue;
                }
                var adjustedDistanceToCollision = collisionPosition.Value.DistanceToCollision * fisheyeCompensations[pixel];
                var destinationHeight = ViewingField.Height / adjustedDistanceToCollision;

                float percentageOfWidth = pixel / _widthOfViewingArcInDegrees;
                var destinationRectangle = new Rectangle(pixel, (int)((ViewingField.Height - destinationHeight) / 2),
                    1, (int)destinationHeight);

                CompleteRenderData[pixel] = new RenderDataForSlice() { CollisionInfo = collisionPosition.Value, DestinationRectangle = destinationRectangle };
            }
        }

        public void Draw(GameTime gameTime)
        {
            Game1.CurrentGraphicsDevice.Clear(Color.Black);
            //Game1.SpriteBatch.Begin();
            //DrawFloor(gameTime);
            //Game1.SpriteBatch.End();

            var renderSliceMethod = GetCurrentSliceRenderMethod();
            if (PsychedelicMode) { RenderAllToTarget(gameTime, renderSliceMethod); }
            else { RenderAll(gameTime, renderSliceMethod); }
            Game1.SpriteBatch.Begin();
            DrawHelp(gameTime);
            Game1.SpriteBatch.End();
        }

        private void DrawFloor(GameTime gameTime)
        {
            //var projectionPlaneHeightToWidthRatio = _screen.Y / _screen.X;
            //var heightOfViewingArcInDegrees = _widthOfViewingArcInDegrees * projectionPlaneHeightToWidthRatio;
            //var heightOfEyesAboveFloor = .5f;

            //for (int x = 0; x < CompleteRenderData.Length; x+=2)
            //{
            //    for (int y = CompleteRenderData[x].DestinationRectangle.Bottom; y < _screen.Y; y+=2)
            //    {
            //        Game1.SpriteBatch.Draw(_white, new Rectangle(x, y, 2, 2), Color.CornflowerBlue);
            //    }
            //}
            int halfScreenHeight = (int)_screenSize.Y / 2;
            Game1.SpriteBatch.Draw(_white, new Rectangle(0, halfScreenHeight, (int)_screenSize.X, halfScreenHeight), Color.Teal);
        }

        public void NextTextureSet()
        {
            if (Textures.Count == 0) { return; }
            TexturesetIndex++;
            TexturesetIndex %= Textures.Count;
        }

        public void PreviousTextureSet()
        {
            if (Textures.Count == 0) { return; }
            TexturesetIndex--;
            TexturesetIndex += Textures.Count;
            TexturesetIndex %= Textures.Count;
        }

        public void NextRenderSliceMethod()
        {
            if (Textures.Count > 0)
            {
                RenderMethodIndex++;
                RenderMethodIndex %= _renderSliceMethods.Count;
            }
        }

        private void RenderSprites(GameTime gameTime)
        {
            lock (Sprites)
            {
                Sprites.ForEach(sprite => sprite.Draw(gameTime));
            }
        }

        private Action<RenderDataForSlice> GetCurrentSliceRenderMethod()
        {
            if (Textures.Count > 0) { return _renderSliceMethods[RenderMethodIndex]; }
            else { return RenderWhiteSlice; }
        }

        private void RenderAll(GameTime gameTime, Action<RenderDataForSlice> renderMethod)
        {
            Game1.SpriteBatch.Begin();

            for (int pixel = 0; pixel < ViewingField.Width; pixel++)
            {
                if (CompleteRenderData[pixel] != null)
                    renderMethod(CompleteRenderData[pixel]);
            }
            Game1.SpriteBatch.End();
        }

        internal void AddTexture(Texture2D texture)
        {
            lock (Sprites)
            {
                Sprites.Add(new TextureSprite() { Texture = texture, Movement = Vector2.UnitX / 3, Position = new Vector2(-20, _screenSize.Y - 100) });
            }
        }

        private void RenderAllToTargetTwice(GameTime gameTime, Action<RenderDataForSlice> renderSliceMethod)
        {
            RenderAllToTarget(gameTime, renderSliceMethod);
            RenderAllToTarget(gameTime, RenderSliceFromTarget);
            RenderAll(gameTime, RenderSliceFromTarget);
        }

        private void RenderAllToTarget(GameTime gameTime, Action<RenderDataForSlice> renderSliceMethod)
        {
            Game1.CurrentGraphicsDevice.SetRenderTarget(_target1);
            RenderAll(gameTime, renderSliceMethod);
            Game1.CurrentGraphicsDevice.SetRenderTarget(null);
            Game1.SpriteBatch.Begin();
            var scale = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            scale = 1.4f + Math.Abs(scale);
            Game1.SpriteBatch.Draw(_target1, _halfScreenSize, null, Color.White, 0, _halfScreenSize, scale, SpriteEffects.None, 0);
            Game1.SpriteBatch.End();

            RenderAll(gameTime, RenderSliceFromTarget);
        }

        private void RenderWhiteSlice(RenderDataForSlice renderData)
        {
            var sourceRectangle1 = new Rectangle(0, 0, 1, 1);
            float maxDistance = 7;
            float opacity = (1 - (float)(renderData.CollisionInfo.Value.DistanceToCollision / maxDistance));
            Game1.SpriteBatch.Draw(_white, renderData.DestinationRectangle, sourceRectangle1, Color.White * opacity);
        }

        private void RenderSliceFromTarget(RenderDataForSlice renderData)
        {
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = _target1.Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * ViewingField.Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, ViewingField.Height);
            Game1.SpriteBatch.Draw(_target1, renderData.DestinationRectangle, sourceRectangle1, Color.White);
        }

        private void RenderSlice(RenderDataForSlice renderData)
        {
            //TODO: find out whether sharper viewing angle  should result in thinner sourceRectangles
            //for optimal rendering?
            int textureIndex1 = renderData.CollisionInfo.Value.TileHitValue - 1;
            textureIndex1 %= CurrentTextureSet.Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = CurrentTextureSet[textureIndex1].CurrentTexture.Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * CurrentTextureSet[textureIndex1].CurrentTexture.Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, CurrentTextureSet[textureIndex1].CurrentTexture.Height);
            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex1].CurrentTexture, renderData.DestinationRectangle, sourceRectangle1, Color.White);
        }

        private void RenderSliceWithGlide(RenderDataForSlice renderData)
        {
            int textureIndex1 = renderData.CollisionInfo.Value.TileHitValue - 1;
            textureIndex1 %= CurrentTextureSet.Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = CurrentTextureSet[textureIndex1].CurrentTexture.Width / _widthOfViewingArcInDegrees;
            var currentTexture = CurrentTextureSet[textureIndex1].CurrentTexture;
            Rectangle source;
            float offset = _msForNextSlide / _msBetweenSlides;
            offset = QuinticEaseInOut((float)Math.Abs(offset * 2 - 1));
            if (currentTexture.Width > currentTexture.Height)
            {
                float lengthToGlide = currentTexture.Width - currentTexture.Height;
                source = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * currentTexture.Height + offset * lengthToGlide), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, currentTexture.Height);
            }
            else
            {
                float lengthToGlide = currentTexture.Height - currentTexture.Width;
                source = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * currentTexture.Width), (int)(offset * lengthToGlide), _pixelsPerDegreeOfViewingAngleFromSourceBitmap, currentTexture.Width);
            }

            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex1].CurrentTexture, renderData.DestinationRectangle, source, Color.White);
        }

        private void RenderSliceWithDistanceBasedLighting(RenderDataForSlice renderData)
        {
            int textureIndex1 = renderData.CollisionInfo.Value.TileHitValue - 1;
            textureIndex1 %= CurrentTextureSet.Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = CurrentTextureSet[textureIndex1].CurrentTexture.Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * CurrentTextureSet[textureIndex1].CurrentTexture.Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, CurrentTextureSet[textureIndex1].CurrentTexture.Height);
            float maxDistance = 7;
            float opacity = (1 - (float)(renderData.CollisionInfo.Value.DistanceToCollision / maxDistance));
            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex1].CurrentTexture, renderData.DestinationRectangle, sourceRectangle1, Color.White * opacity);
        }

        float BezierBlend(float t) { return (float)Math.Pow(t, 2) * (3.0f - 2.0f * t); }
        static public float QuinticEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 16 * p * p * p * p * p;
            }
            else
            {
                float f = ((2 * p) - 2);
                return 0.5f * f * f * f * f * f + 1;
            }
        }

        private void RenderSliceForSlideShow(RenderDataForSlice renderData)
        {
            int textureIndex1 = renderData.CollisionInfo.Value.TileHitValue - 1 + _slideIndex;
            textureIndex1 %= CurrentTextureSet.Length;

            int textureIndex2 = (textureIndex1 + 1) % CurrentTextureSet.Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap1 = CurrentTextureSet[textureIndex1].CurrentTexture.Width / _widthOfViewingArcInDegrees;

            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap2 = CurrentTextureSet[textureIndex2].CurrentTexture.Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * CurrentTextureSet[textureIndex1].CurrentTexture.Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap1, CurrentTextureSet[textureIndex1].CurrentTexture.Height);
            var sourceRectangle2 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * CurrentTextureSet[textureIndex2].CurrentTexture.Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap2, CurrentTextureSet[textureIndex2].CurrentTexture.Height);

            float transparency = _msForNextSlide / _msBetweenSlides;
            transparency = QuinticEaseInOut(transparency);
            float maxDistance = 7;
            float opacity = (1 - (float)(renderData.CollisionInfo.Value.DistanceToCollision / maxDistance));
            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex1].CurrentTexture, renderData.DestinationRectangle, sourceRectangle1, Color.White);
            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex2].CurrentTexture, renderData.DestinationRectangle, sourceRectangle2, Color.White * (1 - transparency));
        }

        private void DrawHelp(GameTime gameTime)
        {


            Vector2 top = Vector2.UnitY * (ViewingField.Height - 80);
            Vector2 interval = Vector2.UnitY * 20;
            var textureText = "loading first imageset";
            if (Textures.Count > 0)
            {
                textureText = "imageset " + (TexturesetIndex + 1) + " of " + Textures.Count;
            }

            var textureTextPosition = _screenSize - _font.MeasureString(textureText) - Vector2.UnitX * 10;
            Game1.SpriteBatch.DrawString(_font, textureText, textureTextPosition, Color.White);

            if (!ShowHelp)
            {
                top.Y = ViewingField.Height - 20;
                Game1.SpriteBatch.DrawString(_font, "[F1] for Help", top, Color.White);
                return;
            }
            RenderSprites(gameTime);
            DrawMap();
            Game1.SpriteBatch.DrawString(_font, Player.ToString(), Vector2.Zero, Color.White);
            Game1.SpriteBatch.DrawString(_font, "[F10] to cycle presentationmodes", top, Color.White);
            top += interval;
            Game1.SpriteBatch.DrawString(_font, "[F11] to toggle fullscreen", top, Color.White);
            top += interval;
            Game1.SpriteBatch.DrawString(_font, "[NUMLOCK] to switch textures", top, Color.White);
            top += interval;
            Game1.SpriteBatch.DrawString(_font, "[P] to toggle psychedelic mode", top, Color.White);
        }

        public void DrawMap()
        {
            int size = 10;
            int yOffset = 20;
            for (int x = 0; x < Player.Map.GetLength(0); x++)
            {
                for (int y = 0; y < Player.Map.GetLength(1); y++)
                {
                    Game1.SpriteBatch.Draw(_white, new Rectangle(x * size, y * size + yOffset, size, size), Player.Map[x, y] != 0 ? Color.White * .6f : Color.Transparent);
                }
            }
            Game1.SpriteBatch.Draw(_white, new Rectangle((int)(Player.Position.X * size - size / 2), (int)(Player.Position.Y * size + yOffset - size / 2), size, size), Color.Red);

            int direction = ((int)Player.ViewingAngle + 45) / 90;
            switch (direction)
            {
                case 0:
                    Game1.SpriteBatch.Draw(_white, new Rectangle((int)(Player.Position.X * size + size / 2), (int)(Player.Position.Y * size + yOffset - size / 2), 1, size), Color.White);
                    break;

                case 1:
                    Game1.SpriteBatch.Draw(_white, new Rectangle((int)(Player.Position.X * size - size / 2), (int)(Player.Position.Y * size + yOffset + size / 2), size, 1), Color.White);
                    break;
                case 2:
                    Game1.SpriteBatch.Draw(_white, new Rectangle((int)(Player.Position.X * size - size / 2), (int)(Player.Position.Y * size + yOffset - size / 2), 1, size), Color.White);
                    break;

                case 3:
                    Game1.SpriteBatch.Draw(_white, new Rectangle((int)(Player.Position.X * size - size / 2), (int)(Player.Position.Y * size + yOffset - size / 2), size, 1), Color.White);
                    break;
                default:
                    break;
            }
            if (AutonomousPathfinderMover._nextTarget.HasValue)
            {
                Game1.SpriteBatch.Draw(_white, new Rectangle((int)(AutonomousPathfinderMover._nextTarget.Value.Target.X * size - size / 2), (int)(AutonomousPathfinderMover._nextTarget.Value.Target.Y * size + yOffset - size / 2), size, size), Color.Blue);
            }

        }
    }
}
