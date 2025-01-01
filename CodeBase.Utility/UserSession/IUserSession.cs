namespace CodeBase.Utility.UserSession;

public interface IUserSession
{
    public string ClientId { get; }
    public string Email { get; }
    public Guid UserSid { get; }
    public string Name { get; }
    public bool IsSupperUser { get; }
    public List<string> Roldes { get; }
}