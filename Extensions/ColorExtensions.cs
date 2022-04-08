using System.Collections.Immutable;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace HACC.Extensions;

public static class ColorExtensions
{
    private static readonly ImmutableDictionary<ConsoleColor, Color> ConsoleColorToColorMap =
        new Dictionary<ConsoleColor, Color>
        {
            {ConsoleColor.Black, Color.Black},
            {ConsoleColor.DarkBlue, Color.Blue},
            {ConsoleColor.DarkGreen, Color.Green},
            {ConsoleColor.DarkCyan, Color.Cyan},
            {ConsoleColor.DarkRed, Color.Red},
            {ConsoleColor.DarkMagenta, Color.Magenta},
            {ConsoleColor.DarkYellow, Color.Brown},
            {ConsoleColor.DarkGray, Color.DarkGray},
            {ConsoleColor.Gray, Color.Gray},
            {ConsoleColor.Blue, Color.BrightBlue},
            {ConsoleColor.Green, Color.BrightGreen},
            {ConsoleColor.Cyan, Color.BrightCyan},
            {ConsoleColor.Red, Color.BrightRed},
            {ConsoleColor.Magenta, Color.BrightMagenta},
            {ConsoleColor.Yellow, Color.BrightYellow},
            {ConsoleColor.White, Color.White},
        }.ToImmutableDictionary();

    /*private static readonly ImmutableDictionary<ConsoleColor, Spectre.Console.Color> ConsoleColorToSpectreColorMap = (new Dictionary<ConsoleColor, Spectre.Console.Color>(
    {
        {ConsoleColor.Black, Spectre.Console.Color.Black},
        {ConsoleColor.DarkBlue, Spectre.Console.Color.Blue},
        {ConsoleColor.DarkGreen, Spectre.Console.Color.Green},
        {ConsoleColor.DarkCyan, Spectre.Console.Color.Cyan},
        {ConsoleColor.DarkRed, Spectre.Console.Color.Red},
        {ConsoleColor.DarkMagenta, Spectre.Console.Color.Magenta},
        {ConsoleColor.DarkYellow, Spectre.Console.Color.Brown},
        {ConsoleColor.DarkGray, Spectre.Console.Color.DarkGray},
        {ConsoleColor.Gray, Spectre.Console.Color.Gray},
        {ConsoleColor.Blue, Spectre.Console.Color.BrightBlue},
        {ConsoleColor.Green, Spectre.Console.Color.BrightGreen},
        {ConsoleColor.Cyan, Spectre.Console.Color.BrightCyan},
        {ConsoleColor.Red, Spectre.Console.Color.BrightRed},
        {ConsoleColor.Magenta, Spectre.Console.Color.BrightMagenta},
        {ConsoleColor.Yellow, Spectre.Console.Color.BrightYellow},
        {ConsoleColor.White, Spectre.Console.Color.White},
    }).ToImmutableDictionary();*/

    public static Color ToColor(this ConsoleColor consoleColor)
    {
        if (!ConsoleColorToColorMap.ContainsKey(key: consoleColor))
            throw new ArgumentException(message: $"{consoleColor} is not a valid ConsoleColor");

        return ConsoleColorToColorMap[key: consoleColor];
    }

    public static ConsoleColor ToConsoleColor(this Color color)
    {
        if (!ConsoleColorToColorMap.ContainsValue(value: color))
            throw new ArgumentException(message: $"{color} is not a valid Color");

        return ConsoleColorToColorMap.First(predicate: kvp => kvp.Value == color).Key;
    }

    public static Attribute ToAttributeForegroundWithBackground(this Color foreground, Color background)
    {
        return MakeAttribute(
            foreground: foreground,
            background: background);
    }

    public static Attribute ToAttributeBackgroundWithForeground(this Color background, Color foreground)
    {
        return MakeAttribute(
            foreground: foreground,
            background: background);
    }

    public static Attribute ToAttributeForegroundWithBackground(this ConsoleColor foreground, ConsoleColor background)
    {
        return MakeAttribute(
            foreground: foreground.ToColor(),
            background: background.ToColor());
    }

    public static Attribute ToAttributeBackgroundWithForeground(this ConsoleColor background, ConsoleColor foreground)
    {
        return MakeAttribute(
            foreground: foreground.ToColor(),
            background: background.ToColor());
    }

    public static Attribute MakeAttribute(int attribute)
    {
        return new Attribute(
            value: attribute
        );
    }

    public static Attribute MakeAttribute(Color foreground, Color background)
    {
        // Encode the colors into the int value.
        return new Attribute(
            value: (((int) foreground & 0xffff) << 16) | ((int) background & 0xffff),
            foreground: foreground,
            background: background
        );
    }

    public static Attribute MakeAttribute(ConsoleColor foreground, ConsoleColor background)
    {
        // Encode the colors into the int value.
        return new Attribute(
            value: (((int) foreground & 0xffff) << 16) | ((int) background & 0xffff),
            foreground: foreground.ToColor(),
            background: background.ToColor()
        );
    }

    public static (Color foreground, Color background) AttributeToColors(int attribute)
    {
        var foreground = (Color) ((attribute >> 16) & 0xffff);
        var background = (Color) (attribute & 0xffff);
        return (foreground, background);
    }

    public static (Color foreground, Color background) ToColors(this Attribute attribute)
    {
        return (attribute.Foreground, attribute.Background);
    }
}