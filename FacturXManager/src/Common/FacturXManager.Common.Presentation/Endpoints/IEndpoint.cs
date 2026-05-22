using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Common.Presentation.Endpoints;
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
