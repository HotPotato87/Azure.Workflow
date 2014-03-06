using System.Threading.Tasks;

namespace Servershot.WebsiteOrderSample.ExampleScenarios
{
    public interface IExampleScenario
    {
        string Description { get; }
        Task Run();
    }
}