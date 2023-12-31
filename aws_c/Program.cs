﻿using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using SkiaSharp;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace aws_c
{
public class InputData
{
    public string log { get; set; }=String.Empty;
    public string Url { get; set; }=String.Empty;
    public string THIS_IS_A_RETURN_OF_ERROR { get; set; }=String.Empty;
}

/*
public class LambdaEvent
{
    public InputData? input { get; set; }=null;
}
*/
public class FunctionTest
{
    private readonly IAmazonS3 _s3Client;
    private readonly JsonSerializerOptions _serializerOptions;
    private static bool IsFFmpegInitialized = false;
    public FunctionTest()
    {
        _s3Client = new AmazonS3Client();
        _serializerOptions = new JsonSerializerOptions { WriteIndented = true }; // For pretty-printed JSON

    }

    public async Task<string> FunctionHandler(InputData lambdaEvent, ILambdaContext context)
    {

        string eventJson = JsonSerializer.Serialize(lambdaEvent, _serializerOptions);
        Console.WriteLine($"Received event: {eventJson}");
        if (string.IsNullOrEmpty(lambdaEvent.Url))
        {
            throw new Exception("Url not provided in the input");
        }

        string directory = lambdaEvent.Url;
            string result="";
            string outputFilePath = Path.Combine(Path.GetTempPath(), "output.avi");

            try
            {

                CreateAndSaveVideo(outputFilePath);
                await UploadFileToS3BucketAsync(outputFilePath, "aws-simplebucket",directory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
                result += $" - Error: {e.Message}";
            }

            return directory;
        }

        private void CreateAndSaveVideo(string outputFilePath)
        {
            if (!IsFFmpegInitialized)
            {
                FFmpegLoader.FFmpegPath = "/var/task/lib";  // Path in Lambda function environment
                IsFFmpegInitialized = true;
            }
            var settings = new VideoEncoderSettings(width: 640, height: 480, framerate: 30, codec: VideoCodec.MPEG);

            using var videoFile = MediaBuilder.CreateContainer(outputFilePath).WithVideo(settings).Create();
            using var bitmap = new SKBitmap(640, 480);
            using var canvas = new SKCanvas(bitmap);

            for (int i = 0; i < 10; i++)
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

        private async Task UploadFileToS3BucketAsync(string filePath, string bucketName,string dir)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                var key = $"{dir}/output.avi";
                await fileTransferUtility.UploadAsync(filePath, bucketName,key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
                throw; // Re-throw the exception so it's captured by the caller.
            }
        }
    }
}



/*
using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using SkiaSharp;
using System.Runtime.InteropServices;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace aws_c
{
    public class MyEvent
    {
        public string Event { get; set; } = string.Empty;
    }

    public class FunctionTest
    {
        private readonly IAmazonS3 _s3Client;

        public FunctionTest()
        {
            _s3Client = new AmazonS3Client();
        }

        public string FunctionHandler(MyEvent myEvent, ILambdaContext context)
        {
            string result = myEvent.Event.ToUpper();
            try
            {

                FFmpegLoader.FFmpegPath = "/var/task/lib";  // Path in Lambda function environment
                var settings = new VideoEncoderSettings(width: 640, height: 480, framerate: 30, codec: VideoCodec.MPEG);
                string outputFilePath = Path.Combine(Path.GetTempPath(), "output.avi");
                using var videoFile = MediaBuilder.CreateContainer(outputFilePath).WithVideo(settings).Create();
                using var bitmap = new SKBitmap(640, 480);
                using var canvas = new SKCanvas(bitmap);
                for (int i = 0; i < 10; i++)
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
                UploadFileToS3BucketAsync(outputFilePath, "aws-simplebucket").Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
            }
            return result;
        }
        private async Task UploadFileToS3BucketAsync(string filePath, string bucketName)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(filePath, bucketName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
            }
        }
    }
}
*/