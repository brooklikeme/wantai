using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Net;

namespace ImageExtender
{
    public class ImageExpender:Image
    {
        string imageLocation;
        public string ImageLocation
        {
            get
            {
                return imageLocation;
            }
            set
            {
                if (imageLocation != value)
                {
                    imageLocation = value;

                    try
                    {
                        Load();
                    }
                    catch (Exception ep)
                    { }
                }
            }
        }

        //private System.Drawing.Image _Image;

        //public System.Drawing.Image Image
        //{
        //    get { return _Image; }
        //    set
        //    {
        //        StopAnimate();
        //        this._Image = value;

        //        Animate();

        //        RefreshImageSource();
        //    }
        //}
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        /// 
        public void Dispose()
        {
            ImageAnimatiorExpender.Dispose=false;
        }
        public System.Drawing.Image Image
        {
            get { return (System.Drawing.Image)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(System.Drawing.Image), typeof(ImageExpender), new UIPropertyMetadata(null, new PropertyChangedCallback(ImagePropertyChanged)));

        
        private static void ImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageAnimatiorExpender.Dispose = false;
            ImageExpender imageExpender = d as ImageExpender;
            System.Drawing.Image oldImage = e.OldValue as System.Drawing.Image;
            System.Drawing.Image newImage = e.NewValue as System.Drawing.Image;

            imageExpender.StopAnimate(oldImage);
            ImageAnimatiorExpender.Dispose = true;
            imageExpender.Animate(newImage);
            imageExpender.RefreshImageSource();
        }


        bool currentlyAnimating = false;

        #region Animate

        private void Animate(bool animate,System.Drawing.Image image)
        {
            if (animate != this.currentlyAnimating)
            {
                if (animate)
                {
                    if (image != null)
                    {
                        ImageAnimatiorExpender.Animate(image, new EventHandler(this.OnFrameChanged));
                        this.currentlyAnimating = animate;
                    }
                }
                else
                {
                    if (image != null)
                    {
                        ImageAnimatiorExpender.StopAnimate(image, new EventHandler(this.OnFrameChanged));
                        this.currentlyAnimating = animate;
                    }
                }
            }
        }

        private void StopAnimate(System.Drawing.Image image)
        {
            Animate(false,image);
        }

        private void Animate(System.Drawing.Image image)
        {
            Animate(IsEnabled && Visibility== Visibility.Visible,image);
        }

        #endregion

        private void OnFrameChanged(object o, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Threading.ThreadStart(RefreshImageSource));
        }

        private void RefreshImageSource()
        {
            if (this.Image != null)
            {
                ImageAnimatiorExpender.UpdateFrames(this.Image);
                ImageSource imageSource = ImageAnimatiorExpender.GetImageSource(this.Image);

                if (imageSource == null)
                {
                    IntPtr ip = (this.Image as System.Drawing.Bitmap).GetHbitmap();
                    imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ip, IntPtr.Zero, System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                }

                this.Source = imageSource;
            }

        }

        #region 辅助方法

        private Uri CalculateUri(string path)
        {
            Uri uri;
            try
            {
                uri = new Uri(path);
            }
            catch (UriFormatException)
            {
                // It's a relative pathname, get its full path as a file.
                path = Path.GetFullPath(path);
                uri = new Uri(path);
            }
            return uri;
        }

        public void Load()
        {
            if (ImageLocation == null || ImageLocation.Length == 0)
            {
                return;
            }

            System.Drawing.Image img = null;
            try
            {
                Uri uri = CalculateUri(ImageLocation);
                if (uri.IsFile)
                {
                    using (StreamReader reader = new StreamReader(uri.LocalPath))
                    {
                        //img = System.Drawing.Image.FromStream(reader.BaseStream);
                        img = new System.Drawing.Bitmap(uri.LocalPath);
                    }
                }
                else
                {
                    using (WebClient wc = new WebClient())
                    {
                        using (Stream s = wc.OpenRead(uri.ToString()))
                        {
                            img = System.Drawing.Bitmap.FromStream(s);
                            
                        }
                    }
                }
            }
            catch
            {

            }

            this.Image = img;
        }

        #endregion

    }
}
