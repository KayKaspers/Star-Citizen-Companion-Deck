using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xeneon.Runtime.Core;

public sealed record RelativeRect(double X, double Y, double Width, double Height)
{
    public void Validate()
    {
        if (new[] { X, Y, Width, Height }.Any(v => !double.IsFinite(v)) || X < 0 || Y < 0
            || Width <= 0 || Height <= 0 || X + Width > 1 || Y + Height > 1)
            throw new ArgumentException("Relative rectangle must fit inside [0,1].");
    }
    public PixelRect On(PixelRect bounds)
    {
        Validate();
        int left = (int)Math.Round(X * bounds.Width), top = (int)Math.Round(Y * bounds.Height);
        int right = (int)Math.Round((X + Width) * bounds.Width), bottom = (int)Math.Round((Y + Height) * bounds.Height);
        if (right <= left || bottom <= top) throw new ArgumentException("Rectangle is too small for this display.");
        return new(bounds.X + left, bounds.Y + top, right - left, bottom - top);
    }
}

public sealed record RuntimeConfig
{
    public int SchemaVersion { get; init; } = 1;
    public string? DisplayKey { get; init; }
    public string[] DisplayNames { get; init; } = ["XENEON EDGE", "XENON EDGE"];
    public WindowMode Mode { get; init; } = WindowMode.Windowed;
    public RelativeRect WindowedArea { get; init; } = new(.05, .05, .90, .90);
    public RelativeRect CompanionArea { get; init; } = new(.6625, 0, .3375, 1);
    public ExternalSelector? ExternalWindow { get; init; }
    public ExternalPlacementMode ExternalPlacement { get; init; } = ExternalPlacementMode.ResizeToBay;
    public int RefreshMilliseconds { get; init; } = 2000;
    public string? GameLogPath { get; init; }
    public string? OrionEventsPath { get; init; }
    public string CompanionVariant { get; init; } = "Orion";
    public string? CompanionEventsPath { get; init; }

    public void Validate()
    {
        if (CompanionVariant is not ("Orion" or "Aurora")) throw new ArgumentException("CompanionVariant muss Orion oder Aurora sein.");
        if (CompanionEventsPath is not null && (CompanionEventsPath.StartsWith(@"\\", StringComparison.Ordinal)
            || !Path.IsPathFullyQualified(CompanionEventsPath)
            || !string.Equals(Path.GetFileName(CompanionEventsPath), CompanionVariant + "-Companion-Events.jsonl", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("CompanionEventsPath muss eine absolute lokale Ereignisdatei der gewählten Variante sein.");
        if (CompanionEventsPath is not null && OrionEventsPath is not null)
            throw new ArgumentException("Nur CompanionEventsPath oder OrionEventsPath angeben.");
        if (OrionEventsPath is not null && CompanionVariant != "Orion")
            throw new ArgumentException("OrionEventsPath gehört ausschließlich zur Variante Orion.");
        if (OrionEventsPath is not null && (OrionEventsPath.StartsWith(@"\\", StringComparison.Ordinal)
            || !Path.IsPathFullyQualified(OrionEventsPath)
            || !string.Equals(Path.GetFileName(OrionEventsPath), "Orion-Companion-Events.jsonl", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("OrionEventsPath must be an absolute local Orion-Companion-Events.jsonl path.");
        if (SchemaVersion != 1) throw new ArgumentException("Unsupported configuration schema.");
        if (!Enum.IsDefined(Mode)) throw new ArgumentException("Unknown window mode.");
        if (!Enum.IsDefined(ExternalPlacement)) throw new ArgumentException("Unknown external placement mode.");
        if (GameLogPath is not null && (GameLogPath.StartsWith(@"\\", StringComparison.Ordinal) || !Path.IsPathFullyQualified(GameLogPath) || !string.Equals(Path.GetFileName(GameLogPath), "Game.log", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("GameLogPath must be an absolute Game.log path.");
        if (DisplayNames is null || DisplayNames.Length == 0 || DisplayNames.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Display names are required.");
        if (DisplayKey is not null && string.IsNullOrWhiteSpace(DisplayKey)) throw new ArgumentException("Empty display key.");
        if (RefreshMilliseconds < 500 || RefreshMilliseconds > 60000) throw new ArgumentException("Refresh interval must be 500..60000 ms.");
        if (WindowedArea is null || CompanionArea is null) throw new ArgumentException("Layout rectangles are required.");
        WindowedArea.Validate(); CompanionArea.Validate(); ExternalWindow?.Validate();
    }

    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public static RuntimeConfig Load(string path)
    {
        var config = JsonSerializer.Deserialize<RuntimeConfig>(File.ReadAllText(path), Json)
            ?? throw new ArgumentException("Configuration cannot be null.");
        config.Validate(); return config;
    }
}

public static class DisplaySelection
{
    public static (DisplayInfo? Display, string Code) Select(IReadOnlyList<DisplayInfo> displays, RuntimeConfig config)
    {
        // Resolution is never identity. A missing explicit key must never fall back to another display.
        var matches = displays.Where(d => config.DisplayKey is not null ? d.Key == config.DisplayKey
            : config.DisplayNames.Any(n => string.Equals(n.Trim(), d.Name.Trim(), StringComparison.OrdinalIgnoreCase))).ToArray();
        if (matches.Length == 0) return (null, "DISPLAY_MISSING");
        if (matches.Length != 1) return (null, "DISPLAY_AMBIGUOUS");
        if (matches[0].Cloned) return (null, "DISPLAY_CLONED");
        return (matches[0], "DISPLAY_SELECTED");
    }
}
