# Workshop v3 Demo and Source Code

## Overview
This Workshop demo and accompanying source code is designed to help facilitate a deeper understanding of Pivotal Application Services, Spring Cloud Services, and Steeltoe.  The source code is based on two existing projects:
1. [Steeltoe Workshop](https://github.com/SteeltoeOSS/Workshop) developed by Dave Tillman
2. [PCF Exchange Demo and Workshop](https://github.com/pivotal-field-engineering/pcfechange-polyglot-demo) developed by Andrew Stakhov

### Technology
This application is intended to be deployed using Pivotal Application Services on Cloud Foundry. The source code for this project is implemented using dependency injection in .NET Core v2 and the following technologies:

1. Steeltoe v2 Open Source Libraries
2. Syncfusion UI Controls
3. Configuration Server
4. Eureka Discovery Server
5. Hystrix Circuit Breakers
6. RabbitMQ Messaging
7. Redis Caching
8. Pivotal UAC/SSO
9. MySQL Connector
10. Microsoft SQL Server
11. User Provided Services

### Requirements

## Patterns

## Features

### Platform

### Configuration

### Services

### Connections

### Zero Downtime

### Security

### CI/CD Pipeline using VSTS

## Projects

### Workshop UI

### Fortune Teller Service

### Market Data Service

### Order Manager Service

### Exchange BTUSD Service

### Exchange UI

### Tweet Bunny Service



## Installation Instructions

### Installation Packages without Visual Studio
Installation packages that are ready to push are available in the Release section of this repository.  You can find them [here](https://github.com/corn-pivotal/Workshop-v3/releases).

### Cloning and Building the Solution
This project is developed using Visual Studio 2017. To build this solution, clone this repo and open the solution file. The projects can then be published and pushed from the publish folder. 

### Single Sign-On

#### Setting up UAAC

### Configuration

#### Environment Variables
Environment variables are used to configure the Workshop UI. The initial set of variables can be set in the manifest file of the project prior to pushing the Workshop UI application.

- ASPNETCORE_ENVIRONMENT: Environment to load using Config Server
- AppsManagerUrl: URI for Apps Manager Portal
- AppBaseUrl: Base URI for Apps Manager Applications
- EurekaDashboardUrl: Eureka Dashboard URI
- HystrixDashboardUrl: Hystrix Dashboard URI
- OpsManagerUrl: Ops Manager Portal URI
- ConfigServerUrl: Config Server URI
- ConfigRepoUrl: Config Server Repo Location URI
- PCFMetricsUrl: PCF Metrics URI
- ExchangeUrl: PCF Exchange Demo URI
- GithubRepoUrl: Workshop Source Repo URI

##### Sample Entries
    ASPNETCORE_ENVIRONMENT: Production
    AppsManagerUrl: https://apps.sys.islands.cloud
    AppBaseUrl: https://apps.sys.islands.cloud/organizations/21c9ef98-0008-4f6c-a9a8-e7183869992d/spaces/c6781755-3f7f-411f-9f80-8bcd5dc6c04e/applications/
    EurekaDashboardUrl: https://eureka-c177404f-f62e-4747-9a2b-e617b7301a86.apps.islands.cloud/
    HystrixDashboardUrl: https://hystrix-9e3d8c20-ccea-4484-90c3-b1fb316b5682.apps.islands.cloud/hystrix/monitor?stream=https%3A%2F%2Fturbine-9e3d8c20-ccea-4484-90c3-b1fb316b5682.apps.islands.cloud%2Fturbine.stream
    OpsManagerUrl: https://opsmgr.islands.cloud
    ConfigServerUrl: https://spring-cloud-broker.apps.islands.cloud/dashboard/p-config-server/94210da6-7cee-4879-a97b-cd2750e1c1d4
	ConfigRepoUrl: https://github.com/corn-pivotal/config-repo
    PCFMetricsUrl: https://metrics.sys.islands.cloud/apps/
    ExchangeUrl: https://exchangeui-alcidine-trichinization.apps.islands.cloud/
    GithubRepoUrl: https://github.com/corn-pivotal/Workshop-v3/

#### Config Server

#### Services

#### Connection Strings
