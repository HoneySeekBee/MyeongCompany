namespace MSMES.Domain.Bom;

public class Bom
{
    public int     Id          { get; set; }
    public string  BomNo       { get; set; } = string.Empty;
    public string  ProductCode { get; set; } = string.Empty;
    public string  ProductName { get; set; } = string.Empty;
    public string  Version     { get; set; } = "1.0";
    public bool    IsActive    { get; set; } = true;
    public string? Remark      { get; set; }
    public DateTime CreatedAt  { get; set; }
    public string? CreatedBy   { get; set; }
    public IList<BomItem> Items { get; set; } = new List<BomItem>();
}

public class BomItem
{
    public int     Id           { get; set; }
    public int     BomId        { get; set; }
    public int     ItemNo       { get; set; }
    public string  MaterialCode { get; set; } = string.Empty;
    public string  MaterialName { get; set; } = string.Empty;
    public decimal Quantity     { get; set; }
    public string  Unit         { get; set; } = "EA";
    public string? Remark       { get; set; }
}
