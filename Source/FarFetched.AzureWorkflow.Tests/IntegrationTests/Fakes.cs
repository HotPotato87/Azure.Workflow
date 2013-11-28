using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.Plugins;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;

namespace FarFetched.AzureWorkflow.Tests.IntegrationTests
{
    internal class Fakes
    {
        internal class AddsToQueueProcessingFake : InitialWorkflowModule<object>
        {
            private readonly List<object> _payload;
            private readonly Type _typeToSendTo;

            public AddsToQueueProcessingFake(List<object> payload, Type typeToSendTo)
            {
                _payload = payload;
                _typeToSendTo = typeToSendTo;
            }

            public override async Task OnStart()
            {
                base.Session.AddToQueue(_typeToSendTo, _payload);
            }
        }

        internal class RecievesFromQueueProcessingFake : QueueProcessingWorkflowModule<object>
        {
            public RecievesFromQueueProcessingFake()
            {
                
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.Recieved = queueCollection;

                this.Recieved.ToList().ForEach(x=>this.RaiseProcessed(ProcessingResult.Success));
            }

            public IEnumerable<object> Recieved { get; set; }
        }

        internal class QueueProcessingThowsErrorFake : QueueProcessingWorkflowModule<object>
        {
            private readonly Func<Exception> _exceptionFactory;

            public QueueProcessingThowsErrorFake(Func<Exception> exceptionFactory)
            {
                _exceptionFactory = exceptionFactory;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.Recieved = queueCollection;

                this.Recieved.ToList().ForEach(x => this.RaiseError(_exceptionFactory.Invoke()));
            }

            public IEnumerable<object> Recieved { get; set; }
        }

        internal class CategorisesProcessingResultFake : QueueProcessingWorkflowModule<object>
        {
            private readonly Func<Exception> _exceptionFactory;

            public CategorisesProcessingResultFake(Func<Exception> exceptionFactory)
            {
                _exceptionFactory = exceptionFactory;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.Recieved = queueCollection;

                this.RaiseProcessed(ProcessingResult.Success);
                this.RaiseProcessed(ProcessingResult.Success);
                this.RaiseProcessed(ProcessingResult.Success);

                this.RaiseProcessed(ProcessingResult.Fail, "Not enough chocolate");
                this.RaiseProcessed(ProcessingResult.Fail, "Not enough cheese");

                this.RaiseProcessed("Other", "Delivery API couldn't be contacted");
            }

            public IEnumerable<object> Recieved { get; set; }
        }

        internal class QueueProcessingThowsException : QueueProcessingWorkflowModule<object>
        {
            private readonly Func<Exception> _exceptionFactory;

            public QueueProcessingThowsException(Func<Exception> exceptionFactory)
            {
                _exceptionFactory = exceptionFactory;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                throw new Exception("Should throw");
            }

            public IEnumerable<object> Recieved { get; set; }
        }

        internal class LogsMessageFake : QueueProcessingWorkflowModule<object>
        {
            private readonly string _message;

            public LogsMessageFake(string message)
            {
                _message = message;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.LogMessage(_message);
            }
        }

        internal class AlertModuleFake : InitialWorkflowModule<object>
        {
            private readonly string _message;

            public AlertModuleFake(string message)
            {
                _message = message;
            }

            public override async Task OnStart()
            {
                this.RaiseAlert(AlertLevel.Low, _message);
            }
        }

        internal class ReportGenerationFake : ReportGenerationPlugin
        {
            public override void SendSessionReport(IEnumerable<ModuleProcessingSummary> moduleSummaries)
            {
                
            }
        }
    }
}
