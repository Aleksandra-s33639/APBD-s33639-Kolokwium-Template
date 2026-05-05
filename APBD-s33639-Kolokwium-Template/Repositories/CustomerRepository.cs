using Microsoft.Data.SqlClient;
using APBD_s33639_Kolokwium_Template.DTOs;

namespace APBD_s33639_Kolokwium_Template.Repositories;

public class CustomerRepository : ICustomerRepository 
{
    private readonly IConfiguration _configuration;

    public CustomerRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Tworzy połączenie z bazą danych
    private SqlConnection GetConnection()
    {
        return new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection")
        );
    }

    // Sprawdza czy klient istnieje
    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();

        var command = new SqlCommand("""
            SELECT 1
            FROM Customer
            WHERE customer_id = @customerId
        """, connection);

        command.Parameters.AddWithValue("@customerId", customerId);

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }

    // Pobiera klienta + jego wypożyczenia
    public async Task<CustomerRentalsDto?> GetCustomerRentalsAsync(int customerId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();

        // Pobranie klienta
        var customerCommand = new SqlCommand("""
            SELECT first_name, last_name
            FROM Customer
            WHERE customer_id = @customerId
        """, connection);

        customerCommand.Parameters.AddWithValue("@customerId", customerId);

        var reader = await customerCommand.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var result = new CustomerRentalsDto
        {
            FirstName = reader.GetString(0),
            LastName = reader.GetString(1)
        };

        await reader.CloseAsync();

        // Pobranie wypożyczeń + filmów
        var rentalsCommand = new SqlCommand("""
            SELECT 
                r.rental_id,
                r.rental_date,
                r.return_date,
                s.name,
                m.title,
                ri.price_at_rental
            FROM Rental r
            JOIN Status s ON r.status_id = s.status_id
            JOIN Rental_Item ri ON r.rental_id = ri.rental_id
            JOIN Movie m ON ri.movie_id = m.movie_id
            WHERE r.customer_id = @customerId
            ORDER BY r.rental_id
        """, connection);

        rentalsCommand.Parameters.AddWithValue("@customerId", customerId);

        var r = await rentalsCommand.ExecuteReaderAsync();

        while (await r.ReadAsync())
        {
            var rentalId = r.GetInt32(0);

            var rental = result.Rentals.FirstOrDefault(x => x.Id == rentalId);

            if (rental == null)
            {
                rental = new RentalDto
                {
                    Id = rentalId,
                    RentalDate = r.GetDateTime(1),
                    ReturnDate = r.IsDBNull(2) ? null : r.GetDateTime(2),
                    Status = r.GetString(3)
                };

                result.Rentals.Add(rental);
            }

            rental.Movies.Add(new MovieDto
            {
                Title = r.GetString(4),
                PriceAtRental = r.GetDecimal(5)
            });
        }

        return result;
    }

    // Dodaje wypożyczenie (POST)
    public async Task AddRentalAsync(int customerId, CreateRentalDto dto)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            // Dodanie wypożyczenia
            var rentalCommand = new SqlCommand("""
                INSERT INTO Rental (rental_date, return_date, customer_id, status_id)
                OUTPUT INSERTED.rental_id
                VALUES (@rentalDate, NULL, @customerId, 1)
            """, connection, transaction);

            rentalCommand.Parameters.AddWithValue("@rentalDate", dto.RentalDate);
            rentalCommand.Parameters.AddWithValue("@customerId", customerId);

            var rentalId = (int)(await rentalCommand.ExecuteScalarAsync())!;

            // Dodanie filmów
            foreach (var movie in dto.Movies)
            {
                var movieIdCommand = new SqlCommand("""
                    SELECT movie_id
                    FROM Movie
                    WHERE title = @title
                """, connection, transaction);

                movieIdCommand.Parameters.AddWithValue("@title", movie.Title);

                var movieId = (int)(await movieIdCommand.ExecuteScalarAsync())!;

                var itemCommand = new SqlCommand("""
                    INSERT INTO Rental_Item (rental_id, movie_id, price_at_rental)
                    VALUES (@rentalId, @movieId, @price)
                """, connection, transaction);

                itemCommand.Parameters.AddWithValue("@rentalId", rentalId);
                itemCommand.Parameters.AddWithValue("@movieId", movieId);
                itemCommand.Parameters.AddWithValue("@price", movie.RentalPrice);

                await itemCommand.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}