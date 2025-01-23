using CodeBase.EFCore.Data.Model;

namespace CodeBase.EFCore.Data.Repository.Interface;

public interface IQuestRepository
{
    Task<bool> InitializeQuests(IEnumerable<Quest> quests);
    Task<List<Quest>> GetAllQuestsAsync();
}