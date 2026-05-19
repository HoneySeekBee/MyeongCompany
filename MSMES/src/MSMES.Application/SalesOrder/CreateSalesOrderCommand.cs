using MSMES.Domain.SalesOrder;

namespace MSMES.Application.SalesOrder;

public sealed record CreateSalesOrderItemDto(string ItemCode, string ItemName, decimal Quantity, decimal UnitPrice);

public sealed record CreateSalesOrderCommand(
    string CustomerCode,
    string CustomerName,
    DateTime OrderDate,
    DateTime DueDate,
    IReadOnlyList<CreateSalesOrderItemDto> Items,
    string CreatedBy);

public sealed class CreateSalesOrderHandler
{
    private readonly ISalesOrderRepository _repo;
    public CreateSalesOrderHandler(ISalesOrderRepository repo) => _repo = repo;

    public async Task<string> HandleAsync(CreateSalesOrderCommand cmd, CancellationToken ct = default)
    {
        var no = await _repo.NextNumberAsync(ct);
        var order = new Domain.SalesOrder.SalesOrder
        {
            SalesOrderNo = no,
            CustomerCode = cmd.CustomerCode,
            CustomerName = cmd.CustomerName,
            OrderDate = cmd.OrderDate,
            DueDate = cmd.DueDate,
            Status = SalesOrderStatus.Draft,
            CreatedBy = cmd.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        int n = 1;
        foreach (var i in cmd.Items)
        {
            order.Items.Add(new SalesOrderItem
            {
                SalesOrderNo = no,
                ItemNo = n++,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            });
        }
        await _repo.AddAsync(order, ct);
        return no;
    }
}
