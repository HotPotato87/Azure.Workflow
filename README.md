Azure.Workflow
==============

A fully extensible processing workflow library. 

Use it for :

1. Managing cloud-based processing, get informed with alerts and summeries. Schedule your processing how you want it.
2. Handling any processing pathway from handling orders from ecommerce system to importing data into your application.
3. Easily writing worker processes that take advantage of the reliability and scalability of cloud-based queues and deployment.

Features
---------
[1]. Fluent API. Build it how you love it.
```
await WorkflowSession.StartBuild()
       .AddModule(new RottenTomatoesModule())
       .AddModule(new MetacriticModule())
   .WithQueueMechanism(new AzureServiceBusQueueFactory())
   .AttachLogger(new AzureStorageLogger())
   .AttachAlertManager(new ProwlAlertManager())
   .AttachReportGenerator(new SendGridReportGenerator())
   .RunAsync();
```
 
[2]. Read and write with queues such as Azure servicebus or cloud-storage queues.

```
public class RottenTomatoesModule : InitialWorkflowModule<Movie>
{
    public override async Task OnStart()
    {
        List<Movie> movies = CreateMoviesFromRTApi();

        movies.ForEach(x => base.SendTo(typeof (MetacriticModule), x));
    }
```

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

[3]. Alerting System. Get alerts on your phone or email instantaniously. 

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

 4. Get processing summaries sent to you on a schedule via email, text or alternatives. 

```
.AttachReportGenerator(new SendGridReportGenerator())
```

 5. Fully extensible framework and design, want to use RabbitMQ instead? of ServiceBus? Just hot-swap the components. There are plenty out of the box, but you can easily write your own.

```
await WorkflowSession.StartBuild()
        .AddModule(new RottenTomatoesModule())
        .AddModule(new MetacriticModule())
    .WithQueueMechanism(new RabbitMQFactory())
```

