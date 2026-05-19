namespace MSMES.Domain.Shared;

public readonly record struct Money(decimal Amount, string Currency = "KRW")
{
    public static Money Zero(string currency = "KRW") => new(0m, currency);

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency) throw new InvalidOperationException("Currency mismatch");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal qty) => new(a.Amount * qty, a.Currency);
}
