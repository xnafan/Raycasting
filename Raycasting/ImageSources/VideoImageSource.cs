using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using Accord.Video.FFMPEG;

namespace Raycasting.ImageSources
{
    public class VideoImageSource : IImageSource
    {
        Texture2D _currentTexture;
        DateTime _lastImageReceivedTime;
        VideoFileReader _reader;
        int _msBetweenFrames;
        Bitmap _currentBitMap;
        private float _msSpentInCurrentFrame;
        public string VideoPath { get; set; }
        private int _currentFrame;
        public VideoImageSource(string videoPath)
        {
            _reader = new VideoFileReader();
            VideoPath = videoPath;
            _reader.Open(VideoPath);
            _msBetweenFrames = 1000 / _reader.FrameRate  ;
            _lastImageReceivedTime = DateTime.UtcNow;
            MoveToNextFrame();
            CreateTextureFromCurrentBitmap();
        }

        private void MoveToNextFrame()
        {
            var bmp = _reader.ReadVideoFrame();
            if (bmp == null)
            {
                _reader.Close();
                _reader.Open(VideoPath);
                bmp = _reader.ReadVideoFrame();
            }
            _currentBitMap = bmp;
            _msSpentInCurrentFrame = 0;
        }

        public void Update()
        {
            float _msLeftToSpend = (float)(DateTime.UtcNow - _lastImageReceivedTime).TotalMilliseconds;
            _msLeftToSpend += _msSpentInCurrentFrame;
            if (_msLeftToSpend >= _msBetweenFrames)
            {
                while (_msLeftToSpend > 0)
                {
                    _msLeftToSpend -= _msBetweenFrames;
                    MoveToNextFrame();
                }
                CreateTextureFromCurrentBitmap();
                _lastImageReceivedTime = DateTime.UtcNow;
                _msSpentInCurrentFrame = _msLeftToSpend;
            }
        }

        private void CreateTextureFromCurrentBitmap()
        {
            try
            {
                _currentTexture = ImageSourceFactory.BitmapToTexture2D(_currentBitMap);
                _currentBitMap.Dispose();
            }
            catch {}
        }

        public Texture2D CurrentTexture
        {
            get
            {                
                return _currentTexture;
            }
        }
    }
}