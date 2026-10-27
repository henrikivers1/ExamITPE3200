using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

// Everything the Shop page needs, and nothing more
public class CharacterShopViewModel
{
    public IEnumerable<Character> Characters { get; set; } = new List<Character>();

    // Placeholder until players and Credits exist (Jonas, Task 5)
    public int PlayerCredits { get; set; }

    public List<int> OwnedCharacterIds { get; set; } = new();

    public int? ActiveCharacterId { get; set; }
}