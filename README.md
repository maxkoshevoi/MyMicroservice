# MyMicroservice

Simple ASP.NET 5 microservice. Includes tests, health checks with UI, structured logging, distributed tracing, and feature management

## Useful links

- [Microsoft learn: Microservices](https://docs.microsoft.com/en-us/users/dotnet/collections/8mq4i2mzgjwn10)
- [Video that demonstrates most of the things implemented here](https://www.youtube.com/watch?v=PDdHa0ushJ0)

## Docker

Docker is used to create `images` that contain everything that is needed to run the app. Images are created using instructions listed in `Dockerfile`. Here's an example build command:

`docker build -f "MyMicroservice\Dockerfile" .`

- Dockerfile can contain multiple stages (each starts with keyword `FROM`).
- Instructions for each stage executed top to bottom.
- Only stages that "final" stage depends on are executed.
- During **Debug** build Visual Studio only executes "base" stage. Visual Studio handles the rest of the process without regard to the contents of the Dockerfile. If you want to disable the performance optimization and build as the Dockerfile specifies, then set the `ContainerDevelopmentMode` property to `Regular` in the project file (or docker compose project file, if you use it).
- Multistage build also helps to reduce image size. For example, you don't need whole .Net SDK to run the app, but you need it to build it, so "build" stage can use `sdk` image to build the app, and "final" stage can use `runtime` image and just copy artifacts of the "build" stage.
- Each instruction doesn't get executed every time you create new image. Docker tracks what changed from the previous run and only executes instructions after first updated file is copied. That's why "restore" steps are done separately from build. This optimization [works only for local builds and self-hosted agents](https://docs.microsoft.com/en-us/azure/devops/pipelines/ecosystems/containers/build-image?view=azure-devops#is-reutilizing-layer-caching-during-builds-possible-on-azure-pipelines) though.
- Azure Pipelines can be configured to [cache Docker images](https://docs.microsoft.com/en-us/azure/devops/pipelines/release/caching?view=azure-devops#docker-images), but most common ones are already [pre-cached](https://docs.microsoft.com/en-us/azure/devops/pipelines/ecosystems/containers/build-image?view=azure-devops#what-pre-cached-images-are-available-on-hosted-agents).

## WSL

On Windows Docker containers are being run in WSL (Windows Linux Subsystem). It has it's own process (`Vmmem`) which has tendency to consume as much memory as possible.

- You can stop the subsystem using `wsl --shutdown`, but then you'll need to restart the Docker to continue using it.
- You can limit amount of memory WSL can consume by creating [.wslconfig](https://docs.microsoft.com/en-us/windows/wsl/wsl-config#configure-global-options-with-wslconfig) file in user's folder that looks as follows:

```txt
[wsl2]
memory=4GB # Limits VM memory in WSL 2 to 4 GB
```

## Docker compose

Compose is a tool for defining and running multi-container Docker applications. With Compose, you use a YAML file to configure your application’s services. Then, with a single command, you create and start all the services from your configuration.

- Docker compose is used to configure and start multiple containers at the same time. It doesn't provide any advanced orchestration support (restarting container if it fails, auto-scaling, etc.), so it's mainly used for local development and testing.
- To build all your images use `docker compose build`, and `docker compose up` to build and start them.
- If you need to call one container from another one, you can call it by it's name specified in `docker-compose.yml` file (eg: http:\\\\mycontainer). You don't need to expose any ports from `mycontainer` to do that.
- There can be multiple `docker-compose` files, each adding or updating configuration form the previous one. By default there are `docker-compose.yml` and `docker-compose.override.yml`. You can think of them as `appsettings.json` and `appsettings.Development.json`.
- If you need to pass the same environment variable (with the same value) into multiple containers, create `.env` file with key-value pairs, and specify default values for needed variables: `ASPNETCORE_ENVIRONMENT=Development`. After that you can omit value for variables in `docker-compose` file:

```yml
services:
  mymicroservice:
    environment:
      - ASPNETCORE_ENVIRONMENT
```

## Kubernetes

Production-Grade Container Orchestration

Kubernetes, also known as K8s, is an open-source system for automating deployment, scaling, and management of containerized applications.

- Use this command to [convert docker-compose file into Kubernetes deployment](https://kubernetes.io/docs/tasks/configure-pod-container/translate-compose-kubernetes/): `kompose -f docker-compose.yml -f docker-compose.override.yml convert`
- Kubernetes can launch multiple instances of the same image, so you need to create a load balancer to access any container. Internal load balancer is called `Service`, and external one is called `LoadBalancer`.

## Telemetry

### Logging

Structured logging uses a defined format to add important details to logs and make it easier to interact with them in different ways. The default layout for many types of application logs is a plain text layout, which is easily readable for humans but difficult to interact with for machines. Structured logging takes plain text application logs and converts them into a set of data points that can be more easily analyzed by a machine.

`Common` project from this PoC contains all needed pieces to add structured logging to an ASP.NET application.

0. (optional) Add your Application insights instrumentation key in `.env` file.
1. Add Serilog: In `Program.cs` add `.AddTelemetry(<app-name>)` right after `Host.CreateDefaultBuilder(args)`. This will initialize Serilog with sinks to console, [Seq](https://datalust.co/seq), and app insights.
2. Catch apps crashes: In `Program.cs` replace `CreateHostBuilder(args).Build()` with `Telemetry.SafeRun(() => CreateHostBuilder(args).Build());`
3. Enrich HTTP request logs: In `Startup.Configure` add `app.UseTelemetry()` before any handlers whose activities should be logged (like `UseRouting` or `UseEndpoints`).

### Tracing

Distributed tracing, also called distributed request tracing, is a method used to profile and monitor applications, especially those built using a microservices architecture. Distributed tracing helps pinpoint where failures occur and what causes poor performance.

Distributed tracing allows to obtain information about full request path even it crossed multiple services.

`Common` project from this PoC contains all needed pieces to add distributed tracing to an ASP.NET application.

0. (optional) Add your Application insights instrumentation key in `.env` file.
1. Add trace logging: In `Startup.ConfigureServices` add `services.AddTelemetry(<app-name>)`. This will add Application Insights and Open Telemetry (with [Zipkin](https://zipkin.io) exporter) tracing.

#### OpenTelemetry

[OpenTelemetry](https://opentelemetry.io) provides a single, open source standard and a set of technologies to capture and export metrics, traces, and logs (in the future) from your cloud-native applications and infrastructure.

OpenTelemetry provides you with:

- A single, vendor-agnostic instrumentation library per language with support for both automatic and manual instrumentation.
- A single collector binary that can be deployed in a variety of ways including as an agent or gateway.
- An end-to-end implementation to generate, emit, collect, process and export telemetry data.
- Full control of your data with the ability to send data to multiple destinations in parallel through configuration.
- Open-standard semantic conventions to ensure vendor-agnostic data collection
- The ability to support multiple context propagation formats in parallel to assist with migrating as standards evolve.
- A path forward no matter where you are on your observability journey. With support for a variety of open-source and commercial protocols, format and context propagation mechanisms as well as providing shims to the OpenTracing and OpenCensus projects, it is easy to adopt OpenTelemetry.

OpenTelemetry **is not** an observability back-end like Jaeger or Prometheus. Instead, it supports exporting data to a variety of open-source and commercial back-ends. It provides a pluggable architecture so additional technology protocols and formats can be easily added.

#### Application Insights

[Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview), a feature of Azure Monitor, is an extensible Application Performance Management (APM) service for developers and DevOps professionals. Use it to monitor your live applications. It will automatically detect performance anomalies, and includes powerful analytics tools to help you diagnose issues and to understand what users actually do with your app.

- [More info on server-side monitoring](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)
- [How to setup client-side client-side](https://docs.microsoft.com/en-us/azure/azure-monitor/app/javascript)

## Health checks

[Health checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks) are usually used with an external monitoring service or container orchestrator to check the status of an app.

[Kubernetes users `liveness`, `readiness` and `startup` probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/) to more effectively manage the cluster:
- The `kubelet` uses liveness probes to know when to restart a container. For example, liveness probes could catch a deadlock, where an application is running, but unable to make progress. Restarting a container in such a state can help to make the application more available despite bugs.
- The `kubelet` uses readiness probes to know when a container is ready to start accepting traffic. A Pod is considered ready when all of its containers are ready. One use of this signal is to control which Pods are used as backends for Services. When a Pod is not ready, it is removed from Service load balancers.
- The `kubelet` uses startup probes to know when a container application has started. If such a probe is configured, it disables liveness and readiness checks until it succeeds, making sure those probes don't interfere with the application startup. This can be used to adopt liveness checks on slow starting containers, avoiding them getting killed by the kubelet before they are up and running.

`Common` project from this PoC contains all needed pieces to add health checks to an ASP.NET application.

1. Add basic check: add `.AddSelfCheck()` after `services.AddHealthChecks()`
2. Also add checks for dependencies of your app.
For example, here's check that calls health check one some other REST endpoint, and another one that verifies redis instance is up:
```cs
.AddUrlGroup(new Uri("http://mymicroservice:80/readiness"), name: "backendapi-check")
.AddRedis(redisConnectionStr, name: "redis-check")
```
3. Expose health checks: In `app.UseEndpoints` add `endpoints.MapHealthChecks()` this will expose `liveness` check that verifies that app is running, and `readiness` check that accumulates all configured health checks.

You can also quickly create an application that would monitor health of all your services. See `HealthCheckUI` project for reference.

## Feature management

Traditionally, shipping a new application feature requires a complete redeployment of the application itself. Testing a feature often requires multiple deployments of the application. Each deployment may change the feature or expose the feature to different customers for testing.

Feature management is a modern software-development practice that decouples feature release from code deployment and enables quick changes to feature availability on demand. It uses a technique called feature flags (also known as feature toggles, feature switches, and so on) to dynamically administer a feature's lifecycle.

- Feature management allows you to modify behavior of the whole app on the fly (without redeploying or restarting it). [You can use flag checks, controller actions, MVC filters, or custom middleware](https://docs.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core?tabs=core5x#use-dependency-injection-to-access-ifeaturemanager) switch behavior depending on feature flag's value.
- Azure App Configuration provides [feature manager](https://docs.microsoft.com/en-us/azure/azure-app-configuration/manage-feature-flags) as a centralized endpoint for your feature flag values.

`Common` project from this PoC contains all needed pieces to add feature management to an ASP.NET application.

0. (optional) Add your Azure App Configuration endpoint in `.env` file.
1. Add feature management SDK: In `Startup.ConfigureServices` add `services.AddAzureFeatureManagement()`.
2. Connect to Azure App Configuration. In `Program.cs` add `.AddAzureFeatureManagement()` right after `Host.CreateDefaultBuilder(args)`.
3. Add middleware to refresh feature flags values: In `Strtup.Configure` add `app.UseAzureFeatureManagement()`.