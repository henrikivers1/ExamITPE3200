namespace GalacticSlicer.Api.Services;

public sealed class CharacterConflictException : Exception
{
    public CharacterConflictException(string message)
        : base(message)
    {
    }
}
