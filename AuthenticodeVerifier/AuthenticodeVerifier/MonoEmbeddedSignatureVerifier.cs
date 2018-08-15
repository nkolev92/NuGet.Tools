using System;
using System.Reflection;

namespace AuthenticodeVerifier
{
    class MonoEmbeddedSignatureVerifier
    {
        public bool IsValid(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("file path cannot be empty", nameof(filePath));
            }

            var assembly = Assembly.Load("Mono.Security, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = 0738eb9f132ed756");
            if (assembly != null)
            {
                var type = assembly.GetType("Mono.Security.Authenticode.AuthenticodeDeformatter");
                if (type != null)
                {
                    var instance = Activator.CreateInstance(type, filePath);
                    var method = type.GetMethod("IsTrusted");
                    if (method != null)
                    {
                        bool isTrusted = (bool)method.Invoke(instance, null);
                        LogReason(type, instance);
                        return isTrusted;
                    }
                }
            }

            Console.WriteLine("Either the assembly could not be loaded or the method cannot be found.");

            return false;
        }
        // Reference for the reason codes: https://github.com/mono/mono/blob/master/mcs/class/Mono.Security/Mono.Security.Authenticode/AuthenticodeDeformatter.cs
        private void LogReason(Type type, object instance)
        {
            var myField = type.GetProperty("Reason");
            if (myField != null)
            {
                Console.WriteLine("Reason {0}", myField.GetValue(instance));
            }
            else
            {
                Console.WriteLine("Reason cannot be found");
            }
        }
    }
}
