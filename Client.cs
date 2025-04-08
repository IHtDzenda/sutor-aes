namespace SutorAes
{
  class Client
  {
    Guid id { get; set; }
    byte[] RSA_pub { get; set; }
    byte[] RSA_priv { get; set; }
    Dictionary<Guid, byte[]> AES { get; set; }
    public static byte[] Encrypt()
    {
      if (!AES.ContainsKey(targetId))
      {
        throw new ArgumentException("AES klíč pro zadané ID nebyl nalezen.");
      }

      byte[] aesKey = AES[targetId];

      using (var rsa = RSA.Create())
      {
        rsa.ImportRSAPrivateKey(RSA_priv, out _);

        byte[] encryptedData = rsa.Encrypt(aesKey, RSAEncryptionPadding.Pkcs1);
        return encryptedData;
      }
    }
  }
}

