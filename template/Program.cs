using System;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using SkiaSharp;
using System.Runtime.InteropServices;
namespace template
{
    public class FunctionTest
    {
        public static void Main()
        {
            try
            {
                FFmpegLoader.FFmpegPath = "/home/perugo/dotnet_project/template/ffmpeg_libs";
                var settings = new VideoEncoderSettings(width: 640, height: 480, framerate: 30, codec: VideoCodec.MPEG);
                using var videoFile = MediaBuilder.CreateContainer("/home/perugo/dotnet_project/template/output.mp4").WithVideo(settings).Create();
                using var bitmap = new SKBitmap(640, 480);
                using var canvas = new SKCanvas(bitmap);
                for (int i = 0; i < 300; i++)
                {
                    canvas.Clear(SKColors.White);
                    using var paint = new SKPaint { Color = SKColors.Black };
                    canvas.DrawRect(i * 10 % 640, 0, 100, 480, paint);
                    var pixels = bitmap.GetPixelSpan().ToArray();
                    var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                    var pointer = handle.AddrOfPinnedObject();
                    var frame = ImageData.FromPointer(pointer, ImagePixelFormat.Bgra32, new System.Drawing.Size(bitmap.Width, bitmap.Height));
                    videoFile.Video.AddFrame(frame);
                    handle.Free();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
                return;
            }
        }
    }
}