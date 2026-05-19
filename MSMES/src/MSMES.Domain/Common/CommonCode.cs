using MSMES.Domain.Shared;

namespace MSMES.Domain.Common;

public class CommonCode : Entity
{
    public string CodeGroup { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CodeName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
