using System;
using System.Runtime.InteropServices;
using FrameDecoderCore;
using FrameDecoderCore.DecodedFrames;
using SkiaSharp;

namespace TestRtspClient
{
    public class FrameTransformer
    {
        private readonly SKSizeI _pictureSize;

        public FrameTransformer(int pictureWidth, int pictureHeight)
        {
            _pictureSize = new SKSizeI(pictureWidth, pictureHeight);
        }

        public SKBitmap TransformToBitmap(IDecodedVideoFrame decodedFrame)
        {
            var managedArray = TransformFrame(decodedFrame, _pictureSize);
            var im = CopyDataToBitmap(managedArray, _pictureSize);
            return im;
        }

        private static byte[] TransformFrame(IDecodedVideoFrame decodedFrame, SKSizeI pictureSize)
        {
            var transformParameters = new TransformParameters(
                SKRectI.Empty,
                pictureSize,
                ScalingPolicy.Stretch, PixelFormat.Bgra32, ScalingQuality.FastBilinear);

            var pictureArraySize = pictureSize.Width * pictureSize.Height * 4;
            var unmanagedPointer = Marshal.AllocHGlobal(pictureArraySize);

            decodedFrame.TransformTo(unmanagedPointer, pictureSize.Width * 4, transformParameters);
            var managedArray = new byte[pictureArraySize];
            Marshal.Copy(unmanagedPointer, managedArray, 0, pictureArraySize);
            Marshal.FreeHGlobal(unmanagedPointer);
            return managedArray;
        } 

        private static SKBitmap CopyDataToBitmap(byte[] data, SKSizeI pictureSize)
        {
            var bmp = new SKBitmap(pictureSize.Width, pictureSize.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            IntPtr dstPixels = bmp.GetPixels();
            Marshal.Copy(data, 0, dstPixels, data.Length);
            return bmp;
        }

    }
}