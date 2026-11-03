using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

// What the Home page needs: the intro is the same for everyone, players also get a greeting
// and a button to their next mission
public class HomeViewModel
{
    public bool IsLoggedIn { get; set; }
    public string Username { get; set; } = string.Empty;

    // First unlocked planet that isn't completed yet (null if every unlocked one is done)
    public PlanetViewModel? NextPlanet { get; set; }

    // Last unlocked planet, for "Replay" when every unlocked mission is completed
    public PlanetViewModel? LastUnlockedPlanet { get; set; }
}
