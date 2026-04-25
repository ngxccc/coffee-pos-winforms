using CoffeePOS.Data.Repositories.Contracts;

using CoffeePOS.Shared.Dtos.User;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    private static readonly string SqlGetAll = SqlFileLoader.Load(SqlKeys.User.GetAll);
    private static readonly string SqlAuthenticate = SqlFileLoader.Load(SqlKeys.User.Authenticate);
    private static readonly string SqlInsert = SqlFileLoader.Load(SqlKeys.User.Insert);
    private static readonly string SqlUpdateProfile = SqlFileLoader.Load(SqlKeys.User.UpdateProfile);
    private static readonly string SqlSetActiveStatus = SqlFileLoader.Load(SqlKeys.User.SetActiveStatus);
    private static readonly string SqlUpdatePassword = SqlFileLoader.Load(SqlKeys.User.UpdatePassword);

    public async Task<List<UserGridDto>> GetAllUsersAsync()
    {
        var result = new List<UserGridDto>();

        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlGetAll, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        // PERF: O(N) Iteration.
        // WHY: Bypassing string allocation by extracting the natively mapped Enum directly.
        int idOrdinal = reader.GetOrdinal("id");
        int usernameOrdinal = reader.GetOrdinal("username");
        int fullNameOrdinal = reader.GetOrdinal("full_name");
        int roleOrdinal = reader.GetOrdinal("role");
        int isActiveOrdinal = reader.GetOrdinal("is_active");

        while (await reader.ReadAsync())
        {
            var role = reader.GetFieldValue<UserRole>(roleOrdinal);
            var isActive = reader.GetBoolean(isActiveOrdinal);

            result.Add(new UserGridDto(
                reader.GetInt32(idOrdinal),
                reader.GetString(usernameOrdinal),
                reader.IsDBNull(fullNameOrdinal) ? "---" : reader.GetString(fullNameOrdinal),
                role,
                role.ToDisplayName(),
                isActive,
                isActive ? "Đang hoạt động" : "Đã khóa"));
        }

        return result;
    }

    public async Task<AuthUserDto?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        using var conn = await dataSource.OpenConnectionAsync(cancellationToken);
        using var cmd = new NpgsqlCommand(SqlAuthenticate, conn);
        cmd.Parameters.Add(new NpgsqlParameter<string>("u", username));

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            string dbHash = reader.GetFieldValue<string>(reader.GetOrdinal("password_hash"));
            bool isActive = reader.GetBoolean(reader.GetOrdinal("is_active"));

            // HACK: Synchronous BCrypt verify inside Async method.
            // It takes ~300ms. If heavily concurrent, consider Task.Run(() => BCrypt.Verify(...))
            if (await Task.Run(() => BCrypt.Net.BCrypt.Verify(password, dbHash)))
            {
                if (!isActive)
                    throw new InvalidOperationException("Tài khoản của bạn đã bị khóa!\nMọi thắc mắc xin liên hệ quản trị viên.");

                return new AuthUserDto(
                    reader.GetInt32(reader.GetOrdinal("id")),
                    reader.GetString(reader.GetOrdinal("username")),
                    reader.GetString(reader.GetOrdinal("full_name")),
                    reader.GetFieldValue<UserRole>(reader.GetOrdinal("role")));
            }
        }

        return null;
    }

    public async Task InsertUserAsync(string username, string fullName, UserRole role, string passwordHash)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlInsert, conn);

        // PERF: Sử dụng Generic NpgsqlParameter<T> để zero-boxing và bypass hoàn toàn Type Inference
        cmd.Parameters.Add(new NpgsqlParameter<string>("username", username));
        cmd.Parameters.Add(new NpgsqlParameter<string>("hash", passwordHash));
        cmd.Parameters.Add(new NpgsqlParameter<string>("full_name", fullName));
        // WHY: Truyền thẳng Enum xuống. Npgsql Global Mapper sẽ tự động dịch nó sang PostgreSQL custom enum type
        cmd.Parameters.Add(new NpgsqlParameter<UserRole>("role", role));

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại, vui lòng chọn tên khác!");
        }
    }

    public async Task UpdateUserProfileAsync(int userId, string username, string fullName, UserRole role)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlUpdateProfile, conn);

        cmd.Parameters.Add(new NpgsqlParameter<int>("id", userId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("username", username));
        cmd.Parameters.Add(new NpgsqlParameter<string>("full_name", fullName));
        cmd.Parameters.Add(new NpgsqlParameter<UserRole>("role", role));

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại, vui lòng chọn tên khác!");
        }
    }

    public async Task SetUserActiveStatusAsync(int targetUserId, bool isActive)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlSetActiveStatus, conn);

        cmd.Parameters.Add(new NpgsqlParameter<int>("id", targetUserId));
        cmd.Parameters.Add(new NpgsqlParameter<bool>("is_active", isActive));

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

        cmd.Parameters.Add(new NpgsqlParameter<int>("id", userId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("hash", newPasswordHash));

        await cmd.ExecuteNonQueryAsync();
    }
}
