using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

// One character on the Manage Characters page (also used by the Delete page)
public class CharacterManageViewModel
{
    public Character Character { get; set; } = null!;

    // How many players have bought this character
    public int OwnerCount { get; set; }

    // Domain rule: nobody may own it, and it can't be the last free character
    public bool CanDelete { get; set; }
}