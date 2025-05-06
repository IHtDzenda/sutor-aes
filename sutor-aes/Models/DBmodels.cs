using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DBUser{
    [Key]
    public Guid UserID { get; set; }
    public byte[]? RsaPublicKey { get; set; }
}

public class DBInbox{
    [Key]
    public int Id { get; set; }
    [ForeignKey("UserID")]
    public DBUser? User { get; set; }
    public Guid UserID { get; set; }
    public byte[]? EncryptedMessage { get; set; }
}