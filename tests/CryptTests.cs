using System;
using System.Text;
using NUnit.Framework;
using SutorAes;

namespace tests;

public class ProgramTests
{
    private byte[] _key;
    private byte[] _iv;

    [SetUp]
    public void Setup()
    {
        _key = new byte[32];
        Random random = new Random();
        random.NextBytes(_key);

        _iv = new byte[16];
        random.NextBytes(_iv);
    }

    [Test]
    public void EncryptDecrypt_ShouldReturnOriginalString()
    {
        string originalText = "This is a test message for encryption and decryption";

        byte[] encryptedData = Program.EncryptString(originalText, _key, _iv);
        string decryptedText = Program.DecryptString(encryptedData, _key, _iv);

        Assert.That(decryptedText, Is.EqualTo(originalText));
    }

    [Test]
    public void Encrypt_ShouldProduceDifferentOutput()
    {
        string originalText = "This is a test message";

        byte[] encryptedData = Program.EncryptString(originalText, _key, _iv);
        byte[] originalBytes = Encoding.UTF8.GetBytes(originalText);

        Assert.That(encryptedData, Is.Not.EqualTo(originalBytes));
    }

    [Test]
    public void EncryptDecrypt_WithEmptyString_ShouldWork()
    {
        string originalText = "";

        byte[] encryptedData = Program.EncryptString(originalText, _key, _iv);
        string decryptedText = Program.DecryptString(encryptedData, _key, _iv);

        Assert.That(decryptedText, Is.EqualTo(originalText));
    }

    [Test]
    public void EncryptString_WithNullInput_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Program.EncryptString(null, _key, _iv));
    }

    [Test]
    public void DecryptString_WithNullInput_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Program.DecryptString(null, _key, _iv));
    }
}