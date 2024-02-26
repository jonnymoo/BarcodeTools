using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using ZXing;
using ZXing.ImageSharp;
using ZXing.Rendering;

namespace JonnyMuir.Function
{
    public class GenerateQRCodeAsHex
    {
        private readonly ILogger<GenerateQRCodeAsHex> _logger;

        public GenerateQRCodeAsHex(ILogger<GenerateQRCodeAsHex> logger)
        {
            _logger = logger;
        }

        [Function("GenerateQRCodeAsRTF")]
        public IActionResult GenerateQRCodeAsRTF([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string? inputString = req.Query["text"];

            if (string.IsNullOrEmpty(inputString))
            {
                return new BadRequestObjectResult("Please provide a text value in the 'text' query parameter.");
            }

            try
            {
                // Generate QR code image using ZXing
                var qrWriter = new ZXing.ImageSharp.BarcodeWriter<Rgba32>();
                qrWriter.Format = BarcodeFormat.QR_CODE;
                qrWriter.Options.Margin = 0;

                var qrCode = qrWriter.Write(inputString);
                using var ms = new MemoryStream();
                qrCode.SaveAsPng(ms);
                StringBuilder sb = new();
                var bytes = ms.ToArray();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                var imageData = sb.ToString();

                return new OkObjectResult($"{{\\pict\\pngblip0\n{imageData}\n}}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return new StatusCodeResult(500);
            }
        }

        [Function("GenerateQRCodeAsDataURL")]
        public IActionResult GenerateQRCodeAsDataURL([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string? inputString = req.Query["text"];

            if (string.IsNullOrEmpty(inputString))
            {
                return new BadRequestObjectResult("Please provide a text value in the 'text' query parameter.");
            }

            try
            {
                // Generate QR code image using ZXing
                var qrWriter = new ZXing.ImageSharp.BarcodeWriter<Rgba32>();
                qrWriter.Format = BarcodeFormat.QR_CODE;
                qrWriter.Options.Margin = 0;

                var qrCode = qrWriter.Write(inputString);
                return new OkObjectResult(qrCode.ToBase64String(PngFormat.Instance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return new StatusCodeResult(500);
            }
        }
    }
}
