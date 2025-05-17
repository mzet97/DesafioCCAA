using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLivro.Infrastructure.Email;

public interface ISendEmail
{
    Task SendEmailAsync(string email, string subject, string body);
}
