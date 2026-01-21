using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FluentResults;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;

namespace NVs.Budget.Controllers.Web.Utils;

public class FileReadingSettingsMapper
{
    public Dictionary<string, FileReadingSettingResponse> ToResponse(IReadOnlyDictionary<Regex, FileReadingSetting> settings)
    {
        var responseSettings = new Dictionary<string, FileReadingSettingResponse>();

        foreach (var kvp in settings)
        {
            var pattern = kvp.Key.ToString();
            var setting = kvp.Value;

            var validationRules = setting.Validation
                .Select(v => new ValidationRuleResponse
                {
                    Pattern = v.Pattern,
                    Condition = v.Condition.ToString(),
                    Value = v.Value,
                    ErrorMessage = v.ErrorMessage
                })
                .ToList();

            var response = new FileReadingSettingResponse
            {
                Culture = setting.Culture.Name,
                Encoding = setting.Encoding.WebName,
                DateTimeKind = setting.DateTimeKind.ToString(),
                Fields = new Dictionary<string, string>(setting.Fields),
                Attributes = new Dictionary<string, string>(setting.Attributes),
                Validation = validationRules
            };

            responseSettings[pattern] = response;
        }

        return responseSettings;
    }

    public Result<IReadOnlyDictionary<Regex, FileReadingSetting>> FromRequest(Dictionary<string, FileReadingSettingResponse> request)
    {
        var settings = new Dictionary<Regex, FileReadingSetting>();
        var errors = new List<IError>();

        foreach (var kvp in request)
        {
            var patternString = kvp.Key;
            var settingResponse = kvp.Value;

            // Parse regex pattern
            Regex regex;
            try
            {
                regex = new Regex(patternString);
            }
            catch (ArgumentException ex)
            {
                errors.Add(new Error($"Invalid regex pattern '{patternString}': {ex.Message}"));
                continue;
            }

            // Parse culture
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(settingResponse.Culture);
            }
            catch (CultureNotFoundException ex)
            {
                errors.Add(new Error($"Invalid culture '{settingResponse.Culture}': {ex.Message}"));
                continue;
            }

            // Parse encoding
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(settingResponse.Encoding);
            }
            catch (ArgumentException ex)
            {
                errors.Add(new Error($"Invalid encoding '{settingResponse.Encoding}': {ex.Message}"));
                continue;
            }

            // Parse DateTimeKind
            if (!Enum.TryParse<DateTimeKind>(settingResponse.DateTimeKind, out var dateTimeKind))
            {
                errors.Add(new Error($"Invalid DateTimeKind '{settingResponse.DateTimeKind}'. Must be one of: Local, Utc, Unspecified"));
                continue;
            }

            // Parse validation rules
            var validationRules = new List<ValidationRule>();
            foreach (var vrResponse in settingResponse.Validation)
            {
                if (!Enum.TryParse<ValidationRule.ValidationCondition>(vrResponse.Condition, out var condition))
                {
                    errors.Add(new Error($"Invalid ValidationCondition '{vrResponse.Condition}'. Must be one of: Equals, NotEquals"));
                    continue;
                }

                validationRules.Add(new ValidationRule(
                    vrResponse.Pattern,
                    condition,
                    vrResponse.Value,
                    vrResponse.ErrorMessage
                ));
            }

            var setting = new FileReadingSetting(
                culture,
                encoding,
                dateTimeKind,
                settingResponse.Fields,
                settingResponse.Attributes,
                validationRules
            );

            settings[regex] = setting;
        }

        if (errors.Any())
        {
            return Result.Fail(errors);
        }

        return Result.Ok<IReadOnlyDictionary<Regex, FileReadingSetting>>(settings);
    }
}

