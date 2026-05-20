namespace DAL.Entities;

public sealed class Role
{
    private readonly List<User> _users = [];

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<User> Users => _users;
}
