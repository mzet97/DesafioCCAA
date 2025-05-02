using System.Security.Claims;

namespace DesafioCCAA.Application.UseCases.Auth.ViewModels;

public class ClaimViewModel
{
    public string Value { get; set; }
    public string Type { get; set; }


    public ClaimViewModel(Claim claim)
    {
        Value = claim.Value;
        Type = claim.Type;
    }
}
