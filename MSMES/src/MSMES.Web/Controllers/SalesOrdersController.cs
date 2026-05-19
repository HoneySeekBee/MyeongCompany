using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.SalesOrder;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/sales-orders")]
public class SalesOrdersController : ControllerBase
{
    private readonly CreateSalesOrderHandler _create;
    private readonly GetSalesOrderHandler _get;
    private readonly ListSalesOrdersHandler _list;

    public SalesOrdersController(CreateSalesOrderHandler c, GetSalesOrderHandler g, ListSalesOrdersHandler l)
    { _create = c; _get = g; _list = l; }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await _list.HandleAsync(new ListSalesOrdersQuery(skip, take), ct));

    [HttpGet("{no}")]
    public async Task<IActionResult> Get(string no, CancellationToken ct)
    {
        var so = await _get.HandleAsync(new GetSalesOrderQuery(no), ct);
        return so is null ? NotFound() : Ok(so);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderCommand cmd, CancellationToken ct)
    {
        var no = await _create.HandleAsync(cmd, ct);
        return CreatedAtAction(nameof(Get), new { no }, new { salesOrderNo = no });
    }
}
