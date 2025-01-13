using CodeBase.EFCore.Data.Model;

namespace CodeBase.QuestService;

public interface IQuestService
{
    Task<bool> InitializeQuestsAsync();
    Task<List<Quest>> GetAllQuestsAsync();
}