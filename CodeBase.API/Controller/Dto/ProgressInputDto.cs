namespace CodeBase.API.Controller.Dto;

public record ProgressInputDto
{
    public Guid PlayerId { get; set; }
    public int PlayerLevel { get; set; }
    public int ChipAmountBet { get; set; }
}