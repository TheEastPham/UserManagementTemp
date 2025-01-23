namespace CodeBase.API.Controller.Dtos;

public class QuestDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TotalPointsRequired { get; set; }
    public double RateFromBet { get; set; }
    public double LevelBonusRate { get; set; }
    public List<MilestoneDto> Milestones { get; set; }
}