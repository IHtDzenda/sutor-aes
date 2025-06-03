using System;
using System.Text;
using SutorAes;

public class Program
{
    public static void Main()
    {
        Client client = new Client("user");
        Console.WriteLine(Convert.ToBase64String(client.RsaPub));
    }
}