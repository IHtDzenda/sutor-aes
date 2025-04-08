namespace SutorAes
{
  class Client
  {
    Guid id { get; set; }
    byte[] RSA_pub { get; set; }
    byte[] RSA_priv { get; set; }
    Dictionary<Guid, byte[]> AES { get; set; }

  }
}
