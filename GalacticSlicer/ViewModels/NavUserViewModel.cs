using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

// What the navbar shows for a logged-in player: name, Credits and the equipped character's badge
public class NavUserViewModel
{
    public string Username { get; set; } = string.Empty;
    public int Credits { get; set; }
    public Character? ActiveCharacter { get; set; }
}