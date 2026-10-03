using System.Globalization;
using System.Text.Json;

namespace Xeneon.Runtime.Core;

// Read only the watcher's documented event output. This confirms emission, not audio playback.
public static class OrionEventParser
{
    public static GameLogEvent? Parse(string line, long sequence, int generation, bool historical)
        => CompanionEventParser.Parse(line, sequence, generation, historical, "Orion");
}

public static class CompanionEventParser
{
    public static GameLogEvent? Parse(string line, long sequence, int generation, bool historical, string companion)
    {
        if (companion is not ("Orion" or "Aurora")) throw new ArgumentException("Unbekannte Begleiter-Variante.");
        try
        {
            using var document = JsonDocument.Parse(line, new JsonDocumentOptions { MaxDepth = 8 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("schemaVersion", out var schema)
                || schema.ValueKind != JsonValueKind.Number || !schema.TryGetInt32(out int version) || version != 1
                || !root.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String
                || !root.TryGetProperty("timestampUtc", out var timestamp) || timestamp.ValueKind != JsonValueKind.String
                || !DateTimeOffset.TryParse(timestamp.GetString(), CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var time)) return null;
            string kind = type.GetString()!;
            var (category, message) = kind switch
            {
                "ship_identified" => ("SHIP", "Schiff erkannt"),
                "blueprint_received" => ("PLAYER", "Bauplan erhalten"),
                "player_death" => ("COMBAT", "Spielertod erkannt"),
                "server_error_30000" => ("ERROR", "Serverfehler 30000 erkannt"),
                "safety_zone_entered" => ("LOCATION", "Sicherheitszone betreten"),
                "safety_zone_left" => ("LOCATION", "Sicherheitszone verlassen"),
                "restricted_zone_entered" => ("LOCATION", "Sperrzone betreten"),
                "restricted_zone_left" => ("LOCATION", "Sperrzone verlassen"),
                "monitored_space_entered" => ("LOCATION", "Überwachten Raum betreten"),
                "monitored_space_left" => ("LOCATION", "Überwachten Raum verlassen"),
                _ => ("SYSTEM", "")
            };
            if (message.Length == 0) return null;
            if (kind is "ship_identified" or "blueprint_received"
                && root.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
                message += ": " + GameLogParser.Parse(name.GetString()!, sequence, generation, historical).Message;
            return new(sequence, generation, time, "Notice", category, companion.ToUpperInvariant() + " · " + kind, message, historical, true);
        }
        catch (JsonException) { return null; }
    }
}
