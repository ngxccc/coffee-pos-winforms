using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    private static readonly string SqlAuthenticate = SqlFileLoader.Load(SqlKeys.User.Authenticate);
    private static readonly string SqlDeactivateUser = SqlFileLoader.Load(SqlKeys.User.DeactivateUser);
    private static readonly string SqlUpdatePassword = SqlFileLoader.Load(SqlKeys.User.UpdatePassword);

    public async Task<AuthUserDto?> AuthenticateAsync(string username, string password)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlAuthenticate, conn);
        cmd.Parameters.AddWithValue("u", username);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            string dbHash = reader.GetRequiredString("password_hash");

            bool isValid = BCrypt.Net.BCrypt.Verify(password, dbHash);

            if (isValid)
            {
                return new AuthUserDto(
                    reader.GetRequiredInt("id"),
                    reader.GetRequiredString("username"),
                    reader.GetRequiredString("full_name"),
                    reader.GetRequiredInt("role"));
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
