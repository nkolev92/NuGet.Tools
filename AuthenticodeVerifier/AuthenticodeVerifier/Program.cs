using System;

namespace AuthenticodeVerifier
{
    public class Program
    {

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You need to provide the full path to a path");
                return;
            }

            var filePath = args[0];

            if (args.Length >= 2)
            {
                var verifier = new WindowsEmbeddedSignatureVerifier();
                Console.WriteLine("Windows IsTrusted {0}", verifier.IsValid(filePath));
            } else
            {
                var verifier = new MonoEmbeddedSignatureVerifier();
                Console.WriteLine("Mono IsTrusted {0}", verifier.IsValid(filePath));
            }


        }
    }
}
