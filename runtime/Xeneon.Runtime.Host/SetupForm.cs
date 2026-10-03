using System.Text.Json;
using Xeneon.Runtime.Core;
using Xeneon.Runtime.Windows;

namespace Xeneon.Runtime.Host;

internal sealed class SetupForm : Form
{
    internal static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "StarCitizenCompanionDeck", "config.json");
    private readonly ComboBox variant = new() { Name = "variant", DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly TextBox gamePath = new() { Name = "gamePath", Dock = DockStyle.Fill };
    private readonly TextBox eventsPath = new() { Name = "eventsPath", Dock = DockStyle.Fill };
    private readonly string storagePath;
    private readonly RuntimeConfig? existing;
    internal RuntimeConfig? Result { get; private set; }

    internal SetupForm(RuntimeConfig? existing = null, string? storagePath = null)
    {
        this.existing = existing; this.storagePath = storagePath ?? ConfigPath;
        Text = "Begleiter-Deck einrichten"; ClientSize = new(760, 440);
        AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new(700, 400);
        StartPosition = FormStartPosition.CenterScreen;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new(24), ColumnCount = 3, RowCount = 7 };
        layout.ColumnStyles.Add(new(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new(SizeType.Absolute, 115));
        Controls.Add(layout);
        var intro = new Label { Text = "Orion und Aurora sind externe Tools von Aurora Systems.\nDas Deck integriert den separat gestarteten Begleiter. XENEON EDGE in iCUE auf Desktop stellen.", AutoSize = true, MaximumSize = new(700, 0), Margin = new(0, 0, 0, 20) };
        layout.Controls.Add(intro, 0, 0); layout.SetColumnSpan(intro, 3);
        variant.Items.AddRange(["Orion", "Aurora"]); variant.SelectedItem = existing?.CompanionVariant ?? "Orion";
        layout.Controls.Add(new Label { Text = "Begleiter", AutoSize = true }, 0, 1); layout.Controls.Add(variant, 1, 1);
        AddPath(layout, 2, "Game.log", gamePath, "Game.log|Game.log");
        AddPath(layout, 3, "Ereignisdatei", eventsPath, "Begleiter-Ereignisse|*-Companion-Events.jsonl");
        gamePath.Text = existing?.GameLogPath ?? "";
        eventsPath.Text = existing?.CompanionEventsPath ?? existing?.OrionEventsPath ?? DefaultEvents((string)variant.SelectedItem!);
        variant.SelectedIndexChanged += (_, _) => eventsPath.Text = DefaultEvents((string)variant.SelectedItem!);
        var note = new Label { AutoSize = true, MaximumSize = new(680, 0), Margin = new(0, 16, 0, 16),
            Text = "Game.log optional: nur für ‚Weitere Logs‘. Wähle die Datei aus deiner eigenen Star-Citizen-Installation – jedes Laufwerk ist möglich. Die Ereignisdatei darf beim ersten Start noch fehlen." };
        layout.Controls.Add(note, 0, 4); layout.SetColumnSpan(note, 3);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var save = new Button { Text = "Speichern und starten", AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel };
        var search = new Button { Text = "Game.log suchen", AutoSize = true };
        var searchState = new Label { AutoSize = true, MaximumSize = new(520, 0), Text = "Typische LIVE-Installationen auf lokalen Laufwerken suchen." };
        var searchRow = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        searchRow.Controls.Add(search); searchRow.Controls.Add(searchState);
        layout.Controls.Add(searchRow, 0, 5); layout.SetColumnSpan(searchRow, 3);
        async Task Search()
        {
            string initial = gamePath.Text; search.Enabled = false;
            searchState.Text = "Suche nach Game.log…";
            try
            {
                var candidates = await Task.Run(GameLogDiscovery.FindLocal);
                if (IsDisposed || Disposing) return;
                if (gamePath.Text != initial) { searchState.Text = "Deine manuelle Auswahl bleibt erhalten."; return; }
                if (candidates.Count == 1)
                { gamePath.Text = candidates[0]; searchState.Text = "Game.log gefunden. Bitte den vorgeschlagenen Pfad prüfen."; }
                else if (candidates.Count == 0)
                    searchState.Text = "Kein Treffer in typischen Ordnern. Bitte ‚Auswählen…‘ verwenden.";
                else
                {
                    using var picker = new Form { Text = "Game.log auswählen", ClientSize = new(780, 240), StartPosition = FormStartPosition.CenterParent };
                    var list = new ListBox { Dock = DockStyle.Fill }; list.Items.AddRange(candidates.Cast<object>().ToArray());
                    var accept = new Button { Dock = DockStyle.Bottom, Height = 40, Text = "Ausgewählten Pfad übernehmen" };
                    picker.Controls.Add(list); picker.Controls.Add(accept);
                    accept.Click += (_, _) => { if (list.SelectedItem is string selected) { gamePath.Text = selected; picker.DialogResult = DialogResult.OK; } };
                    searchState.Text = "Mehrere Installationen gefunden. Bitte auswählen.";
                    picker.ShowDialog(this);
                }
            }
            catch (IOException) { if (!IsDisposed) searchState.Text = "Suche nicht verfügbar. Bitte Datei manuell auswählen."; }
            finally { if (!IsDisposed) search.Enabled = true; }
        }
        search.Click += async (_, _) => await Search();
        Shown += async (_, _) => { if (string.IsNullOrWhiteSpace(gamePath.Text)) await Search(); };
        buttons.Controls.Add(save); buttons.Controls.Add(cancel); layout.Controls.Add(buttons, 0, 6); layout.SetColumnSpan(buttons, 3);
        AcceptButton = save; CancelButton = cancel;
        save.Click += (_, _) =>
        {
            try
            {
                SaveConfiguration(); DialogResult = DialogResult.OK; Close();
            }
            catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException)
            { MessageBox.Show(this, e.Message, "Einrichtung prüfen", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
    }

    internal void SaveConfiguration()
    {
        string name = (string)variant.SelectedItem!;
        string? game = string.IsNullOrWhiteSpace(gamePath.Text) ? null : gamePath.Text.Trim();
        if (game is not null && !File.Exists(game)) throw new ArgumentException("Die gewählte Game.log existiert nicht.");
        var config = (existing ?? new RuntimeConfig()) with {
            Mode = existing?.Mode ?? WindowMode.Fullscreen, CompanionVariant = name,
            GameLogPath = game, CompanionEventsPath = eventsPath.Text.Trim(), OrionEventsPath = null,
            ExternalWindow = new(name + " Log-Wächter", Title: name == "Aurora" ? "Aurora Orb" : "Orion Companion"),
            ExternalPlacement = ExternalPlacementMode.CenterInBay };
        config.Validate(); Directory.CreateDirectory(Path.GetDirectoryName(storagePath)!);
        string pending = storagePath + ".tmp";
        File.WriteAllText(pending, JsonSerializer.Serialize(config, RuntimeConfig.Json));
        File.Move(pending, storagePath, true); Result = config;
    }

    private static string DefaultEvents(string name) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "VoiceAttack", name + " Log-Wächter", name + "-Companion-Events.jsonl");
    private static void AddPath(TableLayoutPanel layout, int row, string label, TextBox field, string filter)
    {
        layout.Controls.Add(new Label { Text = label, AutoSize = true }, 0, row); layout.Controls.Add(field, 1, row);
        var browse = new Button { Text = "Auswählen…", Dock = DockStyle.Fill };
        layout.Controls.Add(browse, 2, row);
        browse.Click += (_, _) => { using var picker = new OpenFileDialog { Filter = filter, CheckFileExists = true };
            if (picker.ShowDialog() == DialogResult.OK) field.Text = picker.FileName; };
    }
}
