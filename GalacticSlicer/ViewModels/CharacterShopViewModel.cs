using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

// Everything the Shop page needs, and nothing more
public class CharacterShopViewModel
{
    public IEnumerable<Character> Characters { get; set; } = new List<Character>();

    // The logged-in player's Credits (0 for visitors)
    public int PlayerCredits { get; set; }

    public List<int> OwnedCharacterIds { get; set; } = new();

    public int? ActiveCharacterId { get; set; }

    // Current search and filter values, sent back so the form keeps them after submitting
    public string? SearchString { get; set; }
    public int? MaxPrice { get; set; }
    public string? SortOrder { get; set; }
    public bool OwnedOnly { get; set; }

    // True when at least one filter is used (changes the "nothing found" message)
    public bool IsSearching =>
        !string.IsNullOrWhiteSpace(SearchString) || MaxPrice != null || OwnedOnly;
}