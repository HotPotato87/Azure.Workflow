Azure.Workflow
==============

A fully extensible processing workflow library. 

Use it for :

1. Managing cloud-based processing, get informed with alerts and summeries. Schedule your processing how you want it.
2. Handling any processing pathway from handling orders from ecommerce system to importing data into your application.
3. Easily writing worker processes that take advantage of the reliability and scalability of cloud-based queues and deployment.

Features
---------
1. Fluent API. Build it how you love it.
```
await WorkflowSession.StartBuild()
                        .AddModule(new RottenTomatoesModule())
                        .AddModule(new MetacriticModule())
                    .WithQueueMechanism(new AzureServiceBusQueueFactory(new ServiceBusQueueSettings() { ConnectionString = DemoSettings.Default.ServiceBusConnectionString }))
                    .AttachLogger(new AzureStorageLogger())
                    .AttachAlertManager(new ProwlAlertManager())
                    .AttachReportGenerator(new SendGridReportGenerator())
                    .RunAsync();
```
 
2. Read and write with queues such as Azure servicebus or cloud-storage queues.

3. Alerting System. Get alerts on your phone or email instantaniously. 
 
4. 

5. Fully extensible framework and design, want to use Amazon ECS instead? Just hot-swap the components.
