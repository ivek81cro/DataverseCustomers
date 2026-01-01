namespace VehicleDemo.Models;

/// <summary>
/// Customer data transfer object
/// </summary>
/// <param name="Id">Unique customer identifier (GUID)</param>
/// <param name="Name">Customer name</param>
/// <param name="Address">Customer physical address</param>
/// <param name="Email">Customer email address</param>
public record CustomerDto(
    Guid Id,
    string Name,
    string Address,
    string Email
);
