using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using Microsoft.Xrm.Sdk.Client;

namespace ConfigurationWriter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the organization service url:");
            string orgUrl = Console.ReadLine();

            Console.WriteLine("Please select the endpoint type, 0:Active Directory, 1:Federation");
            string endpointtype = Console.ReadLine();

            AuthenticationProviderType authType = AuthenticationProviderType.ActiveDirectory;
            if (endpointtype.Equals("0"))
            {
                authType = AuthenticationProviderType.ActiveDirectory;
            }
            else if (endpointtype.Equals("1"))
            {
                authType = AuthenticationProviderType.Federation;
            }
            else
            {
                Console.WriteLine("The end point type must be 0 or 1");
                return;
            }

            string domain = "";
            if (authType == AuthenticationProviderType.ActiveDirectory)
            {
                Console.WriteLine("Please enter the domain:");
                domain = Console.ReadLine();
            }

            Console.WriteLine("Please enter the account name:");
            string name = Console.ReadLine();

            Console.WriteLine("Please enter the account password:");
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                Console.Write("*");
                password += info.KeyChar;
                info = Console.ReadKey(true);
            }
      
            TestPlugIn.CRMDecider plugin = new TestPlugIn.CRMDecider();
            plugin.DepolyConfiguration(new Uri(orgUrl),
                null,
                authType,
                domain,
                name,
                password);
            Console.Read();
        }
    }
}
