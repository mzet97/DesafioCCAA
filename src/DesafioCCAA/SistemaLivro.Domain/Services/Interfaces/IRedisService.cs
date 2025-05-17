using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLivro.Domain.Services.Interfaces;

public interface IRedisService
{
    Task SetValueAsync(string key, string value);
    Task<string?> GetValueAsync(string key);
    Task RemoveValueAsync(string key);
}
