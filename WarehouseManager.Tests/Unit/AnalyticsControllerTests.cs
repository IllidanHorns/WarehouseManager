using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseManagerApi.Controllers;

namespace WarehouseManager.Tests.Unit;

[TestClass]
public class AnalyticsControllerTests
{
    [TestMethod]
    public void ResolvePeriod_DefaultsToLastThreeMonths()
    {
        // Act
        var (start, end) = AnalyticsController.ResolvePeriod(null, null);

        // Assert
        Assert.IsTrue((DateTime.UtcNow - end).TotalSeconds < 5, "Конечная дата должна быть близка к текущему моменту.");
        Assert.AreEqual(end.AddMonths(-3), start, "Дата начала должна быть на 3 месяца раньше конца.");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ResolvePeriod_Throws_WhenStartAfterEnd()
    {
        // Arrange
        var end = DateTime.UtcNow;
        var start = end.AddDays(1);

        // Act
        _ = AnalyticsController.ResolvePeriod(start, end);
    }
}

