using CoffeePOS.Models;

namespace CoffeePOS.Core;

public interface IUserSession
{
    User? CurrentUser { get; }
    bool IsLoggedIn { get; }

    event Action? OnUserUpdated;

    void Login(User user);
    void Logout();
}

public class UserSession : IUserSession
{
    public User? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser != null;

    public event Action? OnUserUpdated;

    public void Login(User user)
    {
        CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public void UpdateProfile(User updatedUser)
    {
        CurrentUser = updatedUser;

        // Kích hoạt Event (Hét lên).
        // Dấu ngoặc chấm hỏi (?.) nghĩa là: Nếu có ai đang nghe thì hét, không ai nghe thì thôi.
        OnUserUpdated?.Invoke();
    }
}
