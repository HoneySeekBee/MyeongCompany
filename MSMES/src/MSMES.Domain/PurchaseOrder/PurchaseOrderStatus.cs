namespace MSMES.Domain.PurchaseOrder;

public enum PurchaseOrderStatus
{
    Draft = 0,
    Issued = 1,
    PartiallyReceived = 2,
    Received = 3,
    Closed = 4
}
