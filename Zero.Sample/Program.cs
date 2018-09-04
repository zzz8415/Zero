using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zero.Core.Wcf;

namespace Zero.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = Router<IHubService>.Instance.CreateChannel(2);
            c.Logout();
            Console.ReadLine();
        }
    }
}
