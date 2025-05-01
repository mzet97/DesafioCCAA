using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesafioCCAA.Shared.Models;


public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; protected set; }

    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }
}