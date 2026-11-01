using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

// One planet on the Manage Planets page (also used by the Delete page)
public class PlanetManageViewModel
{
    public Planet Planet { get; set; } = null!;

    // How many mission questions are stored on this planet
    public int QuestionCount { get; set; }

    // Domain rule: the first planet and planets with questions can't be deleted
    public bool CanDelete { get; set; }
}