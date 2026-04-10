namespace Laser.Activation.Web.Models;

public class ActivateRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;
    public string VersionInf { get; set; } = string.Empty;
}
