using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;
using System.Threading;

namespace WpfAnimatedControl
{
    public class AnimatedImage : System.Windows.Controls.Image
    {
        private List<BitmapSource> _BitmapSources = null;
        private int _nCurrentFrame=0;
        private List<int> Delays = null;
        private Timer timer = null;
        
        private bool _bIsAnimating=false;

        public bool IsAnimating
        {
            get { return _bIsAnimating; }
        }

        static AnimatedImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedImage), new FrameworkPropertyMetadata(typeof(AnimatedImage)));
        }

        /// <summary>
        /// Animated GIF image.
        /// </summary>
        public Bitmap AnimatedBitmap
        {
            get { return (Bitmap)GetValue(AnimatedBitmapProperty); }
            set { StopAnimate(); SetValue(AnimatedBitmapProperty, value); }
        }

        public static readonly DependencyProperty AnimatedBitmapProperty =
            DependencyProperty.Register(
                "AnimatedBitmap", typeof(Bitmap), typeof(AnimatedImage),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnAnimatedBitmapChanged)));

        private static void OnAnimatedBitmapChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            AnimatedImage control = (AnimatedImage)obj;

            control.UpdateAnimatedBitmap();

            RoutedPropertyChangedEventArgs<Bitmap> e = new RoutedPropertyChangedEventArgs<Bitmap>(
                (Bitmap)args.OldValue, (Bitmap)args.NewValue, AnimatedBitmapChangedEvent);
            control.OnAnimatedBitmapChanged(e);
        }

        public static readonly RoutedEvent AnimatedBitmapChangedEvent = EventManager.RegisterRoutedEvent(
            "AnimatedBitmapChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<Bitmap>), typeof(AnimatedImage));

        public event RoutedPropertyChangedEventHandler<Bitmap> AnimatedBitmapChanged
        {
            add { AddHandler(AnimatedBitmapChangedEvent, value); }
            remove { RemoveHandler(AnimatedBitmapChangedEvent, value); }
        }

        /// <summary>
        /// Loads the smile out of the bitmap.
        /// </summary>
        /// <param name="bitmap">Animated gif file.</param>
        public void LoadSmile(Bitmap bitmap)
        {
            this.AnimatedBitmap = bitmap;
        }

         /// <summary>
         /// Raises the ValueChanged event.
         /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnAnimatedBitmapChanged(RoutedPropertyChangedEventArgs<Bitmap> args)
        {
            try
            {
                RaiseEvent(args);
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateAnimatedBitmap()
        {

            try
            {
                int nTimeFrames = GetFramesCount(); //get frames count
                _nCurrentFrame = 0; //Set current frame to default value
                if (nTimeFrames > 0) //this is animated file
                {
                    MemoryStream stream = new MemoryStream();
                    AnimatedBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Gif);
                    stream.Seek(0, SeekOrigin.Begin);
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    ParseGif(buffer);
                    _BitmapSources = new List<BitmapSource>(nTimeFrames);
                    stream.Dispose();
                    FillBitmapSources(nTimeFrames);
                    timer = new Timer(OnFrameChanged, null, -1 , -1); //Initialize timer
                    StartAnimate(); //start animation
                }
                else //this is single image
                {
                    Bitmap bitmap = new Bitmap(AnimatedBitmap);
                    _BitmapSources = new List<BitmapSource>(1);
                    _BitmapSources.Add(CreateBitmapSourceFromBitmap(bitmap));
                    Source = _BitmapSources[0];
                }
            }
            catch { }
        }

        /// <summary>
        /// Parse gif file to get delay for each frame
        /// </summary>
        /// <param name="buffer"></param>
        private void ParseGif(byte[] buffer)
        {
            ParseGif p_gif = new ParseGif();
            Delays = p_gif.ParseGifDataStream(buffer, 0); 
        }

        /// <summary>
        /// Fills the list of GIF frames.
        /// </summary>
        /// <param name="nTimeFrames"></param>
        private void FillBitmapSources(int nTimeFrames)
        {
            for (int i = 0; i < nTimeFrames; i++)
            {
                AnimatedBitmap.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, i);
                Bitmap bitmap = new Bitmap(AnimatedBitmap);
                bitmap.MakeTransparent();
                _BitmapSources.Add(CreateBitmapSourceFromBitmap(bitmap));
                _BitmapSources[i].Freeze();
            }
        }

        /// <summary>
        /// Returns frames count.
        /// </summary>
        /// <returns></returns>
        private int GetFramesCount()
        {
            try
            {
                return AnimatedBitmap.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
            }
            catch (Exception)
            {
                return 1;
            }
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource CreateBitmapSourceFromBitmap(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            IntPtr hBitmap = bitmap.GetHbitmap();

            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        private delegate void VoidDelegate();

        /// <summary>
        /// This method is executed on the timer.
        /// </summary>
        /// <param name="obj"></param>
        private void OnFrameChanged(object obj)
        {
            try
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new VoidDelegate(delegate { ChangeSource(); }));
            }
            catch { }
        }

        /// <summary>
        /// Changes the current GIF frame.
        /// </summary>
        void ChangeSource()
        {
            try
            {
                timer.Change(Delays[_nCurrentFrame] * 10, 0);
                Source = _BitmapSources[_nCurrentFrame++];
                _nCurrentFrame = _nCurrentFrame % _BitmapSources.Count;
            }
            catch { }
        }
        
        /// <summary>
        /// Stops animation process.
        /// </summary>
        public void StopAnimate()
        {
            try
            {
                if (_bIsAnimating)
                {
                    timer.Change(-1, -1);
                    _bIsAnimating = false;
                }
            }
            catch { }
        }

        /// <summary>
        /// Starts animation process.
        /// </summary>
        public void StartAnimate()
        {
            try
            {
                if (!_bIsAnimating)
                {
                    timer.Change(0, 0);
                    _bIsAnimating = true;
                }
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                timer.Change(-1, -1);
                timer.Dispose();
                _BitmapSources.Clear();
                Source = null;
                GC.Collect();
                GC.SuppressFinalize(this);
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch { }
        }
    }
}
