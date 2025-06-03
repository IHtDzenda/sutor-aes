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
    public void EstablishConnection_SendMessage()
    {
        string originalText = "This is a test message for encryption and decryption";

        Client bob = new Client("Bob");
        Client alice = new Client("Alice");

        bob.EstablishConnection(alice);

        bool recieved = false;
        alice.OnMessage((string msg) => recieved = msg == originalText);
        bob.SendMessage(alice, originalText);
        Assert.That(recieved);
    }
}
