using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;

namespace Laser.Activation.Web.Services;

public interface ICaptchaService
{
    (string captchaId, string base64Image) GenerateCaptcha();
    bool Validate(string captchaId, string code);
}

public class CaptchaService : ICaptchaService
{
    private readonly IMemoryCache _cache;
    private static readonly Random _random = new();
    private static readonly char[] _chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public CaptchaService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public (string captchaId, string base64Image) GenerateCaptcha()
    {
        var code = GenerateCode(4);
        var captchaId = Guid.NewGuid().ToString("N");

        _cache.Set($"captcha:{captchaId}", code, TimeSpan.FromMinutes(5));

        var imageBytes = DrawCaptcha(code);
        var base64 = Convert.ToBase64String(imageBytes);

        return (captchaId, $"data:image/png;base64,{base64}");
    }

    public bool Validate(string captchaId, string code)
    {
        if (string.IsNullOrEmpty(captchaId) || string.IsNullOrEmpty(code))
            return false;

        var key = $"captcha:{captchaId}";
        if (_cache.TryGetValue(key, out string? storedCode))
        {
            _cache.Remove(key);
            return string.Equals(storedCode, code, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private static string GenerateCode(int length)
    {
        lock (_random)
        {
            return new string(Enumerable.Range(0, length)
                .Select(_ => _chars[_random.Next(_chars.Length)]).ToArray());
        }
    }

    private static byte[] DrawCaptcha(string code)
    {
        const int width = 130;
        const int height = 44;

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.WhiteSmoke);

        using var linePaint = new SKPaint { Color = new SKColor(190, 190, 190), StrokeWidth = 1, IsAntialias = true };
        for (int i = 0; i < 6; i++)
        {
            lock (_random)
            {
                canvas.DrawLine(
                    _random.Next(width), _random.Next(height),
                    _random.Next(width), _random.Next(height), linePaint);
            }
        }

        var colors = new[]
        {
            SKColors.DarkSlateBlue,
            SKColors.DarkRed,
            SKColors.DarkGreen,
            SKColors.DarkOrange,
            SKColors.Brown
        };

        float x = 8;
        foreach (char c in code)
        {
            using var font = new SKFont
            {
                Size = 26 + RandomOffset(4),
                Typeface = SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };
            using var textPaint = new SKPaint
            {
                IsAntialias = true,
                Color = colors[RandomIndex(colors.Length)]
            };

            canvas.Save();
            float y = 30 + RandomOffset(6);
            canvas.Translate(x, y);
            canvas.RotateDegrees(RandomOffset(20));
            canvas.DrawText(c.ToString(), 0, 0, font, textPaint);
            canvas.Restore();
            x += 28;
        }

        using var dotPaint = new SKPaint { Color = new SKColor(170, 170, 170) };
        for (int i = 0; i < 40; i++)
        {
            lock (_random)
            {
                canvas.DrawCircle(_random.Next(width), _random.Next(height), 1, dotPaint);
            }
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static float RandomOffset(int range)
    {
        lock (_random) { return _random.Next(-range, range + 1); }
    }

    private static int RandomIndex(int length)
    {
        lock (_random) { return _random.Next(length); }
    }
}
