using System.Globalization;

namespace GalacticSlicer.Helpers;

// Rarity tier of a character, decided by its price.
// Used by the Shop cards and the badge partial, so the price limits only live here.
public static class CharacterTier
{
    // Key is used in CSS class names (gs-badge--rare), Label is shown to the player
    public static (string Key, string Label) Get(int price) => price switch
    {
        0 => ("starter", "Starter"),
        < 300 => ("uncommon", "Uncommon"),
        < 500 => ("rare", "Rare"),
        < 1000 => ("epic", "Epic"),
        _ => ("legendary", "Legendary")
    };

    // Points for the legendary badge's starburst edge (12 spikes around the center).
    // InvariantCulture keeps "." as the decimal mark, since SVG can't read "50,5" on a Norwegian PC.
    public static string StarburstPoints()
    {
        var points = new List<string>();
        for (var i = 0; i < 24; i++)
        {
            var radius = i % 2 == 0 ? 44 : 37;
            var angle = Math.PI * i / 12 - Math.PI / 2;
            var x = (50 + radius * Math.Cos(angle)).ToString("0.#", CultureInfo.InvariantCulture);
            var y = (50 + radius * Math.Sin(angle)).ToString("0.#", CultureInfo.InvariantCulture);
            points.Add($"{x},{y}");
        }
        return string.Join(" ", points);
    }
}
