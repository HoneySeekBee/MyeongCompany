namespace MSMES.Domain.LotManagement;

public enum LotStatus
{
    Created = 0,
    InProcess = 1,
    QualityHold = 2,
    Released = 3,
    Shipped = 4,
    Scrapped = 5
}
