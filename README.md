# Galactic Slicer

Galactic Slicer is a web application made for the ITPE3200 Web Applications course at OsloMet.

The goal of the project is to make cybersecurity learning more interactive through a gamified web application. Users can go through different planets, complete challenges, earn credits and track progress.

This version is a basic MVP for the mandatory assignment and is meant to be developed further for the final exam project.

## Technologies

The project uses:

- ASP.NET Core 10.0
- C#
- MVC
- Entity Framework Core
- SQLite
- Razor Views
- HTML
- CSS
- JavaScript
- Bootstrap
- Serilog
- Node.js v24.19.0

## Project structure

The application follows the MVC structure.

- `Models` contains the Challenge model, ViewModels and DbContext
- `Controllers` contains the logic for the different pages
- `Views` contains the Razor pages
- `wwwroot` contains CSS, JavaScript, images and other static files
- `Migrations` contains the Entity Framework migrations

## Database

The project uses SQLite together with Entity Framework Core.

The main entity in the current version is `Challenge`.

A Challenge contains:

- Title
- Question
- Correct answer
- Credit reward
- Planet name

`GalacticSlicerDbContext` is used to connect the application to the database.

Entity Framework migrations are used to create and update the database structure.

## Server-side validation

Server-side validation is added to the Challenge model.

We use Data Annotations such as:

- `Required`
- `StringLength`
- `Range`

The Create and Edit actions also check:

`ModelState.IsValid`

before changes are saved to the database.

Validation messages are shown in the Create and Edit views.

## Error handling

Error handling is added to the database operations in `ChallengeController`.

Create, Edit and Delete use `try/catch` blocks.

If an error happens while saving data, the user is returned to the view and an error message is added to `ModelState`.

## Logging

Serilog is used for server-side logging.

Logging is configured in `Program.cs`.

Errors in Create, Edit and Delete are logged using:

`_logger.LogError(...)`

The log files are stored in the `logs` folder.

The `logs` folder is ignored by Git.

## How to run the project

Requirements:

- .NET SDK 10.0
- Node.js v24.19.0

Go to the project folder:

`cd GalacticSlicer`

Restore the dependencies:

`dotnet restore`

Build the project:

`dotnet build`

Run the application:

`dotnet run`

Then open the localhost address shown in the terminal.

## Sources and inspiration

The main sources used for Jonas part of the project were course material and demos from ITPE3200 on Canvas.

Examples used as reference:

- MVC course examples
- Entity Framework and DAL material
- MyShop / Expanded CRUD demo
- Logging, Error Handling and Input Validation material

The examples were used as reference and adapted to fit the Galactic Slicer project.

## Use of AI

AI was used as a support tool during development.

We used AI to:

- discuss how different parts could be implemented
- understand errors and error messages
- troubleshoot problems
- understand concepts from the course
- discuss how course examples could be adapted to Galactic Slicer
- get suggestions for HTML and CSS structure and styling
- improve and adjust frontend code where needed

The final code was reviewed and adjusted by the group to fit the project and the assignment requirements.

## Git ignore

The project ignores generated and temporary files such as:

- `bin/`
- `obj/`
- `node_modules/`
- `logs/`
- `*.db-shm`
- `*.db-wal`


## Dynamic content

Challenge data is stored in the SQLite database and displayed dynamically through the Challenge views. Users can create, view, edit and delete challenges through the CRUD functionality.

## Current MVP limitations

Some parts of the application are still placeholders in this MVP version. Login and Sign Up do not yet include full authentication, and some progress and practice functionality will be expanded further in the final exam project.

## Group work

### Jonas

My main responsibilities were:

- Challenge model
- Entity Framework setup
- SQLite database setup
- DbContext
- Database registration in Program.cs
- Server-side validation
- Error handling
- Logging

### Henrik

My main responsibilities were:

- ChallengeController
- CRUD functionality
- CRUD views
- Create and Edit forms
- Displaying Challenge data from the database
- Connecting CRUD operations to Entity Framework Core

### Filip

My main responsibilities were:

- Frontend design
- Navigation
- Home page
- Challenge and Practice pages
- Galaxy and Progress pages
- Login and Sign Up pages
- CSS, Bootstrap and JavaScript