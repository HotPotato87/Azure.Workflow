using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Implementation.Reporting;
using Servershot.WebsiteOrderSample.ExampleScenarios;

namespace Servershot.WebsiteOrderSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => StartConsole());

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        private static async void StartConsole()
        {
            try
            {
                Console.WriteLine("Welcome to the website order servershot demo. There are several scenarios to choose from");

                var scenarios = GetScenarios();
                Console.WriteLine("\r\n");

                int i = 1;
                foreach (var scenario in scenarios)
                {
                    Console.WriteLine("Scenario {0}", i);
                    Console.WriteLine("-----------------");
                    Console.WriteLine("{0}", scenario.Description);
                    Console.WriteLine("\r\n");
                    i++;
                }


                Console.WriteLine("\r\n");
                Console.WriteLine("What scenario would you like to load?");

                var response = int.Parse(Console.ReadLine());

                var scenarioToLoad = scenarios[response - 1];

                await scenarioToLoad.Run();

                StartConsole();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                StartConsole();
            }
        }

        private static List<IExampleScenario> GetScenarios()
        {
            return new List<IExampleScenario>(new IExampleScenario[]
            {
                new Scenario1(),
                new Scenario2(),
                new Scenario3(), 
                new Scenario4(),
                new Scenario5(),
                new Scenario6()
            });
        }
    }
}
