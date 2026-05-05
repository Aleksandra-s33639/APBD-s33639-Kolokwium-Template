using Microsoft.AspNetCore.Mvc;
using APBD_s33639_Kolokwium_Template.DTOs;
using APBD_s33639_Kolokwium_Template.Repositories;

namespace APBD_s33639_Kolokwium_Template.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;

    public CustomersController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id}/rentals")]
    public async Task<IActionResult> GetCustomerRentals(int id)
    {
        var result = await _repository.GetCustomerRentalsAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{id}/rentals")]
    public async Task<IActionResult> AddRental(int id, CreateRentalDto dto)
    {
        if (dto.Movies.Count == 0)
            return BadRequest();

        var customerExists = await _repository.CustomerExistsAsync(id);

        if (!customerExists)
            return NotFound();

        await _repository.AddRentalAsync(id, dto);

        return Created();
    }
}