namespace APBD_s33639_Kolokwium_Template.DTOs;

// DTO = obiekt, który zwracamy jako JSON
public class CustomerRentalsDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public List<RentalDto> Rentals { get; set; } = new();
}

public class RentalDto
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = null!;
    public List<MovieDto> Movies { get; set; } = new();
}

public class MovieDto
{
    public string Title { get; set; } = null!;
    public decimal PriceAtRental { get; set; }
}