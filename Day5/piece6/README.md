### Exercise
**HttpClient + Resilience Handler Config:**
\\\csharp
builder.Services.AddHttpClient("ExternalService")
    .AddResilienceHandler("default", b =>
    {
        b.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        })
        .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 2
        })
        .AddTimeout(TimeSpan.FromSeconds(10));
    });
\\\

**Test (Forced Transient Failure) & Retry Logs:**
Triggered via an endpoint calling https://httpstat.us/500. The logs show the retries executing, followed by the circuit breaker opening to prevent further failing calls.
\\\	ext
[21:02:39 INF] Start processing HTTP request GET https://httpstat.us/500
[21:02:40 INF] Sending HTTP request GET https://httpstat.us/500
[21:02:44 WRN] Execution attempt. Source: 'ExternalService-default//Retry', Operation Key: 'null', Result: 'An error occurred while sending the request.', Handled: 'True', Attempt: '0', Execution Time: 4106.8595ms
[21:02:44 WRN] Resilience event occurred. EventName: 'OnRetry', Source: 'ExternalService-default//Retry', Operation Key: 'null', Result: 'An error occurred while sending the request.'
[21:02:46 ERR] Resilience event occurred. EventName: 'OnCircuitOpened', Source: 'ExternalService-default//CircuitBreaker', Operation Key: 'null', Result: 'An error occurred while sending the request.'
[21:02:47 INF] Execution attempt. Source: 'ExternalService-default//Retry', Operation Key: 'null', Result: 'The circuit is now open and is not allowing calls.', Handled: 'False', Attempt: '1'
\\\

### GitHub link
https://github.com/thinkbridge-thinkschool/your-repo/tree/feature/day5-piece6/Day5/piece6

### What did you learn this session?
I learned how to use .NET 8's Microsoft.Extensions.Http.Resilience package to wrap HttpClients with Polly v8 strategies. Combining jittered exponential backoffs and circuit breakers protects downstream services from cascading failures and prevents resource exhaustion.

### What would break this?
A downstream service that returns a 200 OK status code but includes an error payload in the JSON body (a "soft error"). The default HTTP resilience handler only evaluates HTTP 5XX codes or network timeouts, so it would silently accept the soft error without triggering a retry or opening the circuit.
