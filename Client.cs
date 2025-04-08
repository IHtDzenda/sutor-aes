namespace SutorAes
{
  class Client
  {
    Guid id { get; set; }
    byte[] RSA_pub { get; set; }
    byte[] RSA_priv { get; set; }
    Dictionary<Guid, byte[]> KnownHosts { get; set; }

    private byte[] DecryptMsg(byte[] msg, Guid id, byte[] IV)
    {
      using(var aes = AES.Create())
      {
        aes.Key = KnownHosts[id];
        aes.IV = IV;

        return aes.CreateDecryptor().TransformFinalBlock(msg, 0, msg.Lenght);
      }
    }
  }
}
