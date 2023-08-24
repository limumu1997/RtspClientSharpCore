﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using RtspClientSharpCore;
using RtspClientSharpCore.RawFrames;
using SkiaSharp;

namespace TestRtspClient
{
    class Program
    {
        // TODO: Change to your values
        //private const string urlToCamera = "rtsp://192.168.2.30:8554/live";
        private const string urlToCamera = "rtsp://192.168.3.110/h264";

        //    private const string urlToCamera = "rtsp://MirrorBoy:123454321@192.168.0.189/h264Preview_01_main";
        private const string pathToSaveImage = @"/home/villain/Pictures/images";
        private const int streamWidth = 2560;//240;
        private const int streamHeight = 1440;//160;

        //public static event EventHandler<IDecodedVideoFrame> FrameReceived;
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private static int _imageNumber = 1;
        private static readonly FrameDecoder FrameDecoder = new FrameDecoder();
        private static readonly FrameTransformer FrameTransformer = new FrameTransformer(streamWidth, streamHeight);


        static void Main()
        {
            Console.WriteLine($"Platform {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}");

            var serverUri = new Uri(urlToCamera);
            //var credentials = new NetworkCredential("admin", "admin12345678");

            var connectionParameters = new ConnectionParameters(serverUri/*, credentials*/);

            SaveManyPicture(connectionParameters);
            //SaveOnePicture(connectionParameters).Wait();
        }

        private static void SaveManyPicture(ConnectionParameters connectionParameters)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var connectTask = ConnectAsync(connectionParameters, cancellationTokenSource.Token);

            Console.WriteLine("Press any key to cancel");
            Console.ReadLine();

            cancellationTokenSource.Cancel();

            Console.WriteLine("Canceling");
            connectTask.Wait(CancellationToken.None);
        }

        private static async Task ConnectAsync(ConnectionParameters connectionParameters, CancellationToken token)
        {
            try
            {
                var delay = TimeSpan.FromSeconds(5);

                using var rtspClient = new RtspClient(connectionParameters);
                rtspClient.FrameReceived += RtspClient_FrameReceived;

                while (true)
                {
                    try
                    {
                        Console.WriteLine("Connecting...");
                        await rtspClient.ConnectAsync(token);
                        Console.WriteLine("Connected.");
                        await rtspClient.ReceiveAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (RtspClientSharpCore.Rtsp.RtspClientException e)
                    {
                        Console.WriteLine(e.ToString());
                        await Task.Delay(delay, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static void RtspClient_FrameReceived(object sender, RawFrame rawFrame)
        {
            if (rawFrame is not RtspClientSharpCore.RawFrames.Video.RawVideoFrame rawVideoFrame)
                return;

            var decodedFrame = FrameDecoder.TryDecode(rawVideoFrame);

            if (decodedFrame == null)
                return;

            var bitmap = FrameTransformer.TransformToBitmap(decodedFrame);

            var fileName = $"image{_imageNumber++}.jpg";

            var frameType = rawFrame is RtspClientSharpCore.RawFrames.Video.RawH264IFrame ? "IFrame" : "PFrame";
            Console.WriteLine($"Frame was successfully decoded! {frameType} Trying to save to JPG file {fileName}...");

            try
            {
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode();
                if (IsWindows)
                {
                    var path = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "rtsp"));
                    using var stream = System.IO.File.OpenWrite(Path.Combine(path.FullName, fileName));
                    data.SaveTo(stream);
                    return;
                }

                if (!IsLinux) throw new PlatformNotSupportedException("Not supported OS platform!!");
                // Change to your path
                var linuxPath = Path.Combine(pathToSaveImage, fileName);
                using var linuxPathStream = System.IO.File.OpenWrite(linuxPath);
                data.SaveTo(linuxPathStream);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error saving to file: {e.Message}");
                Debug.WriteLine($"Error saving to file: {e.Message}");
                Debug.WriteLine($"Stack trace: {e.StackTrace}");
            }
        }

    }
}
