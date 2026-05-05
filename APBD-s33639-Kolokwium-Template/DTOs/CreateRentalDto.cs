namespace APBD_s33639_Kolokwium_Template.DTOs;

// DTO do POST-a, czyli dane przychodzące od użytkownika
public class CreateRentalDto
{
    public DateTime RentalDate { get; set; }
    public List<CreateRentalMovieDto> Movies { get; set; } = new();
}

public class CreateRentalMovieDto
{
    public string Title { get; set; } = null!;
    public decimal RentalPrice { get; set; }
}