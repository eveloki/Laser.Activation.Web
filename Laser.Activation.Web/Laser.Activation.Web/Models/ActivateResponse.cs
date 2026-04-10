namespace Laser.Activation.Web.Models;

public class ActivateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ActivationCode { get; set; }
    public int? RecordId { get; set; }
}
