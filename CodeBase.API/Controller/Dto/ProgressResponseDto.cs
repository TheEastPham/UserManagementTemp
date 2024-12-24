namespace CodeBase.API.Controller.Dto;

public class ProgressResponseDto
{
    public int QuestPointsEarned { get; set; }
    public int TotalQuestPercentCompleted { get; set; }
    public MilestonesCompleted MilestonesCompleted { get; set; }
}