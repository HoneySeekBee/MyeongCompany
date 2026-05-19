using MSMES.Domain.SalesOrder;
using Xunit;

namespace MSMES.Tests;

public class SalesOrderTests
{
    [Fact]
    public void Confirm_Should_Throw_When_No_Items()
    {
        var so = new SalesOrder { SalesOrderNo = "SO001" };
        Assert.Throws<InvalidOperationException>(() => so.Confirm());
    }

    [Fact]
    public void Confirm_Should_Transition_To_Confirmed()
    {
        var so = new SalesOrder { SalesOrderNo = "SO002" };
        so.Items.Add(new SalesOrderItem { ItemNo = 1, ItemCode = "P1", Quantity = 1, UnitPrice = 100 });
        so.Confirm();
        Assert.Equal(SalesOrderStatus.Confirmed, so.Status);
    }

    [Fact]
    public void TotalAmount_Should_Sum_Items()
    {
        var so = new SalesOrder();
        so.Items.Add(new SalesOrderItem { Quantity = 2, UnitPrice = 100 });
        so.Items.Add(new SalesOrderItem { Quantity = 3, UnitPrice = 50 });
        Assert.Equal(350, so.TotalAmount);
    }
}
