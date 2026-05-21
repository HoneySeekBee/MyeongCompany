namespace MSMES.Domain.Receiving;

public class GoodsReceipt
{
    public int     Id              { get; set; }
    public string  ReceiptNo       { get; set; } = string.Empty;
    public string  PurchaseOrderNo { get; set; } = string.Empty;
    public string  SupplierName    { get; set; } = string.Empty;
    public DateTime ReceiptDate    { get; set; }
    public byte    Status          { get; set; }
    public string? InspectorName   { get; set; }
    public DateTime? InspectedAt   { get; set; }
    public string? Remark          { get; set; }
    public DateTime CreatedAt      { get; set; }
    public string? CreatedBy       { get; set; }
    public IList<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();

    public string StatusName => Status switch {
        0 => "입고대기", 1 => "검사중", 2 => "합격", 3 => "불합격", 4 => "조건부합격", _ => "-"
    };
    public string StatusCss => Status switch {
        0 => "secondary", 1 => "warning", 2 => "success", 3 => "danger", 4 => "info", _ => "secondary"
    };
    public decimal TotalReceived => Items.Sum(i => i.ReceivedQty);
    public decimal TotalAccepted => Items.Sum(i => i.AcceptedQty);
}

public class GoodsReceiptItem
{
    public int     Id             { get; set; }
    public int     GoodsReceiptId { get; set; }
    public string  MaterialCode   { get; set; } = string.Empty;
    public string  MaterialName   { get; set; } = string.Empty;
    public decimal OrderedQty     { get; set; }
    public decimal ReceivedQty    { get; set; }
    public decimal InspectedQty   { get; set; }
    public decimal AcceptedQty    { get; set; }
    public decimal RejectedQty    { get; set; }
    public string? DefectReason   { get; set; }
    public string  Unit           { get; set; } = "EA";
}
