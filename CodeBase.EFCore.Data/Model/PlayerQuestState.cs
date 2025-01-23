namespace CodeBase.EFCore.Data.Model;

public class PlayerQuestState
{
    public long PlayerId { get; set; }
    public int QuestId { get; set; }
    public int TotalPoints { get; set; }
    public int LastMilestoneIndexCompleted { get; set; }
    public bool IsComplete { get; set; }
    public DateTime UpdateTime { get; set; }
}