using Microsoft.EntityFrameworkCore;

public class Database : DbContext
{
    public DbSet<DBUser> Users { get; set; }
    public DbSet<DBInbox> Inbox { get; set; }

    private readonly string _dbPath;
    public Database(string dbPath = "./Database.db")
    {
        _dbPath = dbPath;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    public void RegisterUser(Guid userId, byte[]? rsaPublicKey)
    {
        var user = new DBUser
        {
            UserID = userId,
            RsaPublicKey = rsaPublicKey
        };
        Users.Add(user);
        SaveChanges();
    }
    public Dictionary<Guid, byte[]?> ListUsers()
    {
        var users = new Dictionary<Guid, byte[]?>();
        foreach (var user in Users)
        {
            users.Add(user.UserID, user.RsaPublicKey);
        }
        return users;
    }
    public List<byte[]?> GetMsgsByUserID(Guid userId)
    {
        var inbox = new List<byte[]?>();
        foreach (var message in Inbox.Where(m => m.UserID == userId))
        {
            inbox.Add(message.EncryptedMessage);
        }
        return inbox;
    }
    public void AddMsgToUser(Guid userId, byte[]? encryptedMessage)
    {
        var message = new DBInbox
        {
            UserID = userId,
            EncryptedMessage = encryptedMessage
        };
        Inbox.Add(message);
        SaveChanges();
    }
}