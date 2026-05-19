using MSMES.Domain.Shared;

namespace MSMES.Domain.Quality;

public class DefectType : Entity
{
    public string DefectTypeCode { get; set; } = string.Empty;
    public string DefectTypeName { get; set; } = string.Empty;
    public string? DefectCause { get; set; }
    public bool IsActive { get; set; } = true;
}
