namespace VehicleDemo.Models;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    decimal TotalAmount,
    string Status,
    Guid CustomerId,
    string CustomerName
);