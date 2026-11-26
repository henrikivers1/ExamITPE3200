
namespace GalacticSlicer.Services;

public class RankService : IRankService
{
    // Every rank and the XP it starts at, lowest first.
    // GetRank and GetXpRange both read this list, so a rank's limit only has to be changed here.
    private static readonly (int MinXp, string Name)[] Ranks =
    {
        (0, "Outsider"),
        (500, "Slicer"),
        (1500, "Elite Slicer"),
        (3000, "Breach Specialist"),
        (5000, "Shadow Operator"),
        (8000, "System Architect"),
        (12000, "Galactic Slicer"),
    };

    public string GetRank(int xp)
    {
        return Ranks[CurrentRankIndex(xp)].Name;
    }

    // XP where the current rank starts and where the next one starts.
    // At the top rank both are 12000, so the XP bar shows as full.
    public (int Current, int Next) GetXpRange(int xp)
    {
        int index = CurrentRankIndex(xp);
        int current = Ranks[index].MinXp;
        int next = index < Ranks.Length - 1 ? Ranks[index + 1].MinXp : current;

        return (current, next);
    }

    // Position in Ranks of the highest rank the player has reached
    private static int CurrentRankIndex(int xp)
    {
        int index = 0;
        while (index < Ranks.Length - 1 && xp >= Ranks[index + 1].MinXp)
        {
            index++;
        }
        return index;
    }
}
