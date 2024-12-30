using System.Collections.ObjectModel;

namespace CodeBase.EFCore.Data.Model;

public class Quest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TotalPointsRequired { get; set; }
    public double RateFromBet { get; set; }
    public double LevelBonusRate { get; set; }
    public virtual Collection<Milestone> Milestones { get; set; }
}