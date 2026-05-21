namespace MSMES.Domain.Partner;

public class Partner
{
    public int     Id           { get; set; }
    public string  PartnerCode  { get; set; } = string.Empty;
    public string  PartnerName  { get; set; } = string.Empty;
    public string  PartnerType  { get; set; } = string.Empty;  // CUSTOMER, SUPPLIER, BOTH
    public string? BusinessNo   { get; set; }
    public string? RepName      { get; set; }
    public string? Tel          { get; set; }
    public string? Email        { get; set; }
    public string? Address      { get; set; }
    public string? ContactName  { get; set; }
    public string? ContactTel   { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public byte    Rating       { get; set; } = 3;
    public bool    IsActive     { get; set; } = true;
    public string? Memo         { get; set; }
    public DateTime CreatedAt   { get; set; }
    public string? CreatedBy    { get; set; }

    public string TypeName => PartnerType switch {
        "CUSTOMER" => "고객사", "SUPPLIER" => "공급업체", "BOTH" => "고객/공급", _ => PartnerType
    };
    public string TypeCss => PartnerType switch {
        "CUSTOMER" => "primary", "SUPPLIER" => "success", "BOTH" => "info", _ => "secondary"
    };
    public string RatingStars => new string('★', Rating) + new string('☆', 5 - Rating);
}

public interface IPartnerRepository
{
    Task<IReadOnlyList<Partner>> ListAsync(CancellationToken ct = default);
    Task<Partner?>               GetAsync(int id, CancellationToken ct = default);
    Task<int>                    CreateAsync(Partner partner, CancellationToken ct = default);
    Task                         UpdateAsync(Partner partner, CancellationToken ct = default);
    Task<string>                 NextPartnerCodeAsync(string type, CancellationToken ct = default);
}
