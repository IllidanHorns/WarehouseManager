using System.Windows;
using System.Windows.Media;
using ModernWpf;

namespace WarehouseManager.Wpf.Helpers;

public static class ThemeResourceHelper
{
    private record ThemePalette(string Surface, string Panel, string Card, string PrimaryText,
        string SecondaryText, string TableText, string TableHeaderText, string AccentPrimary, string AccentSecondary,
        string AccentHover, string Border, string Muted);

    private static readonly Dictionary<ApplicationTheme, ThemePalette> Palettes = new()
    {
        {
            ApplicationTheme.Light,
            new ThemePalette(
                Surface: "#FFF4D4C4",
                Panel: "#FFEEE2DC",
                Card: "#FFEDC7B7",
                PrimaryText: "#FF123C69",
                SecondaryText: "#FFEEE2DC",
                TableText: "#FF123C69",
                TableHeaderText: "#FF123C69",
                AccentPrimary: "#FF123C69",
                AccentSecondary: "#FFAC3B61",
                AccentHover: "#FFC42D4D",
                Border: "#FFBAB2B5",
                Muted: "#FFD8C3A5")
        },
        {
            ApplicationTheme.Dark,
            new ThemePalette(
                Surface: "#FF1F1F1F",
                Panel: "#FF252525",
                Card: "#FF303030",
                PrimaryText: "#FFF4F4F4",
                SecondaryText: "#FFE0E0E0",
                TableText: "#FFFFFFFF",
                TableHeaderText: "#FFE0E0E0",
                AccentPrimary: "#FF8AB4F8",
                AccentSecondary: "#FFFF6B81",
                AccentHover: "#FFFF8FA4",
                Border: "#FF4A4A4A",
                Muted: "#FF3A3A3A")
        }
    };

    public static void ApplyTheme(ApplicationTheme theme)
    {
        if (!Palettes.TryGetValue(theme, out var palette))
        {
            palette = Palettes[ApplicationTheme.Light];
        }

        var resources = System.Windows.Application.Current.Resources;

        resources["SurfaceBrush"] = CreateBrush(palette.Surface);
        resources["PanelBrush"] = CreateBrush(palette.Panel);
        resources["CardBrush"] = CreateBrush(palette.Card);
        resources["PrimaryTextBrush"] = CreateBrush(palette.PrimaryText);
        resources["SecondaryTextBrush"] = CreateBrush(palette.SecondaryText);
        resources["AccentBrush"] = CreateBrush(palette.AccentPrimary);
        resources["AccentSecondaryBrush"] = CreateBrush(palette.AccentSecondary);
        resources["AccentHoverBrush"] = CreateBrush(palette.AccentHover);
        resources["BorderBrushBase"] = CreateBrush(palette.Border);
        resources["MutedBrush"] = CreateBrush(palette.Muted);
        resources["TableTextBrush"] = CreateBrush(palette.TableText);
        resources["TableHeaderTextBrush"] = CreateBrush(palette.TableHeaderText);
    }

    private static SolidColorBrush CreateBrush(string colorHex)
    {
        var color = (Color)ColorConverter.ConvertFromString(colorHex)!;
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}

