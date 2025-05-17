﻿using Microsoft.AspNetCore.Identity;

namespace SistemaLivro.Domain.Domains.Identities;

public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public ApplicationUser User { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
}
