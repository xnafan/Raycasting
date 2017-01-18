using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Raycasting
{
    public class Renderer
    {
        public bool ShowHelp { get; set; }
        public List<Texture2D[]> Textures { get; set; } = new List<Texture2D[]>();
        public List<TextureSprite> Sprites  = new List<TextureSprite>();
        public Player Player { get; set; }
        public List<Action<RenderData>> _renderSliceMethods = new List<Action<RenderData>>();
        public bool PsychedelicMode { get; set; }
        public Rectangle ViewingField { get; set; }
        public int RenderMethodIndex;
        public int TexturesetIndex = 0;
        RenderTarget2D _target;
        Vector2 _screen, _halfScreen;
        Texture2D _white;
        float _msForNextSlide, _msBetweenSlides = 5000;
        int _slideIndex = 0;
        int _halfWidthOfViewingFieldInPixels;
        int _widthOfViewingArcInDegrees = 60;
        SpriteFont _font;
        int[,] _maze;
        private RenderData[] CompleteRenderData;

        public Texture2D[] CurrentTextureSet
        {
            get {
                if(Textures.Count == 0) { return null; }
                else{return Textures[TexturesetIndex];}
            }
        }


        public Renderer(int width, int height, Player player, int[,] maze)
        {
            _renderSliceMethods.Add(RenderSliceWithDistanceBasedLighting);
            _renderSliceMethods.Add(RenderSliceForSlideShow);
            _renderSliceMethods.Add(RenderSlice);
            _white = Game1.ContentManager.Load<Texture2D>("white");
            ViewingField = new Rectangle(0, 0, width, height);
            _halfWidthOfViewingFieldInPixels = ViewingField.Width / 2;
            _screen = new Vector2(ViewingField.Width, ViewingField.Height);
            _halfScreen = _screen / 2;
            _font = Game1.ContentManager.Load<SpriteFont>("DefaultFont");
            _target = new RenderTarget2D(Game1.CurrentGraphicsDevice, Game1.CurrentGraphicsDevice.PresentationParameters.BackBufferWidth, Game1.CurrentGraphicsDevice.PresentationParameters.BackBufferHeight);
            _maze = maze;
            Player = player;
            CompleteRenderData = new RenderData[ViewingField.Width];
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
                    if (Sprites[i].Position.X > _screen.X + Sprites[i].DestinationRectangle.Width)
                    { Sprites.RemoveAt(i); }
                }
            }
            CalculateRenderData();
        }

        private void CalculateRenderData()
        {
            float degreePerPixel = _widthOfViewingArcInDegrees / (float)ViewingField.Width;
            float halfWidthOfViewingArcInDegrees = _widthOfViewingArcInDegrees / 2;
            float absoluteAngleOfLeftMostPeripheralVision = Player.ViewingAngle - halfWidthOfViewingArcInDegrees;
            for (int pixel = 0; pixel < ViewingField.Width; pixel++)
            {
                var realAngle = absoluteAngleOfLeftMostPeripheralVision + degreePerPixel * pixel;
                var angleFromCenter = Math.Abs(Player.ViewingAngle - realAngle);
                var fisheyeCompensation = (float)Math.Cos(MathHelper.ToRadians(angleFromCenter));

                CollisionInfo? collisionPosition = _maze.GetCollisionPointImproved(Player.Position, realAngle, 100);

                

                if (!collisionPosition.HasValue)
                {
                    CompleteRenderData[pixel] = null;
                    continue;
                }
                var adjustedDistanceToCollision = collisionPosition.Value.DistanceToCollision * fisheyeCompensation;
                var destinationHeight = ViewingField.Height / adjustedDistanceToCollision;

                float percentageOfWidth = pixel / _widthOfViewingArcInDegrees;
                var destinationRectangle = new Rectangle(pixel, (int)((ViewingField.Height - destinationHeight) / 2),
                    1, (int)destinationHeight);

                CompleteRenderData[pixel] = new RenderData() { CollisionInfo = collisionPosition.Value, DestinationRectangle = destinationRectangle };            
            }
        }

        public void Draw(GameTime gameTime)
        {
            Game1.CurrentGraphicsDevice.Clear(Color.Black);
            var renderSliceMethod = GetCurrentSliceRenderMethod();
            if (PsychedelicMode) { RenderAllToTarget(gameTime, renderSliceMethod); }
            else { RenderAll(gameTime, renderSliceMethod); }
            Game1.SpriteBatch.Begin();
            RenderSprites(gameTime);
            DrawHelp();
            Game1.SpriteBatch.End();
        }

        public void NextTextureSet()
        {
            if(Textures.Count == 0) { return; }
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


        private void RenderSprites(GameTime gameTime)
        {
            lock (Sprites)
            {
                Sprites.ForEach(sprite => sprite.Draw(gameTime));
            }
        }

        private Action<RenderData> GetCurrentSliceRenderMethod()
        {
            if (Textures.Count > 0){ return _renderSliceMethods[RenderMethodIndex]; }
            else{return RenderWhiteSlice;}
        }

        private void RenderAll(GameTime gameTime, Action<RenderData> renderMethod)
        {
            Game1.SpriteBatch.Begin();
            
            for (int pixel = 0; pixel < ViewingField.Width; pixel++)
            {
                if(CompleteRenderData[pixel] != null)
                renderMethod(CompleteRenderData[pixel]);
            }

            Game1.SpriteBatch.End();
        }

        internal void AddTexture(Texture2D texture)
        {
            lock (Sprites)
            {
                Sprites.Add(new TextureSprite() { Texture = texture, Movement = Vector2.UnitX / 3, Position = new Vector2(-20, _screen.Y - 100) });
            }
        }

        private void RenderAllToTargetTwice(GameTime gameTime, Action<RenderData> renderSliceMethod)
        {
            Game1.CurrentGraphicsDevice.SetRenderTarget(_target);
            RenderAllToTarget(gameTime, renderSliceMethod);
            Game1.CurrentGraphicsDevice.SetRenderTarget(null);
            RenderAllToTarget(gameTime, RenderSliceFromTarget);
            RenderAll(gameTime, RenderSliceFromTarget);
        }

        private void RenderAllToTarget(GameTime gameTime, Action<RenderData> renderSliceMethod)
        {
            Game1.CurrentGraphicsDevice.SetRenderTarget(_target);
            RenderAll(gameTime, renderSliceMethod);
            Game1.CurrentGraphicsDevice.SetRenderTarget(null);
            Game1.SpriteBatch.Begin();
            var scale = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            scale = 1.4f + Math.Abs(scale);
            Game1.SpriteBatch.Draw(_target, _halfScreen, null, Color.White, 0, _halfScreen, scale, SpriteEffects.None, 0);
            Game1.SpriteBatch.End();

            RenderAll(gameTime, RenderSliceFromTarget);
        }

        private void RenderWhiteSlice(RenderData renderData)
        {
            var sourceRectangle1 = new Rectangle(0, 0, 1, 1);
            float maxDistance = 7;
            float opacity = (1 - (float)(renderData.CollisionInfo.Value.DistanceToCollision / maxDistance));
            Game1.SpriteBatch.Draw(_white, renderData.DestinationRectangle, sourceRectangle1, Color.White * opacity);
        }

        private void RenderSliceFromTarget(RenderData renderData)
        {
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = _target.Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * ViewingField.Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, ViewingField.Height);
            Game1.SpriteBatch.Draw(_target, renderData.DestinationRectangle, sourceRectangle1, Color.White);
        }

        private void RenderSlice(RenderData renderData)
        {
            int textureIndex1 = renderData.CollisionInfo.Value.TileHitValue - 1;
            textureIndex1 %= CurrentTextureSet.Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = CurrentTextureSet[textureIndex1].Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * CurrentTextureSet[textureIndex1].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, CurrentTextureSet[textureIndex1].Height);
            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex1], renderData.DestinationRectangle, sourceRectangle1, Color.White);
        }

        private void RenderSliceWithDistanceBasedLighting(RenderData renderData)
        {
            int textureIndex1 = renderData.CollisionInfo.Value.TileHitValue - 1;
            textureIndex1 %= CurrentTextureSet.Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap = CurrentTextureSet[textureIndex1].Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * CurrentTextureSet[textureIndex1].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap, CurrentTextureSet[textureIndex1].Height);
            float maxDistance = 7;
            float opacity = (1 - (float)(renderData.CollisionInfo.Value.DistanceToCollision / maxDistance));
            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex1], renderData.DestinationRectangle, sourceRectangle1, Color.White * opacity);
        }

        float BezierBlend(float t) { return (float)Math.Pow(t, 2) * (3.0f - 2.0f * t); }

        private void RenderSliceForSlideShow(RenderData renderData)
        {
            int textureIndex1 = renderData.CollisionInfo.Value.TileHitValue - 1 + _slideIndex;
            textureIndex1 %= CurrentTextureSet.Length;

            int textureIndex2 = (textureIndex1 + 1) % CurrentTextureSet.Length;
            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap1 = CurrentTextureSet[textureIndex1].Width / _widthOfViewingArcInDegrees;

            int _pixelsPerDegreeOfViewingAngleFromSourceBitmap2 = CurrentTextureSet[textureIndex2].Width / _widthOfViewingArcInDegrees;
            var sourceRectangle1 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * CurrentTextureSet[textureIndex1].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap1, CurrentTextureSet[textureIndex1].Height);
            var sourceRectangle2 = new Rectangle((int)(renderData.CollisionInfo.Value.PositionOnWall * CurrentTextureSet[textureIndex2].Width), 0, _pixelsPerDegreeOfViewingAngleFromSourceBitmap2, CurrentTextureSet[textureIndex2].Height);

            float transparency = _msForNextSlide / _msBetweenSlides;
            transparency = BezierBlend(transparency);
            float maxDistance = 7;
            float opacity = (1 - (float)(renderData.CollisionInfo.Value.DistanceToCollision / maxDistance));
            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex1], renderData.DestinationRectangle, sourceRectangle1, Color.White);
            Game1.SpriteBatch.Draw(CurrentTextureSet[textureIndex2], renderData.DestinationRectangle, sourceRectangle2, Color.White * (1 - transparency));
        }

        private void DrawHelp()
        {

            Vector2 top = Vector2.UnitY * (ViewingField.Height - 80);
            Vector2 interval = Vector2.UnitY * 20;
            var textureText = "loading first imageset";
            if (Textures.Count > 0)
            {
                textureText = "imageset " + (TexturesetIndex + 1) + " of " + Textures.Count;
            }

            var textureTextPosition = _screen - _font.MeasureString(textureText) - Vector2.UnitX * 10;
            Game1.SpriteBatch.DrawString(_font, textureText, textureTextPosition, Color.White);

            if (!ShowHelp)
            {
                top.Y = ViewingField.Height - 20;
                Game1.SpriteBatch.DrawString(_font, "[F1] for Help", top, Color.White);
                return;
            }
            Game1.SpriteBatch.DrawString(_font, Player.ToString(), Vector2.Zero, Color.White);
            Game1.SpriteBatch.DrawString(_font, "[F10] to cycle presentationmodes", top, Color.White);
            top += interval;
            Game1.SpriteBatch.DrawString(_font, "[F11] to toggle fullscreen", top, Color.White);
            top += interval;
            Game1.SpriteBatch.DrawString(_font, "[NUMLOCK] to switch textures", top, Color.White);
            top += interval;
            Game1.SpriteBatch.DrawString(_font, "[P] to toggle psychedelic mode", top, Color.White);
        }
    }
}
