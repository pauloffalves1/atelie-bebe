namespace AtelieBebe.Application.Dashboard;

public sealed record OrdersByStatusDto(string Status, int Count);

public sealed record RecentOrderSummaryDto(Guid Id, string CustomerName, string Status, decimal Total, DateTime CreatedAt);

public sealed record DashboardDto(
    int TotalOrders,
    int OpenOrders,
    decimal RevenueTotal,
    decimal RevenueThisMonth,
    int TotalProducts,
    int LowStockProducts,
    int TotalCustomers,
    IReadOnlyList<OrdersByStatusDto> OrdersByStatus,
    IReadOnlyList<RecentOrderSummaryDto> RecentOrders);
