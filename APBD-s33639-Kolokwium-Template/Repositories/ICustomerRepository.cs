using APBD_s33639_Kolokwium_Template.DTOs;

namespace APBD_s33639_Kolokwium_Template.Repositories;

public interface ICustomerRepository
{
    Task<bool> CustomerExistsAsync(int customerId);

    Task<CustomerRentalsDto?> GetCustomerRentalsAsync(int customerId);

    Task AddRentalAsync(int customerId, CreateRentalDto dto);
}