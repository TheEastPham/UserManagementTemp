using CodeBase.EFCore.Data.Model;

namespace CodeBase.Service;

public interface IQuestService
{
    Task<bool> InitializeQuestsAsync();
    Task<List<Quest>> GetAllQuestsAsync();
}