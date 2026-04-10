using System.Security.Cryptography;
using System.Text;

namespace Laser.Activation.Web.Services;

public interface IActivationService
{
    string GenerateActivationCode(string identificationCode);
    bool ValidateActivationCode(string identificationCode, string activationCode);
    (string body, string error) ParseV2Input(string input);
    string FormatV2Output(string activationCode);
}

public class EcdsaActivationService : IActivationService
{
    private readonly ECDsa _ecdsa;
    private readonly ILogger<EcdsaActivationService> _logger;

    public EcdsaActivationService(ILogger<EcdsaActivationService> logger)
    {
        _logger = logger;
        _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    }

    public string GenerateActivationCode(string identificationCode)
    {
        try
        {
            var dataBytes = Encoding.UTF8.GetBytes(identificationCode);
            var signature = _ecdsa.SignData(dataBytes, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(signature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate activation code");
            throw;
        }
    }

    public bool ValidateActivationCode(string identificationCode, string activationCode)
    {
        try
        {
            var dataBytes = Encoding.UTF8.GetBytes(identificationCode);
            var signatureBytes = Convert.FromBase64String(activationCode);
            return _ecdsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate activation code");
            return false;
        }
    }

    public (string body, string error) ParseV2Input(string input)
    {
        var lastCommaIndex = input.LastIndexOf(',');
        if (lastCommaIndex < 0 || lastCommaIndex >= input.Length - 1)
        {
            return (string.Empty, "V2格式无效：缺少SHA1校验和（格式应为 {正文},{SHA1}）");
        }

        var body = input.Substring(0, lastCommaIndex);
        var providedSha1 = input.Substring(lastCommaIndex + 1).Trim();

        var computedSha1 = ComputeSha1Hex(body);

        if (!string.Equals(providedSha1, computedSha1, StringComparison.OrdinalIgnoreCase))
        {
            return (string.Empty, $"SHA1校验失败：期望 {computedSha1}，实际 {providedSha1}");
        }

        return (body, string.Empty);
    }

    public string FormatV2Output(string activationCode)
    {
        var sha1 = ComputeSha1Hex(activationCode);
        return $"{activationCode},{sha1}";
    }

    private static string ComputeSha1Hex(string text)
    {
        using var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
