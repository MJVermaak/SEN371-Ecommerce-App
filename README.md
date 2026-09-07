# SEN371-Ecommerce-App
A full-stack, cross-platform e-commerce web application built using MVC architecture, RESTful APIs, and Test-Driven Development (TDD) for SEN371.

# SEN371 - Full-Stack E-Commerce Web Application

## Overview
A responsive, cross-platform e-commerce platform developed as part of the Software Engineering 371 (SEN371) module. This project follows an Agile development methodology with iterative sprints, Test-Driven Development (TDD), and an MVC architecture to ensure high maintainability, robust API security, and seamless user experience.

### Key Features & Architecture
The solution is divided into four distinct projects enforcing inward-pointing dependencies:

- **GrandmastersHub.Domain**: Contains the core enterprise logic, entity definitions (e.g., User, Cart, Product), and repository interfaces. This layer has zero external dependencies.

- **GrandmastersHub.Application**: Coordinates system use cases. Contains business services, validation logic, and Data Transfer Objects (DTOs) to shape API requests and responses.

- **GrandmastersHub.Infrastructure**: Handles external concerns, specifically the SQL Server database connection, Entity Framework Core configurations, and the physical implementation of the repository interfaces.

- **GrandmastersHub.Api**: The RESTful HTTP entry point. Contains the controllers (prefixed with /api/v1/), Swagger UI configuration, and middleware for global error handling.


## Prerequisites
Ensure the following tools are installed on your local development environment before proceeding:
- .NET 10.0 SDK or later
- Visual Studio 2022 (or VS Code)
- SQL Server (Developer or Express edition)
- Git

## Local Setup & Execution - Run these commands one by one
1. Clone the Repository:
- ```git clone https://github.com/MJVermaak/SEN371-Ecommerce-App.git```
- ```cd SEN371-Ecommerce-App/GrandmastersHub```

2. Restore Dependencies:
Navigate into the API project folder and restore the required NuGet packages (including Swashbuckle for Swagger UI).
- ```cd GrandmastersHub/GrandmastersHub.Api```
- ```dotnet restore```

3. Run the Server:
Launch the development server. By default, the application will listen on HTTP port 5188.
- ```dotnet run```

Test the Endpoints:
Once the server is running, open a web browser and navigate to the Swagger UI dashboard to interact with the API endpoints visually:
- `http://localhost:5188/swagger`

## Database Migrations
Note: The automated database migration check on startup is currently disabled to allow frontend and API routing tests without a local SQL Server instance.
When the database schema is ready to be generated or updated, execute the following commands from the root solution folder:

To create a new migration:
- ```dotnet ef migrations add <MigrationName> --project GrandmastersHub.Infrastructure --startup-project GrandmastersHub.Api```

To apply migrations to the database:
- ```dotnet ef database update --project GrandmastersHub.Infrastructure --startup-project GrandmastersHub.Api```

# Project Roadmap (Milestones 1 - 6)
- **Milestone 1: Project Planning & Architecture (Completed)**
  - System documentation, UI mockups, and database schema design.
  - Establishment of the baseline Clean Architecture folder structure.
    
- **Milestone 2: Backend Domain & Infrastructure (In Progress)**
  - Implementation of core entities, EF Core database context, and repository logic.
  - SQL Server integration and initial data seeding.
    
- **Milestone 3: Application & API Layers**
  - Development of RESTful controllers, DTOs, and business services.
  - Implementation of JSON Web Token (JWT) authentication and role-based authorization.
    
- **Milestone 4: Front End (React Client & UI)**
  - Initialization of the React workspace and responsive HTML5/CSS3 layout.
  - Development of static components for the product catalog, cart, and user dashboards.
    
- **Milestone 5: System Integration**
  - Connecting the React frontend to the ASP.NET Core backend APIs via Axios/Fetch.
  - Implementing global state management and final checkout flows.
    
- **Milestone 6: QA Testing & Deployment**
  - End-to-end testing, bug squashing, and performance optimization.
  - Final project presentation preparation and cloud deployment (if applicable).
 

## Project Team
- **Person 1: Martinus Jacobus Vermaak**
  - **Responsibilities**: Core MVC Architecture, Product Catalog APIs (Endpoints & DTOs), Core React UI, and HTML5/CSS3 Frontend Shell.
    
- **Person 2: Kimberly Tadiwanashe Karonga**
  - **Responsibilities**: EF Core Database Integration, Data Repositories, Order & Cart APIs, and Frontend Shopping Cart UI.
    
- **Person 3: Ethan Ogle**
  - **Responsibilities**: System Authentication, Security Endpoints (JWTs & Password Hashing), and Login/Registration Frontend UI.
    
- **Person 4: Reinhardt Kleynhans**
  - **Responsibilities**: Additional Data Repositories, Global Error Handling Middleware, and User Profile Dashboard UI.
