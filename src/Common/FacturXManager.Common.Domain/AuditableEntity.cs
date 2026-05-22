using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturXManager.Common.Domain;
public class AuditableEntity : Entity
{
    public DateTime CreationDate { get; set; }

    public DateTime LastUpdateDate { get; set; }
}
