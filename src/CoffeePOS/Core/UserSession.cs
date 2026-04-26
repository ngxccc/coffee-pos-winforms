using CoffeePOS.Shared.Dtos.User;

namespace CoffeePOS.Core;

public class UserSession : IUserSession
{
    public AuthUserDto? CurrentUser { get; private set; }
    public DateTime? LoginTime { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public event Action? OnUserUpdated;
    public decimal StartingCash { get; private set; }

    public void Login(AuthUserDto user)
    {
        CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
        LoginTime = DateTime.Now;
    }

    public void Logout()
    {
        CurrentUser = null;
        LoginTime = null;
        StartingCash = 0;
    }

    public void SetStartingCash(decimal amount)
    {
        StartingCash = amount;
    }

    public void UpdateProfile(AuthUserDto updatedUser)
    {
        CurrentUser = updatedUser;

        // Kích hoạt Event (Hét lên).
        // Dấu ngoặc chấm hỏi (?.) nghĩa là: Nếu có ai đang nghe thì hét, không ai nghe thì thôi.
        OnUserUpdated?.Invoke();
    }
}
