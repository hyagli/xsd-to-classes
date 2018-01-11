using System;
using System.Reflection;
using System.Runtime.InteropServices;
using BlueToque.XsdToClasses;

namespace Register
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("{0} {1}",
                    Assembly.GetAssembly(typeof(XsdCodeGenerator)).GetName().Name,
                    Assembly.GetAssembly(typeof(XsdCodeGenerator)).GetName().Version.ToString());
                if (args.Length==0 ||
                    args.Length==1 &&  args[0] == "/r")
                {
                    Console.Write("Registering XsdToClasses... ");
                    new RegistrationServices().RegisterAssembly(Assembly.GetAssembly(typeof(XsdCodeGenerator)), AssemblyRegistrationFlags.SetCodeBase);
                    Console.WriteLine("Success");
                }
                else if (args.Length == 1 && args[0] == "/u")
                {
                    Console.WriteLine("Unregistering XsdToClasses... ");
                    new RegistrationServices().UnregisterAssembly(Assembly.GetAssembly(typeof(XsdCodeGenerator)));
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Parameters: \r\n\t/r: register\r\n\t/u: unregister");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:\r\n{0}", ex);
            }
            finally
            {
                Console.WriteLine("Hit any key to continue");
                Console.Read();
            }
        }
    }
}
