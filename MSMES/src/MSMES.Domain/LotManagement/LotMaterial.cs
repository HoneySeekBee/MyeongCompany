namespace MSMES.Domain.LotManagement;

public class LotMaterial
{
    public int     Id           { get; set; }
    public string  LotNo        { get; set; } = string.Empty;
    public string  MaterialCode { get; set; } = string.Empty;
    public string  MaterialName { get; set; } = string.Empty;
    public decimal Quantity     { get; set; }
    public string  Unit         { get; set; } = "EA";
    public string? ReceiptNo    { get; set; }
    public DateTime CreatedAt   { get; set; }
}
