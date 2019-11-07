using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace TCSlackbot.Logic
{
    public class DataProtection
    {
        public byte[] toEncrypt = UnicodeEncoding.ASCII.GetBytes("UserId");

        public DataProtection(string userId)
        {
            this.toEncrypt = UnicodeEncoding.ASCII.GetBytes(userId);
        }
        public string Encrypt(byte[] Buffer, MemoryProtectionScope Scope)
        {
            if (Buffer == null)
                throw new ArgumentNullException("Buffer");
            if (Buffer.Length <= 0)
                throw new ArgumentException("Buffer");


            // Encrypt the data in memory. The result is stored in the same array as the original data.
            ProtectedMemory.Protect(Buffer, Scope);


        }
        public static byte[] CreateRandomEntropy()
        {
            // Create a byte array to hold the random value.
            byte[] entropy = new byte[16];

            // Create a new instance of the RNGCryptoServiceProvider.
            // Fill the array with a random value.
            new RNGCryptoServiceProvider().GetBytes(entropy);

            // Return the array.
            return entropy;


        }

    }
    
}
