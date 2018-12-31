using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace WpfAnimatedGif
{
    static class AnimationCache
    {
        private class CacheKey
        {
            private readonly ImageSource _source;
            private readonly RepeatBehavior _repeatBehavior;

            public CacheKey(ImageSource source, RepeatBehavior repeatBehavior)
            {
                _source = source;
                _repeatBehavior = repeatBehavior;
            }

            private bool Equals(CacheKey other)
            {
                return ImageEquals(_source, other._source)
                    && Equals(_repeatBehavior, other._repeatBehavior);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((CacheKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (ImageGetHashCode(_source) * 397) ^ _repeatBehavior.GetHashCode();
                }
            }

            private static int ImageGetHashCode(ImageSource image)
            {
                if (image != null)
                {
                    var uri = GetUri(image);
                    if (uri != null)
                        return uri.GetHashCode();
                }
                return 0;
            }

            private static bool ImageEquals(ImageSource x, ImageSource y)
            {
                if (Equals(x, y))
                    return true;
                if ((x == null) != (y == null))
                    return false;
                // They can't both be null or Equals would have returned true
                // and if any is null, the previous would have detected it
                // ReSharper disable PossibleNullReferenceException
                if (x.GetType() != y.GetType())
                    return false;
                // ReSharper restore PossibleNullReferenceException
                var xUri = GetUri(x);
                var yUri = GetUri(y);
                return xUri != null && xUri == yUri;
            }

            private static Uri GetUri(ImageSource image)
            {
                var bmp = image as BitmapImage;
                if (bmp != null && bmp.UriSource != null)
                {
                    if (bmp.UriSource.IsAbsoluteUri)
                        return bmp.UriSource;
                    if (bmp.BaseUri != null)
                        return new Uri(bmp.BaseUri, bmp.UriSource);
                }
                var frame = image as BitmapFrame;
                if (frame != null)
                {
                    string s = frame.ToString();
                    if (s != frame.GetType().FullName)
                    {
                        Uri fUri;
                        if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out fUri))
                        {
                            if (fUri.IsAbsoluteUri)
                                return fUri;
                            if (frame.BaseUri != null)
                                return new Uri(frame.BaseUri, fUri);
                        }
                    }
                }
                return null;
            }
        }

        private static readonly Dictionary<CacheKey, ObjectAnimationUsingKeyFrames> _animationCache = new Dictionary<CacheKey, ObjectAnimationUsingKeyFrames>();
        private static readonly Dictionary<CacheKey, int> _referenceCount = new Dictionary<CacheKey, int>();

        public static void IncrementReferenceCount(ImageSource source, RepeatBehavior repeatBehavior)
        {
            var cacheKey = new CacheKey(source, repeatBehavior);
            int count;
            _referenceCount.TryGetValue(cacheKey, out count);
            count++;
            _referenceCount[cacheKey] = count;
        }

        public static void DecrementReferenceCount(ImageSource source, RepeatBehavior repeatBehavior)
        {
            var cacheKey = new CacheKey(source, repeatBehavior);
            int count;
            _referenceCount.TryGetValue(cacheKey, out count);
            if (count > 0)
            {
                count--;
                _referenceCount[cacheKey] = count;
            }
            if (count == 0)
            {
                _animationCache.Remove(cacheKey);
                _referenceCount.Remove(cacheKey);
            }
        }

        public static void AddAnimation(ImageSource source, RepeatBehavior repeatBehavior, ObjectAnimationUsingKeyFrames animation)
        {
            var key = new CacheKey(source, repeatBehavior);
            _animationCache[key] = animation;
        }

        public static void RemoveAnimation(ImageSource source, RepeatBehavior repeatBehavior, ObjectAnimationUsingKeyFrames animation)
        {
            var key = new CacheKey(source, repeatBehavior);
            _animationCache.Remove(key);
        }
        public static void ClearAnimation()
        {
            _animationCache.Clear();
        }
        public static ObjectAnimationUsingKeyFrames GetAnimation(ImageSource source, RepeatBehavior repeatBehavior)
        {
            var key = new CacheKey(source, repeatBehavior);
            ObjectAnimationUsingKeyFrames animation;
            _animationCache.TryGetValue(key, out animation);
            return animation;
        }
    }
}