using EasyStock.Web.Extensions;
using EasyStock.Web.Models;

namespace EasyStock.Tests.Unit;

public class DomainRulesTests
{
    [Theory]
    [InlineData(5, 2, false)]
    [InlineData(3, 2, false)]
    [InlineData(2, 2, true)]
    [InlineData(0, 2, true)]
    public void IsLow_ReturnsTrue_AtOrBelowMinimum(int quantity, int minimum, bool expected)
    {
        Assert.Equal(expected, Item.IsLow(quantity, minimum));
    }

    [Theory]
    [InlineData(MovementType.Entry, true)]
    [InlineData(MovementType.Return, true)]
    [InlineData(MovementType.AssignmentReturn, true)]
    [InlineData(MovementType.Exit, false)]
    [InlineData(MovementType.Assignment, false)]
    public void IsIncrease_MatchesTheMovementDirection(MovementType type, bool expected)
    {
        Assert.Equal(expected, type.IsIncrease());
    }

    [Theory]
    [InlineData(MovementType.Entry, true)]
    [InlineData(MovementType.Exit, true)]
    [InlineData(MovementType.Return, true)]
    [InlineData(MovementType.Assignment, false)]
    [InlineData(MovementType.AssignmentReturn, false)]
    public void IsManual_ExcludesAssignmentMovements(MovementType type, bool expected)
    {
        Assert.Equal(expected, type.IsManual());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  Finance  ", "Finance")]
    public void TrimToNull_TrimsOrReturnsNull(string? input, string? expected)
    {
        Assert.Equal(expected, input.TrimToNull());
    }
}