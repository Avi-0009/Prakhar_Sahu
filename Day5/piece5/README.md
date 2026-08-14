# Day 5, Piece 5: App Insights & KQL Verification

## KQL Query Execution
![KQL Result Screenshot](./kql-result.png)

**Observation:** 
Both endpoints received 9 requests. The /api/quotes endpoint had a p99 latency of ~198ms, while the /health endpoint was near-instant at ~0.34ms. This makes sense because /health just returns a basic status, whereas /api/quotes processes authentication middleware.

## Extra Credit

**What did you learn this session?**
I learned how to query OpenTelemetry data in Application Insights using KQL, and how to measure true endpoint performance using p50 and p99 percentiles.

**What would break this?**
If the container app loses the APPLICATIONINSIGHTS_CONNECTION_STRING environment variable during a deployment update, the telemetry pipeline breaks instantly.
