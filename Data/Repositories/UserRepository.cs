using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    private static readonly string SqlGetAll = SqlFileLoader.Load(SqlKeys.User.GetAll);
    private static readonly string SqlAuthenticate = SqlFileLoader.Load(SqlKeys.User.Authenticate);
    private static readonly string SqlInsert = SqlFileLoader.Load(SqlKeys.User.Insert);
    private static readonly string SqlSetActiveStatus = SqlFileLoader.Load(SqlKeys.User.SetActiveStatus);
    private static readonly string SqlUpdatePassword = SqlFileLoader.Load(SqlKeys.User.UpdatePassword);

    public async Task<List<UserGridDto>> GetAllUsersAsync()
    {
        var result = new List<UserGridDto>();

        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlGetAll, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int role = reader.GetRequiredInt("role");
            bool isActive = reader.GetRequiredBool("is_active");

            result.Add(new UserGridDto(
                reader.GetRequiredInt("id"),
                reader.GetRequiredString("username"),
                reader.GetNullableString("full_name") ?? "---",
                role,
                role == 0 ? "Admin" : "Thu ngân",
                isActive,
                isActive ? "Đang hoạt động" : "Đã khóa"));
        }

        return result;
    }

    public async Task<AuthUserDto?> AuthenticateAsync(string username, string password)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlAuthenticate, conn);
        cmd.Parameters.AddWithValue("u", username);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            string dbHash = reader.GetRequiredString("password_hash");
            bool isActive = reader.GetRequiredBool("is_active");

            bool isValid = BCrypt.Net.BCrypt.Verify(password, dbHash);

            if (isValid)
            {
                if (!isActive)
                {
                    throw new InvalidOperationException("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                }

                return new AuthUserDto(
                    reader.GetRequiredInt("id"),
                    reader.GetRequiredString("username"),
                    reader.GetRequiredString("full_name"),
                    reader.GetRequiredInt("role"));
            }
        }

        return null;
    }

    public async Task InsertUserAsync(string username, string fullName, int role, string passwordHash)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlInsert, conn);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("hash", passwordHash);
        cmd.Parameters.AddWithValue("fullName", fullName);
        cmd.Parameters.AddWithValue("role", role);

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại, vui lòng chọn tên khác!");
        }
    }

    public async Task SetUserActiveStatusAsync(int targetUserId, bool isActive)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlSetActiveStatus, conn);
        cmd.Parameters.AddWithValue("id", targetUserId);
        cmd.Parameters.AddWithValue("isActive", isActive);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeactivateUserAsync(int adminId, int targetUserId)
    {
        if (adminId == targetUserId)
        {
            throw new InvalidOperationException("Hệ thống từ chối lệnh tự hủy tài khoản của chính bạn!");
        }

        await SetUserActiveStatusAsync(targetUserId, false);
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
