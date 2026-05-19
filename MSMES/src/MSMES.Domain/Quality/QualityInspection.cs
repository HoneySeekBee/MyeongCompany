using MSMES.Domain.Shared;

namespace MSMES.Domain.Quality;

public class QualityInspection : Entity
{
    public string InspectionNo { get; set; } = string.Empty;
    public string LotNo { get; set; } = string.Empty;
    public string InspectionItem { get; set; } = string.Empty;
    public QualityStatus Result { get; set; } = QualityStatus.Pending;
    public decimal InspectedQuantity { get; set; }
    public decimal DefectQuantity { get; set; }
    public string? DefectTypeCode { get; set; }
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public string Inspector { get; set; } = string.Empty;
    public string? Remarks { get; set; }

    public decimal DefectRate => InspectedQuantity == 0 ? 0 : Math.Round(DefectQuantity / InspectedQuantity * 100m, 2);
}
