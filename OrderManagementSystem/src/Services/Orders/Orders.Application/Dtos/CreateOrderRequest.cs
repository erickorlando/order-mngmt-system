namespace Orders.Application.Dtos;

public record CreateOrderRequest
{
    public Guid CustomerId { get; init; }
    public Guid VendorId { get; init; }
    public string? Notes { get; init; }
    public List<CreateOrderItemRequest> Items { get; init; } = new();
}

public record CreateOrderItemRequest
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

public record GetOrdersRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CustomerId { get; init; }
    public Guid? VendorId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SearchTerm { get; init; }
}

public record AddOrderItemRequest
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

public record OrderStatisticsDto
{
    public int TotalOrders { get; init; }
    public int PendingOrders { get; init; }
    public int ConfirmedOrders { get; init; }
    public int CompletedOrders { get; init; }
    public int CancelledOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal AverageOrderValue { get; init; }
    public int TotalCustomers { get; init; }
    public int TotalVendors { get; init; }
    public DateTime PeriodFrom { get; init; }
    public DateTime PeriodTo { get; init; }
}

public record TopCustomerDto
{
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public int OrderCount { get; init; }
    public decimal TotalSpent { get; init; }
    public DateTime LastOrderDate { get; init; }
}


public record TopVendorDto
{
    public Guid VendorId { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public string VendorEmail { get; init; } = string.Empty;
    public int OrderCount { get; init; }
    public decimal TotalSales { get; init; }
    public DateTime LastOrderDate { get; init; }
}