
# Running the application

```
dotnet build
./ElasticApmTestClient/bin/Debug/net6.0/ElasticApmTestClient.exe {elastic-instace-id} {elastic-apm-bearer-token}
```

Replacing `{elastic-instace-id}` with your Elastic Cloud APM Server instance ID, and `{elastic-apm-bearer-token}` with your Elastic Cloud APM Server bearer token. You can find this information in Elastic Cloud under `Integrations > APM > APM Agents > OpenTelemetry`.
