namespace CoffeePOS.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public int Role { get; set; } // 0: Admin, 1: Staff
}
