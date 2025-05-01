using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace DesafioCCAA.Domain.Domains.Identities;

public class ApplicationUserClaim : IdentityUserClaim<Guid>
{

    public ApplicationUserClaim()
    {

    }

    public ApplicationUserClaim(Claim claim)
    {
        InitializeFromClaim(claim);
    }

    public static ApplicationUserClaim Create(Claim claim)
    {
        return new ApplicationUserClaim(claim);
    }

    public static ApplicationUserClaim Create(string type, string value)
    {
        return new ApplicationUserClaim(new Claim(type, value));
    }

    public static ApplicationUserClaim Create(string type, string value, string? valueType)
    {
        return new ApplicationUserClaim(new Claim(type, value, valueType));
    }

}
