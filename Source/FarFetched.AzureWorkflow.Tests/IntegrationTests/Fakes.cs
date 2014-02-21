using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Plugins.Alerts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServerShot.Framework.Tests.IntegrationTests
{
    internal class Fakes
    {
        internal class AddsToQueueProcessingFake : InitialServerShotModule<object>
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
                await Task.Delay(10);
            }
        }

        internal class ThrowsErrorModule : InitialServerShotModule<object>
        {
            private readonly Exception _e;
            private readonly int _times;
            private readonly List<object> _payload;
            private readonly Type _typeToSendTo;

            public ThrowsErrorModule(Exception e, int times, ServerShotModuleSettings settings = null)
                : base(settings)
            {
                _e = e;
                _times = times;
            }

            public override async Task OnStart()
            {
                for (int i = 0; i < _times; i++)
                {
                    this.RaiseError(_e);
                }
            }
        }

        internal class RecievesFromQueueProcessingFake : QueueProcessingServerShotModule<object>
        {
            public RecievesFromQueueProcessingFake()
            {
                
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.Recieved = queueCollection;

                this.Recieved.ToList().ForEach(x=>this.CategorizeResult(ProcessingResult.Success));
            }

            public IEnumerable<object> Recieved { get; set; }
        }

        internal class QueueProcessingThowsErrorFake : QueueProcessingServerShotModule<object>
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

        internal class CategorisesProcessingResultFake : QueueProcessingServerShotModule<object>
        {
            private readonly List<Tuple<object, string>> _processMessages;

            public CategorisesProcessingResultFake(List<Tuple<object, string>> processMessages)
            {
                _processMessages = processMessages;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.Recieved = queueCollection;

                _processMessages.ForEach(x=>base.CategorizeResult(x.Item1, x.Item2));

                await Task.Delay(10);
            }

            public IEnumerable<object> Recieved { get; set; }
        }

        internal class QueueProcessingThowsException : QueueProcessingServerShotModule<object>
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

        internal class LogsMessageFake : QueueProcessingServerShotModule<object>
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

        internal class AlertModuleFake : InitialServerShotModule<object>
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
            internal ServerShotSession Session { get; private set; }

            public override void SendSessionReport(ServerShotSession ServerShotSession, IEnumerable<ModuleProcessingSummary> moduleSummaries)
            {
                this.Session = ServerShotSession;
            }
        }
    }
}
