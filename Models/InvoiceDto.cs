namespace VehicleDemo.Models;

/// <summary>
/// Invoice data transfer object
/// </summary>
/// <param name="Id">Unique invoice identifier (GUID)</param>
/// <param name="InvoiceNumber">Invoice number</param>
/// <param name="TotalAmount">Total invoice amount in decimal format</param>
/// <param name="Status">Current invoice status (e.g., Paid, Pending, Draft)</param>
/// <param name="CustomerId">Related customer identifier (GUID)</param>
/// <param name="CustomerName">Related customer name (populated via OData expand)</param>
public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    decimal TotalAmount,
    string Status,
    Guid CustomerId,
    string CustomerName
);