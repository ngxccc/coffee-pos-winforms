using CoffeePOS.Models;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    private static readonly string SqlAuthenticate = SqlFileLoader.Load("User.authenticate.sql");
    private static readonly string SqlDeactivateUser = SqlFileLoader.Load("User.deactivate_user.sql");
    private static readonly string SqlUpdatePassword = SqlFileLoader.Load("User.update_password.sql");

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlAuthenticate, conn);
        cmd.Parameters.AddWithValue("u", username);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            string dbHash = reader.GetString(2);

            bool isValid = BCrypt.Net.BCrypt.Verify(password, dbHash);

            if (isValid)
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    FullName = reader.GetString(3),
                    Role = reader.GetInt32(4)
                };
            }
        }

        return null;
    }

    public async Task DeactivateUserAsync(int adminId, int targetUserId)
    {
        if (adminId == targetUserId)
        {
            throw new InvalidOperationException("Hệ thống từ chối lệnh tự hủy tài khoản của chính bạn!");
        }

        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlDeactivateUser, conn);
        cmd.Parameters.AddWithValue("id", targetUserId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlUpdatePassword, conn);
        cmd.Parameters.AddWithValue("hash", newPasswordHash);
        cmd.Parameters.AddWithValue("id", userId);
        await cmd.ExecuteNonQueryAsync();
    }
}
