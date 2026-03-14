using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories.Impl;

public class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = "SELECT id, username, password_hash, full_name, role FROM users WHERE username = @u AND is_active = true";
        using var cmd = new NpgsqlCommand(sql, conn);
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

        string sql = @"
            UPDATE users
            SET is_active = false, updated_at = NOW()
            WHERE id = @id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", targetUserId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        string sql = "UPDATE users SET password_hash = @hash, updated_at = NOW() WHERE id = @id";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("hash", newPasswordHash);
        cmd.Parameters.AddWithValue("id", userId);
        await cmd.ExecuteNonQueryAsync();
    }
}
