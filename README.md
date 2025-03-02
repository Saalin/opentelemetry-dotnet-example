# OpenTelemetry .NET Demo  

[![.NET](https://github.com/Saalin/opentelemetry-dotnet-example/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Saalin/opentelemetry-dotnet-example/actions/workflows/dotnet.yml)

This project demonstrates the full integration of OpenTelemetry in .NET, covering logs, traces, and metrics. The collected telemetry data is exported to the `opentelemetry-collector`, which then forwards it to Loki, Tempo, and Prometheus. The data can be explored in Grafana, which is preconfigured to provide seamless navigation between logs and traces.  

## Features  

- **Comprehensive OpenTelemetry integration** – logs, traces, and metrics.  
- **Preconfigured exporters** – data is sent to Loki, Tempo, and Prometheus.  
- **Grafana integration** – includes cross-links between logs and traces.  
- **Sample endpoint** demonstrating:  
  - Built-in HttpClient instrumentation.  
  - Custom instrumentation with enhanced tracing features.  

## TODO  

- [ ] Provision a sample Grafana dashboard that visualizes the ingested telemetry data. 
