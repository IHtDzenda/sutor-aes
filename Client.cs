namespace SutorAes
{
  class Client
  {
    Guid id { get; set; }
    byte[] RSA_pub { get; set; }
    byte[] RSA_priv { get; set; }
    Dictionary<Guid, byte[]> AES { get; set; }

  }

  class Database
  {
    private Dictionary<Guid, byte[]> User2Pubkey { get; set; }

    public Database()
    {
      User2Pubkey = new Dictionary<Guid, byte[]>();
    }

    public void RegisterUser(Guid id, byte[] rsa_pub)
    {
      User2Pubkey.Add(id, rsa_pub);
    }

    public Dictionary<Guid, byte[]> ListUsers()
    {
      return User2Pubkey;
    }
  }
}
