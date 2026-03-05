# DevHound.Emissary

Lightweight request/response mediator with CQRS-friendly markers.

## Install

Reference the project or package:

- Project reference: `DevHound.Emissary.csproj`
- Package id: `DevHound.Emissary`

## Register

```csharp
using DevHound.Emissary;
using Microsoft.Extensions.DependencyInjection;

services.AddEmissary(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(CreateOrderCommand).Assembly));
```

## Define commands and queries

```csharp
using DevHound.Emissary;

public sealed record CreateOrderCommand(int OrderId) : ICommand<int>;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, int>
{
    public Task<int> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        return Task.FromResult(command.OrderId);
    }
}

public sealed record GetOrderQuery(int OrderId) : IQuery<string>;

public sealed class GetOrderHandler : IQueryHandler<GetOrderQuery, string>
{
    public Task<string> Handle(GetOrderQuery query, CancellationToken ct)
    {
        return Task.FromResult($"order:{query.OrderId}");
    }
}
```

## Send requests

```csharp
using DevHound.Emissary;
using Microsoft.AspNetCore.Mvc;

public sealed class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost("orders/{id:int}")]
    public async Task<IActionResult> Create(int id, CancellationToken ct)
    {
        var createdId = await sender.SendCommand(new CreateOrderCommand(id), ct);
        return Ok(createdId);
    }

    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var value = await sender.SendQuery(new GetOrderQuery(id), ct);
        return Ok(value);
    }
}
```

## Notes

- Target framework: `netstandard2.1`
- No notifications or pipeline behaviors (by design)
- CQRS markers are optional; `IRequest<TResponse>` and `IRequestHandler<,>` still work
