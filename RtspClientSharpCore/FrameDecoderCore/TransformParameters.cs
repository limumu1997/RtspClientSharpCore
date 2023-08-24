using SkiaSharp;

namespace FrameDecoderCore
{
    public class TransformParameters
    {
        public SKRectI  RegionOfInterest { get; }

        public SKSizeI TargetFrameSize { get; }

        public ScalingPolicy ScalePolicy { get; }

        public PixelFormat TargetFormat { get; }

        public ScalingQuality ScaleQuality { get; }

        public TransformParameters(SKRectI regionOfInterest, SKSizeI targetFrameSize, ScalingPolicy scalePolicy,
            PixelFormat targetFormat, ScalingQuality scaleQuality)
        {
            RegionOfInterest = regionOfInterest;
            TargetFrameSize = targetFrameSize;
            TargetFormat = targetFormat;
            ScaleQuality = scaleQuality;
            ScalePolicy = scalePolicy;
        }

        protected bool Equals(TransformParameters other)
        {
            return RegionOfInterest.Equals(other.RegionOfInterest) &&
                   TargetFrameSize.Equals(other.TargetFrameSize) &&
                   TargetFormat == other.TargetFormat && ScaleQuality == other.ScaleQuality;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TransformParameters) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RegionOfInterest.GetHashCode();
                hashCode = (hashCode * 397) ^ TargetFrameSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) TargetFormat;
                hashCode = (hashCode * 397) ^ (int) ScaleQuality;
                return hashCode;
            }
        }
    }
}
