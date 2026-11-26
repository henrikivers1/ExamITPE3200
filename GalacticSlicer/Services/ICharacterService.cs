namespace GalacticSlicer.Services;

// Buying and equipping characters in the shop
public interface ICharacterService
{
    Task<bool> BuyCharacterAsync(string userId, int characterId);
    Task<bool> EquipCharacterAsync(string userId, int characterId);
    Task<bool> UnequipCharacterAsync(string userId);
    Task<List<int>> GetOwnedCharacterIdsAsync(string userId);
}