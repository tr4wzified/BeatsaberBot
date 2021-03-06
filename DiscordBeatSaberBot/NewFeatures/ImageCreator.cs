﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;
using System.Drawing.Drawing2D;

namespace NewFeatures
{
    class ImageCreator
    {
        private Bitmap _bitmap;
        public ImageCreator()
        {
            string imageFilePath = "../../../Resources/RankingCard-Template.png";
            _bitmap = (Bitmap)Image.FromFile(imageFilePath);
        }

        public void Create()
        {
            _bitmap.Save("../../../Resources/RankingCard.png");
        }

        public void AddText(string text, Color color, int fontsize, float x, float y)
        {
            PointF firstLocation = new PointF(x, y);

            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                using (Font arialFont = new Font("Tourmaline", fontsize))
                {
                    graphics.DrawString(text, arialFont, new SolidBrush(color), firstLocation);
                }
            }
        }

        public void AddImage(string path, float x, float y, int width, int height)
        {
            Image overlayImage = null;

            var request = WebRequest.Create(path);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                overlayImage = Bitmap.FromStream(stream);
            }

            Graphics g = Graphics.FromImage(_bitmap);
            g.DrawImage(overlayImage, x, y, width, height);
        }

        public void AddImageRounded(string path, float x, float y, int width, int height)
        {
            Image overlayImage = null;

            var request = WebRequest.Create(path);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                overlayImage = Bitmap.FromStream(stream);
            }

            Graphics g = Graphics.FromImage(_bitmap);
            
            g.DrawImage(RoundCorners(overlayImage, 25, Color.FromArgb(32,32,32)), x, y, width, height);

        }

        public void AddNoteSlashEffect(string path, float x, float y, int width, int height)
        {
            Image overlayImage = null;

            var request = WebRequest.Create(path);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                overlayImage = Bitmap.FromStream(stream);
            }

            Graphics g = Graphics.FromImage(_bitmap);

            var tuple = AddNoteSlashEffect(overlayImage, 25, Color.FromArgb(32, 32, 32));

            g.DrawImage(tuple.Item1, x / 2 + 100, y + 30, width / 2, height);
            g.DrawImage(tuple.Item2, x * 2 + 220, y - 30, width / 2, height);

        }

        private Image RoundCorners(Image StartImage, int CornerRadius, Color BackgroundColor)
        {
            CornerRadius *= 2;
            Bitmap RoundedImage = new Bitmap(StartImage.Width, StartImage.Height);
            using (Graphics g = Graphics.FromImage(RoundedImage))
            {
                g.Clear(BackgroundColor);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Brush brush = new TextureBrush(StartImage);
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0, CornerRadius, CornerRadius, 270, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                gp.AddArc(0, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                g.FillPath(brush, gp);
                return RoundedImage;
            }
        }

        private Tuple<Image,Image> AddNoteSlashEffect(Image StartImage, int CornerRadius, Color BackgroundColor)
        {
            Image finishedImage = null;
            CornerRadius *= 2;
            Bitmap RoundedImage = new Bitmap(StartImage.Width, StartImage.Height);
            using (Graphics g = Graphics.FromImage(RoundedImage))
            {
                g.Clear(BackgroundColor);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Brush brush = new TextureBrush(StartImage);
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0, CornerRadius, CornerRadius, 270, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                gp.AddArc(0, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                g.FillPath(brush, gp);

                Rectangle rect = new Rectangle(0, 0, RoundedImage.Width / 2, RoundedImage.Height);
                Bitmap leftSide = RoundedImage.Clone(rect, RoundedImage.PixelFormat);
              
                rect = new Rectangle(RoundedImage.Width / 2, 0, RoundedImage.Width / 2, RoundedImage.Height);
                Bitmap rightSide = RoundedImage.Clone(rect, RoundedImage.PixelFormat);

                return new Tuple<Image, Image>(leftSide, rightSide);
            }


        }
    }
}
