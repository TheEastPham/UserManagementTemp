namespace CodeBase.API.Domain.Model.TransferObject.UserData;

public class UserRoleModelDto
{
    public string Email { get; set; }

    public string Name { get; set; }

    public Guid UserId { get; set; }

    public Dictionary<Guid, RoleNames> Roles { get; set; }
    public Guid PrimaryRegionId { get; set; }

    public string CountryCode { get; set; }


    #region Role validate

    public bool IsSuperUser
    {
        get { return Roles.Any(r => r.Value is RoleNames.Admin or RoleNames.Supporter); }
    }

    public bool IsPlayer
    {
        get { return Roles.Any(r => r.Value == RoleNames.Player); }
    }
    #endregion
}