using System.Security.Cryptography;
using System.Text;
namespace SutorAes
{
    class Client
    {
        Guid id { get; set; }
        byte[] RSA_pub { get; set; }
        byte[] RSA_priv { get; set; }
        Dictionary<Guid, byte[]> KnownHosts { get; set; }
        
        public Client()
        {
            GenerateRSAKeys();
        }
        
        private void GenerateRSAKeys()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                RSA_pub = rsa.ExportRSAPublicKey();
                RSA_priv = rsa.ExportRSAPrivateKey();
            }
        }
        
        public byte[] EncryptRSA(byte[] data)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(RSA_pub, out _);
                return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
            }
        }
        
        public byte[] DecryptRSA(byte[] encryptedData)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(RSA_priv, out _);
                return rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
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