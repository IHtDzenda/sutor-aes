namespace SutorAes
{

  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
    }
    public static byte[] EncryptString(string plainText, byte[] key, byte[] iv)
{
    if (plainText == null || plainText.Length <= 0)
    {
        throw new ArgumentNullException(nameof(plainText));
    }
    if (key == null || key.Length <= 0)
    {
        throw new ArgumentNullException(nameof(key));
    }
    if (iv == null || iv.Length <= 0)
    {
        throw new ArgumentNullException(nameof(iv));
    }
    
    using (Aes aesAlg = Aes.Create())
    {
        aesAlg.Key = key;
        aesAlg.IV = iv;
        
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        
        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
            }
            // Return the encrypted bytes directly without Base64 encoding
            return msEncrypt.ToArray();
        }
    }
}
