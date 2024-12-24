using System.Text;
using CodeBase.API.Domain.Model.TransferObject;
using CodeBase.API.Domain.Model.TransferObject.UserData;
using Newtonsoft.Json;

namespace CodeBase.API.Extension;

public static class HeaderExtensions
{
    public static UserDataDto? GetUserIdentify(this IHeaderDictionary header)
    {
        return header.ContainsKey("x-gs-user")
            ? JsonConvert.DeserializeObject<UserDataDto>(
                Encoding.Default.GetString(
                    Convert.FromBase64String(header["x-gs-user"])))
            : null;
    }

    public static UserRoleModelDto ToUserRoleModel(this UserDataDto userDataDto)
    {
        var roles = new Dictionary<Guid, RoleNames>();

        userDataDto?.Roles.ToList().ForEach(r =>
        {
            roles.Add(r.Id, r.Name.ToRoleName());
        });

        return userDataDto == null ? null : new UserRoleModelDto
        {
            UserId = userDataDto.Id,
            Roles = roles,
            Name = userDataDto.Name,
            Email = userDataDto.Email,
            PrimaryRegionId = userDataDto.PrimaryRegionId,
            CountryCode = userDataDto.CountryCode
        };
    }

    private static RoleNames ToRoleName(this string roleName)
    {
        return Enum.Parse<RoleNames>(roleName);
    }
}