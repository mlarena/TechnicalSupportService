using Microsoft.AspNetCore.Identity;

namespace TechnicalSupportService.Data.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
