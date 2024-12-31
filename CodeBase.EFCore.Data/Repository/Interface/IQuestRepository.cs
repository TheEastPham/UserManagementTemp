using CodeBase.EFCore.Data.Model;

namespace CodeBase.EFCore.Data.Repository.Interface;

public interface IQuestRepository
{
    Task<bool> UpdateQuests(IEnumerable<Quest> quests);
}