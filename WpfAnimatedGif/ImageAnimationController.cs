using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WpfAnimatedGif
{
    /// <summary>
    /// Provides a way to pause, resume or seek a GIF animation.
    /// </summary>
    public class ImageAnimationController : IDisposable
    {
        private static readonly DependencyPropertyDescriptor _sourceDescriptor;

        static ImageAnimationController()
        {
            _sourceDescriptor = DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof (Image));
        }

        private readonly Image _image;
        private  ObjectAnimationUsingKeyFrames _animation;
        private readonly AnimationClock _clock;
        private readonly ClockController _clockController;
        
        internal ImageAnimationController(Image image, ObjectAnimationUsingKeyFrames animation, bool autoStart)
        {
            _image = image;
            _animation = animation;
            _animation.Completed += AnimationCompleted;
            _clock = _animation.CreateClock();
            _clockController = _clock.Controller;
            _sourceDescriptor.AddValueChanged(image, ImageSourceChanged);

            // ReSharper disable once PossibleNullReferenceException
            _clockController.Pause();
            
            _image.ApplyAnimationClock(Image.SourceProperty, _clock);
            
            if (autoStart)
                _clockController.Resume();
        }

        void AnimationCompleted(object sender, EventArgs e)
        {
            _image.RaiseEvent(new System.Windows.RoutedEventArgs(ImageBehavior.AnimationCompletedEvent, _image));
        }

        private void ImageSourceChanged(object sender, EventArgs e)
        {
            OnCurrentFrameChanged();
        }

        /// <summary>
        /// Returns the number of frames in the image.
        /// </summary>
        public int FrameCount
        {
            get { return _animation.KeyFrames.Count; }
        }

        /// <summary>
        /// Returns a value that indicates whether the animation is paused.
        /// </summary>
        public bool IsPaused
        {
            get { return _clock.IsPaused; }
        }

        /// <summary>
        /// Returns a value that indicates whether the animation is complete.
        /// </summary>
        public bool IsComplete
        {
            get { return _clock.CurrentState == ClockState.Filling; }
        }

        /// <summary>
        /// Seeks the animation to the specified frame index.
        /// </summary>
        /// <param name="index">The index of the frame to seek to</param>
        public void GotoFrame(int index)
        {
            var frame = _animation.KeyFrames[index];
            _clockController.Seek(frame.KeyTime.TimeSpan, TimeSeekOrigin.BeginTime);
        }

        /// <summary>
        /// Returns the current frame index.
        /// </summary>
        public int CurrentFrame
        {
            get
            {
                var time = _clock.CurrentTime;
                var frameAndIndex =
                    _animation.KeyFrames
                              .Cast<ObjectKeyFrame>()
                              .Select((f, i) => new { Time = f.KeyTime.TimeSpan, Index = i })
                              .FirstOrDefault(fi => fi.Time >= time);
                if (frameAndIndex != null)
                    return frameAndIndex.Index;
                return -1;
            }
        }
        public ObjectAnimationUsingKeyFrames animatable
        {
            set => _animation=value;
        }
        public void chengeKeyFrames(ObjectKeyFrameCollection KeyFrames)
        {
            _animation.KeyFrames = KeyFrames;
        }
        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            _clockController.Pause();
        }

        /// <summary>
        /// Starts or resumes the animation. If the animation is complete, it restarts from the beginning.
        /// </summary>
        public void Play()
        {
            _clockController.Resume();
        }
        /// <summary>
        /// Starts or resumes the animation. If the animation is complete, it restarts from the beginning.
        /// </summary>
        public void Stop()
        {
            _clockController.Stop();
        }
        /// <summary>
        /// Starts or resumes the animation. If the animation is complete, it restarts from the beginning.
        /// </summary>
        public void Remove()
        {
            _clock.Controller.Remove();
            _clockController.Remove();
        }
        /// <summary>
        /// Raised when the current frame changes.
        /// </summary>
        public event EventHandler CurrentFrameChanged;

        private void OnCurrentFrameChanged()
        {
            EventHandler handler = CurrentFrameChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Finalizes the current object.
        /// </summary>
        ~ImageAnimationController()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the current object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object
        /// </summary>
        /// <param name="disposing">true to dispose both managed an unmanaged resources, false to dispose only managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image.BeginAnimation(Image.SourceProperty, null);
                _animation.Completed -= AnimationCompleted;
                _sourceDescriptor.RemoveValueChanged(_image, ImageSourceChanged);
                _image.Source = null;
            }
        }
    }
}