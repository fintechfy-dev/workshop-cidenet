using Domain.Common;

namespace Domain;

public class User : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;

    public ICollection<Role> Roles { get; set; } = new List<Role>();
}

public enum UserStatus
{
    Active,
    Inactive
}
