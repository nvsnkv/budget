namespace NVs.Budget.Controllers.Web.Models;

public class ValidationRuleResponse
{
    public string Pattern { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class FileReadingSettingResponse
{
    public string Culture { get; set; } = string.Empty;
    public string Encoding { get; set; } = string.Empty;
    public string DateTimeKind { get; set; } = string.Empty;
    public Dictionary<string, string> Fields { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();
    public List<ValidationRuleResponse> Validation { get; set; } = new();
}

