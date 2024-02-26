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
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

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
                var imageData = Convert.ToBase64String(ms.ToArray());
                // Convert Base64 string to hex string
                string hexString = BitConverter.ToString(Encoding.UTF8.GetBytes(imageData)).Replace("-", "");

                return new OkObjectResult($"{{\\pict\\pngblip0\n{hexString}\n}}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return new StatusCodeResult(500);
            }
        }
    }
}
