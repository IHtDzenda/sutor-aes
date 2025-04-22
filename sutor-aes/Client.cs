using System.Security.Cryptography;
using System.Text;
using System;

namespace SutorAes
{
  class Client
  {
    Guid id { get; set; }
    byte[] RsaPub { get; set; }
    byte[] RsaPriv { get; set; }
    Dictionary<Guid, byte[]> KnownHosts { get; set; } = new();
    private event Action<string>? handler;

    RSAEncryptionPadding padding = RSAEncryptionPadding.Pkcs1;

    public Client()
    {
        Guid id { get; set; }
        byte[] RsaPub { get; set; }
        byte[] RsaPriv { get; set; }
        Dictionary<Guid, byte[]> KnownHosts { get; set; } = new();
        private event Action<string>? handler;

        RSAEncryptionPadding padding = RSAEncryptionPadding.Pkcs1;

        public Client()
        {
          using (var ms = new MemoryStream())
          {
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                this.RsaPub = rsa.ExportRSAPublicKey();
                this.RsaPriv = rsa.ExportRSAPrivateKey();
            }
            return ms.ToArray();
          }
        }

        public byte[] EncryptMessage(Guid id, byte[] msg, byte[] IV){
          var key = KnownHosts[id];
          using (Aes aes = Aes.Create())
          {
              aes.Key = key;
              aes.IV = IV;
              aes.Padding = PaddingMode.PKCS7;
              aes.Mode = CipherMode.CBC;

              using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
              {
                  using (var ms = new MemoryStream())
                  {
                      using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                      {
                          cs.Write(msg, 0, msg.Length);
                          cs.Close();
                      }
                      return ms.ToArray();
                  }
              }
          }
        }

        public byte[] DecryptMessage(Guid id, byte[] msg, byte[] IV)
        {
            var key = KnownHosts[id];
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (var ms = new MemoryStream(msg))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var resultStream = new MemoryStream())
                            {
                                cs.CopyTo(resultStream);
                                return resultStream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        public void OnMessage(Action<string> handler)
        {
            this.handler += handler;
        }
        public void RecieveMessage(byte[] msg, byte[] IV)
        {
            var decryptedMessage = DecryptMessage(id, msg, IV);
            var message = Encoding.UTF8.GetString(decryptedMessage);
            handler?.Invoke(message);
        }
        public void SendMessage(Client reciever, string msg)
        {
            var IV = new byte[16];
            var encryptedMessage = EncryptMessage(id, Encoding.UTF8.GetBytes(msg), IV);
            reciever.RecieveMessage(encryptedMessage, IV);
        }

        public void EstablishConnection(Client other)
        {
          byte[] key =  new byte[16];

         System.Security.Cryptography.RandomNumberGenerator.Fill(key);
          other.RecieveConnection(this.id, key);
          this.KnownHosts.Add(other.id, key);
        }
        public void RecieveConnection(Guid id, byte[] key)
        {
            this.KnownHosts.Add(id, key);
        }
        
        // public byte[] EncryptRSA(byte[] data)
        // {
        //     using (RSA rsa = RSA.Create())
        //     {
        //         rsa.ImportRSAPublicKey(RsaPub, out _);
        //         return rsa.Encrypt(data, padding);
        //     }
        // }
        // 
        // public byte[] DecryptRSA(byte[] encryptedData)
        // {
        //     using (RSA rsa = RSA.Create())
        //     {
        //         rsa.ImportRSAPrivateKey(RsaPriv, out _);
        //         return rsa.Decrypt(encryptedData, padding);
        //     }
        // }
    }
    
    public byte[] DecryptRSA(byte[] encryptedData)
    {
        using (RSA rsa = RSA.Create())
        {
            User2Pubkey = new Dictionary<Guid, byte[]>();
        }
        
        public void RegisterUser(Guid id, byte[] rsaPub)
        {
            User2Pubkey.Add(id, rsaPub);
        }
        
        public Dictionary<Guid, byte[]> ListUsers()
        {
            return User2Pubkey;
        }
    }
}
