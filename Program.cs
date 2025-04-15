namespace SutorAes
{

  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
    }
    public static string EncryptMsg(string plainText, string key, string iv)
  {
    if (plainText == null  plainText.Length <= 0)
    {
        throw new ArgumentNullException(nameof(plainText));
    }
    if (key == null  key.Length <= 0)
    {
        throw new ArgumentNullException(nameof(key));
    }
    if (iv == null || iv.Length <= 0)
    {
        throw new ArgumentNullException(nameof(iv));
    }

    // Convert string key and IV to byte arrays
    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
    byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

    // Ensure key and IV are the correct size
    using (SHA256 sha256 = SHA256.Create())
    {
        keyBytes = sha256.ComputeHash(keyBytes);  // Get 32 bytes for key (256 bits)
    }

    // Take first 16 bytes for IV (128 bits)
    if (ivBytes.Length < 16)
    {
        Array.Resize(ref ivBytes, 16);  // Pad if too short
    }
    else if (ivBytes.Length > 16)
    {
        Array.Resize(ref ivBytes, 16);  // Truncate if too long
    }

    // Get plaintext bytes
    byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

    using (Aes aesAlg = Aes.Create())
    {
        aesAlg.Key = keyBytes;
        aesAlg.IV = ivBytes;

        // Create encryptor
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        // Perform the encryption in one step
        byte[] encrypted = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);

        // Return as Base64 string
        return Convert.ToBase64String(encrypted);
    }
}
  }

}
