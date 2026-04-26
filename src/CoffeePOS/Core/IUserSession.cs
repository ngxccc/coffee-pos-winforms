using CoffeePOS.Shared.Dtos.User;

namespace CoffeePOS.Core;

public interface IUserSession
{
    AuthUserDto? CurrentUser { get; }
    bool IsLoggedIn { get; }
    DateTime? LoginTime { get; }

    decimal StartingCash { get; }
    void SetStartingCash(decimal amount);

    event Action? OnUserUpdated;

    void Login(AuthUserDto user);
    void Logout();
}
