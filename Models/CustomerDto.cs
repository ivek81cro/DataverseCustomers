namespace VehicleDemo.Models;

public record CustomerDto(
    Guid Id,
    string Name,
    string Address,
    string Email
);
