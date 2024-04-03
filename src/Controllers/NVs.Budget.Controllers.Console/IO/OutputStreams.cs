namespace NVs.Budget.Controllers.Console.IO;

internal record OutputStreams(TextWriter Out, TextWriter Error);

internal record OutputOptions(bool ShowSuccesses = false);
