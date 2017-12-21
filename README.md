# Workshop v3 Demo and Source Code

## Overview
This Workshop demo and accompanying source code is designed to help facilitate a deeper understanding of Pivotal Application Services, Spring Cloud Services, and Steeltoe.  The source code is based on two existing projects:
1. [Steeltoe Workshop](https://github.com/SteeltoeOSS/Workshop) developed by Dave Tillman
2. [PCF Exchange Demo and Workshop](https://github.com/pivotal-field-engineering/pcfechange-polyglot-demo) developed by Andrew Stakhov

A live demonstration can be found [here](https://workshopui-psoriatic-obstructionism.apps.islands.cloud/).

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
The following are required for building and deploying the applications in this workshop.

- [Cloud Foundry CLI](https://github.com/cloudfoundry/cli)
- [Git Client](https://git-scm.com/downloads)
- [.NET Core SDK 2.0](https://www.microsoft.com/net/download)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2017](https://www.visualstudio.com/downloads/)
- [Java 8 JDK](http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html) - Optional, needed to run Eureka and Config servers locally

## Design
While the Steeltoe workshop application helps demonstrate the functionality of Pivotal Application Services and Spring Cloud Services using Steeltoe, there was a need to expose some of the underlying workings of these applications running on PAS. This workshop and demo will help 
users better understand how PAS can help developers deploy at scale. Some additional techniques are used in the development of these applications such as utilizing service client libraries and shared model libraries. 

The following are some of the patterns used in development:

- Micro services using service client shared libraries
- Configuration Services to handle application configuration
- User Provided Services and Configurtion Services to handle service connections
- User and Service Security using OAuth and JWT methods
- Application support for Blue Green Deployments
- Continuous integration and deployment using Visual Studio Team Services

## Features
The workshop application can be navigated through the home page links through the various topics. The workshop can also be used as a demo for the capabilities of PAS. The following areas are designed to support the navigation of many of the features of PAS, SCS, and Steeltoe.

### Platform
The Platform section provides a demonstration of the platform's capabilities and enhancements provided by Steeltoe. Links to logging, tasks, metrics, and health accutators are included on the page to assist in navigation to key functionality.

### Configuration
The Configuration section provides a demonstration of configuration best practices and using steeltoe configuration services. Links to environment variables, the config server dashboard, the config server repository, and build information are included on the page to assist in navigation to key functionality.

### Services
The Services section provides a demonstration of service discovery and circuit breaker patterns. Redis is used to store the service counters. Service security is demonstrated through the SSO feature of the portal. If the user is not logged in, the service will respond with "You will have a nice day" 
Links to the Hystrix and Eureka dashboards are included on the page to assist in navigation to key functionality.

### Connections
The Connections section provides a demonstration of how to leverage user provided services and/or the config server to manage connection string data for services. This can be demonstrated by binding and unbinding the user provided service and by changing the environment for the config server.

### Zero Downtime
The Zero Downtime section provides a demonstration of a Blue/Green deployment. The code leverages Redis cache and config server to coordinate the cutover between applications by consitently showing the usage counters increment. The page will use colors to highlight the two applications participating on the mapped route.

### Security
The application provides the ability to log in and log out of the application using Single Sign-On. If a user is already logged into the Apps Manager, the user will be automatically signed into the Workshop application.

### CI/CD Pipeline using VSTS
The entire project is setup to demonstrate continuous delivery via CI/CD pipelines setup in Visual Studio Team Services. When a change is checked in by a developer, the pipeline will build the application, deploy the artifacts back into the github repository, and push the applications on to PAS.

## Projects
The following are the projects found in this repository and a short description of the functionality that each is designed to demonstrate.

### Workshop UI
Main demo application for the workshop

### Fortune Teller Service
Provides fortunes as a service used to demonstrate the circuit breaker and service discovery design patterns. 

### Market Data Service
Market data service used by the PCF Exchange demo/workshop.

### Order Manager Service
Order manager service used by the PCF Exchange demo/workshop.

### Exchange BTUSD Service
Currency conversion service used by the PCF Exchange demo/workshop.

### Exchange UI
Standalone web application for the PCF Exchange demo/workshop.

### Tweet Bunny Service
Provides sample console application that can leverage the Steeltoe services and connectors.

### Fortune Service Client
Provides a reusable client for accessing the fortune teller service.

### Order Manager Client
Provides a reusable client for accessing the order manager service.

### Pivotal Utilities
Common set of functions used by several applications.

## Installation Instructions
### Deploying through the VSTS Pipeline
This demo source code repository has been integrated into a CI/CD pipeline using Visual Studio Team Services. The build and release jobs developed leverage the Cloud Foundry plug-in to deploy the application on to Pivotal Cloud Foundry. 
The portal for the VSTS CI/CD pipeline can be found [here](https://pivotal-workshops.visualstudio.com/Workshop/). Access is required to the portal to view the job definitions.

### Installation Packages without Visual Studio
Installation packages that are ready to push are available in the Release section of this repository.  You can find them [here](https://github.com/corn-pivotal/Workshop-v3/releases).

### Cloning and Building the Solution
This project is developed using Visual Studio 2017. To build this solution, clone this repo and open the solution file. The projects can then be published and pushed from the publish folder. 

### Single Sign-On
The workshop application utilizes the internal UAC of PCF for user authorization and access. To complete configuration of UAC, the following steps will need to be completed. Existing users can be given access by adding the user to the security groups below.

- read.fortunes*
- read.exchange
- read.database
- write.database

`$ uaac member add read.fortunes {userid}`

*required

#### Step 1: Setting up UAAC
To complete the setup of security for the application, the cf-uaac program needs to be used. For Windows users, this can present a challenge as the utility runs on Linux. Windows 10 users can install the bash shell to complete the configuration.

Installing cf-uaac using Ruby and Gem:

`$ sudo apt install build-essential`

`$ sudo apt install ruby-dev`

`$ sudo apt install ruby`

`$ gem install cf-uaac`


#### Step 2: Configuring Application Security
To complete security configuration, use the cf-uaac command in the Linux shell to execute the following:

`$ uaac target uaa.sys.yourdomain.com --skip-ssl-validation`

`$ uaac token client get admin -s {admin password}`

`$ uaac add group read.fortunes`

`$ uaac user add fortuneadmin -p {password} --emails {email address}`

`$ uaac member add read.fortunes fortuneadmin`

`$ uaac client add myWorkshop --authorized_grant_types authorization_code,refresh_token --authorities uaa.resource --redirect_uri http://workshopui-*-*.apps.yourdomain.com/signin-cloudfoundry --autoapprove cloud_controller.read,cloud_controller_service_permissions.read,openid,read.fortunes,read.exchange,read.database,write.database --secret mySecret`

`$ uaac client update myWorkshop --scope read.fortunes,read.exchange,read.database,write.database,openid,cloud_controller.read,cloud_controller_service_permissions.read`


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

#### Services
To setup the services, use the batch command file in the [scripts folder](https://github.com/corn-pivotal/Workshop-v3/tree/master/scripts). You can also execute the following commands in a console window. See the connection string information section for more information on configuring the database.

##### Sample Entries
- myConfigServer

`> cf create-service p-config-server standard myConfigServer -c config-server.json`

- myDiscoveryService

`> cf create-service p-service-registry standard myDiscoveryService`

- myMySqlService

`> cf create-service p-mysql 100mb myMySqlService`

- myRedisService

`> cf create-service p-redis shared-vm myRedisService`

- myHystrixService

`> cf create-service p-circuit-breaker-dashboard standard myHystrixService`

- myRabbitMQService

`> cf create-service p-rabbitmq standard myRabbitMQService`

- myOAuthService

`> cf cups myOAuthService -p "{\"client_id\": \"myWorkshop\",\"client_secret\": \"mySecret\",\"uri\": \"uaa://login.system.testcloud.com\"}"`

- AttendeeContext

`> cf cups AttendeeContext -p "{\"connectionstring": \"{AttendeeContextConnectionString}\"}"`

#### Config Server
To complete the configuration, update the location of the Config Server repository using the cf CLI.

`> cf update-service myConfigServer -c {pathto/config.json}`


#### Connection Strings
The workshop application demonstrates the ability to load connection string information from both a user provided service and from the config server. In order for the application to function correctly for this demonstration, a SQL Server database 
needs to be setup to access. The following fields are required for the AttendeeContext database:

##### Database Schema
        int Id
        string Name
        string Email
        string Title
        string Department

##### ConnectionString
Configure the AttendeeContext connection string in the manifest.yml, appsettings.json, or update the user provided service.

