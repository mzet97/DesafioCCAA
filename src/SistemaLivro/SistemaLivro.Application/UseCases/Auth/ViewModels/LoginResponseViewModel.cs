using MediatR;
using SistemaLivro.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLivro.Application.UseCases.Auth.ViewModels;

public class LoginResponseViewModel : IRequest<BaseResult<LoginResponseViewModel>>
{
    public string AccessToken { get; set; } = string.Empty;
    public double ExpiresIn { get; set; }
    public UserTokenViewModel UserToken { get; set; } = new UserTokenViewModel();
}
