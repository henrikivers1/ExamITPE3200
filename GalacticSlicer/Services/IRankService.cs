namespace GalacticSlicer.Services;

// Ranks and XP limits for each rank
public interface IRankService
{
    string GetRank(int xp);
    (int Current, int Next) GetXpRange(int xp);
}