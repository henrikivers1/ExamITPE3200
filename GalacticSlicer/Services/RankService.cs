
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
}
