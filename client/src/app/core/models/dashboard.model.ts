export interface OrdersByStatus {
  status: string;
  count: number;
}

export interface RecentOrderSummary {
  id: string;
  customerName: string;
  status: string;
  total: number;
  createdAt: string;
}

export interface Dashboard {
  totalOrders: number;
  openOrders: number;
  revenueTotal: number;
  revenueThisMonth: number;
  totalProducts: number;
  totalCustomers: number;
  ordersByStatus: OrdersByStatus[];
  recentOrders: RecentOrderSummary[];
}
