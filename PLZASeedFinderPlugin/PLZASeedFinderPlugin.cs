using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PKHeX.Core;
using PLZASeedFinderPlugin.Helpers;

namespace PLZASeedFinderPlugin;

/// <summary>
/// PKHeX Plugin for finding Pokémon Legends: Z-A (PA9) seeds that match specific criteria
/// </summary>
public sealed class PLZASeedFinderPlugin : IPlugin
{
    public string Name => "PLZA Seed Finder";
    public int Priority => 1;

    // Initialized on plugin load
    public ISaveFileProvider SaveFileEditor { get; private set; } = null!;
    public IPKMView PKMEditor { get; private set; } = null!;
    private ToolStripMenuItem? PluginMenuItem { get; set; }

    public void Initialize(params object[] args)
    {
        Console.WriteLine($"Loading {Name}...");

        // Enable SearchShiny1 globally for Shiny Alpha Pokemon seed correlation
        // This is required for PKHeX to validate Shiny Alpha Pokemon (RollCount=1)
        // Without this, sysbot and other tools using PKHeX will reject them as invalid
        LumioseSolver.SearchShiny1 = true;
        Console.WriteLine($"[{Name}] Enabled LumioseSolver.SearchShiny1 for Shiny Alpha validation");

        // Check version compatibility
        if (PluginVersion.HasVersionMismatch())
            Console.WriteLine($"[{Name}] {PluginVersion.GetCompatibilityMessage()}");

        SaveFileEditor = (ISaveFileProvider)Array.Find(args, z => z is ISaveFileProvider)!;
        PKMEditor = (IPKMView)Array.Find(args, z => z is IPKMView)!;
        var menu = (ToolStrip)Array.Find(args, z => z is ToolStrip)!;
        LoadMenuStrip(menu);
    }

    private void LoadMenuStrip(ToolStrip menuStrip)
    {
        var items = menuStrip.Items;
        if (items.Find("Menu_Tools", false)[0] is not ToolStripDropDownItem tools)
            throw new ArgumentException(null, nameof(menuStrip));
        AddPluginControl(tools);
    }

    private void AddPluginControl(ToolStripDropDownItem tools)
    {
        PluginMenuItem = new ToolStripMenuItem(Name)
        {
            ShortcutKeys = Keys.Control | Keys.Shift | Keys.W,
            Image = CreateCrystalIcon()
        };

        PluginMenuItem.Click += (_, _) => ShowSeedFinderForm();
        tools.DropDownItems.Add(PluginMenuItem);
        Console.WriteLine($"{Name} added menu item.");
    }

    private static Bitmap CreateCrystalIcon()
    {
        var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Create a diamond/crystal shape
            var points = new Point[]
            {
                new(8, 2),
                new(13, 8),
                new(8, 14),
                new(3, 8)
            };

            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, 16, 16),
                Color.FromArgb(200, 30, 144, 255),
                Color.FromArgb(200, 255, 20, 147),
                45f))
            {
                g.FillPolygon(brush, points);
            }

            using (var pen = new Pen(Color.FromArgb(255, 0, 100, 200), 1))
            {
                g.DrawPolygon(pen, points);
            }

            var shinePoints = new Point[]
            {
                new(8, 4),
                new(10, 6),
                new(8, 8),
                new(6, 6)
            };

            using var shineBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
            g.FillPolygon(shineBrush, shinePoints);
        }
        return bmp;
    }

    private void ShowSeedFinderForm()
    {
        try
        {
            using var form = new GUI.Gen9aSeedFinderForm(SaveFileEditor, PKMEditor);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            WinFormsUtil.Error($"Error loading {Name}:", ex.Message);
        }
    }

    public void NotifySaveLoaded()
    {
        Console.WriteLine($"{Name} was notified that a Save File was just loaded.");

        if (PluginMenuItem != null)
        {
            var sav = SaveFileEditor.SAV;
            // Enable only for Legends: Z-A game
            PluginMenuItem.Enabled = sav is SAV9ZA;
        }
    }

    public bool TryLoadFile(string filePath)
    {
        Console.WriteLine($"{Name} was provided with the file path, but chose to do nothing with it.");
        return false; // no action taken
    }
}
