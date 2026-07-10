using Domain.Common;

namespace Domain;

public class Permission : Entity
{
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
