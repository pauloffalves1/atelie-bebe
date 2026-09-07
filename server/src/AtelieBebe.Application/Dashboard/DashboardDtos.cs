namespace AtelieBebe.Application.Dashboard;

public sealed record OrdersByStatusDto(string Status, int Count);

public sealed record RecentOrderSummaryDto(Guid Id, string CustomerName, string Status, decimal Total, DateTime CreatedAt);

public sealed record TopProductDto(string ProductName, int QuantitySold, decimal Revenue);

public sealed record SalesByDayDto(DateTime Date, decimal Revenue, int OrderCount);

public sealed record DashboardDto(
    int TotalOrders,
    int OpenOrders,
    decimal RevenueTotal,
    decimal RevenueThisMonth,
    decimal AverageOrderValue,
    int TotalProducts,
    int TotalCustomers,
    IReadOnlyList<OrdersByStatusDto> OrdersByStatus,
    IReadOnlyList<RecentOrderSummaryDto> RecentOrders,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<SalesByDayDto> SalesLast30Days);
