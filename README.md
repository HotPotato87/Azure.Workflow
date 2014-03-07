Servershot.Framework
==============

A fully extensible processing library. 

Use it for :

1. Managing asyncronous cloud-based processing, get informed with alerts and summeries. Schedule your processing how you want it with Quartz.net. Send data with ServiceBus. Persist entities with Tablestorage, or Azure Caching.
2. Any distributed system processing from handling orders of ecommerce systems to importing data into your application.
3. Easily writing worker processes that take advantage of the reliability and scalability of cloud-based queues and deployment.

Features
---------
<b>1. Fluent API. Build it how you love it.</b>
```
await WorkflowSession.StartBuild()
       .AddModule(new RottenTomatoesModule())
       .AddModule(new MetacriticModule())
   .WithQueueMechanism(new AzureServiceBusQueueFactory(new ServiceBusQueueSettings() { ConnectionString = ServiceBusConnectionString }))
   .AttachLogger(new AzureStorageLogger())
   .AttachAlertManager(new ProwlAlertManager())
   .AttachReportGenerator(new SendGridReportGenerator())
   .RunAsync();
```
 
<b>2. Read and write with queues such as Azure servicebus or cloud-storage queues. </b>

Send..
```
public class RottenTomatoesModule : InitialWorkflowModule<Movie>
{
    public override async Task OnStart()
    {
        List<Movie> movies = CreateMoviesFromRTApi();

        movies.ForEach(x => base.SendTo(typeof (MetacriticModule), x));
    }
```
Recieve..
```
public class MetacriticModule : QueueProcessingWorkflowModule<Movie>
{
    public override async Task ProcessAsync(IEnumerable<Movie> queueCollection)
    {
        foreach (var movie in queueCollection)
        {
            GetMetaScoreFromMetaCritic(movie);
            base.CategorizeResult(ProcessingResult.Success);
            base.SendTo(typeof(DatabaseStorageModule));
        }
    }
```

<b>3.  Alerting System. Get alerts on your phone or email instantaniously.  </b>

```
public override async Task ProcessAsync(IEnumerable<Movie> queueCollection)
{
    foreach (var movie in queueCollection)
    {
        try
        {
            ProcessMovie();
        }
        catch (Exception e)
        {
            base.RaiseAlert(AlertLevel.Medium, "Problem processing movie " + movie.Title);
        }
            
    }
```

<b>4. Get processing summaries sent to you on a schedule via email, text or alternatives. </b> 

```
.AttachReportGenerator(new SendGridReportGenerator())
```

<b>5. Fully extensible framework and design, want to use RabbitMQ instead of ServiceBus? Just hot-swap the components. There are plenty out of the box, but you can easily write your own. </b>

```
await WorkflowSession.StartBuild()
        .AddModule(new RottenTomatoesModule())
        .AddModule(new MetacriticModule())
    .WithQueueMechanism(new RabbitMQFactory())
```

