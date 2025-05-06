using System.Security.Cryptography;
using System.Text;
using System;

namespace SutorAes
{
    class Client
    {
        Guid Id { get; set; }
        string Name { get; set; }

        byte[] RsaPub { get; set; }
        byte[] RsaPriv { get; set; }
        Dictionary<Guid, byte[]> KnownHosts { get; set; } = new();
        private event Action<string>? Handler;

        RSAEncryptionPadding padding = RSAEncryptionPadding.Pkcs1;

        public Client()
        {
            GenerateRSAKeys();
        }

        private void GenerateRSAKeys()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                this.RsaPub = rsa.ExportRSAPublicKey();
                this.RsaPriv = rsa.ExportRSAPrivateKey();
            }
        }

        public byte[] EncryptMessage(Guid id, byte[] msg, byte[] IV)
        {
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
            this.Handler += handler;
        }
        public void RecieveMessage(byte[] msg, byte[] IV)
        {
            var decryptedMessage = DecryptMessage(Id, msg, IV);
            var message = Encoding.UTF8.GetString(decryptedMessage);
            Handler?.Invoke(message);
        }
        public void SendMessage(Client reciever, string msg)
        {
            var IV = new byte[16];
            RandomNumberGenerator.Fill(IV);
            var encryptedMessage = EncryptMessage(Id, Encoding.UTF8.GetBytes(msg), IV);
            reciever.RecieveMessage(encryptedMessage, IV);
        }

        public void EstablishConnection(Client other)
        {
            byte[] key = new byte[16];

            System.Security.Cryptography.RandomNumberGenerator.Fill(key);
            other.RecieveConnection(this.Id, key);
            this.KnownHosts.Add(other.Id, key);
        }
        public void RecieveConnection(Guid id, byte[] key)
        {
            this.KnownHosts.Add(id, key);
        }

        public byte[] EncryptRSA(byte[] data)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(RsaPub, out _);
                return rsa.Encrypt(data, padding);
            }
        }

        public byte[] DecryptRSA(byte[] encryptedData)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(RsaPriv, out _);
                return rsa.Decrypt(encryptedData, padding);
            }
        }
    }

    class Database
    {
        private Dictionary<Guid, byte[]> User2Pubkey { get; set; }

        public Database()
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
