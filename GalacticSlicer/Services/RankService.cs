
namespace GalacticSlicer.Services;

public class RankService
{
    public string GetRank(int xp)
    {
        return xp switch
        {
            >= 12000 => "Galactic Slicer",
            >= 8000 => "System Architect",
            >= 5000 => "Shadow Operator",
            >= 3000 => "Breach Specialist",
            >= 1500 => "Elite Slicer",
            >= 500 => "Slicer",
            _ => "Outsider"
        };
    }

    // XP where the current rank starts and where the next one starts (same limits as GetRank).
    // At the top rank both are 12000, so the XP bar shows as full.
    public (int Current, int Next) GetXpRange(int xp)
    {
        return xp switch
        {
            >= 12000 => (12000, 12000),
            >= 8000 => (8000, 12000),
            >= 5000 => (5000, 8000),
            >= 3000 => (3000, 5000),
            >= 1500 => (1500, 3000),
            >= 500 => (500, 1500),
            _ => (0, 500)
        };
    }
}
