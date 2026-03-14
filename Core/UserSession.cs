using CoffeePOS.Models;

namespace CoffeePOS.Core;

public interface IUserSession
{
    User? CurrentUser { get; }
    bool IsLoggedIn { get; }

    void Login(User user);
    void Logout();
}

public class UserSession : IUserSession
{
    public User? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser != null;

    public void Login(User user)
    {
        CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}
