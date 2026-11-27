# Galactic Slicer

Galactic Slicer is a web application made for the ITPE3200 Web Applications course at OsloMet (exam project, Task 2: Gamification Platform for Course Learning).

The goal of the project is to make cybersecurity learning more interactive. Players travel from planet to planet, pass a mission on each planet by answering questions, earn Credits and XP, rank up, and spend their Credits on character badges. They can also practice each subject and create their own practice challenges.

The repository contains two sub-applications that work independently:

- **Subapp 1** (`GalacticSlicer/`): ASP.NET Core 10.0 MVC with Razor views
- **Subapp 2** (`GalacticSlicer.Subapp2/`): ASP.NET Core 10.0 Web API with a React frontend

## Subapp 1 (MVC)

### Technologies

- .NET SDK 10.0 (developed with 10.0.402), ASP.NET Core 10.0 MVC, C#
- Entity Framework Core 10.0 with SQLite
- ASP.NET Core Identity with roles (cookie authentication)
- Razor views, HTML, CSS, JavaScript, Bootstrap, jQuery Validation
- Serilog (logging to file)
- xUnit and NSubstitute (unit tests in `GalacticSlicer.Tests/`)

Node.js is not needed for Subapp 1.

### How to run

Requirements: .NET SDK 10.0.

```
cd GalacticSlicer
dotnet run
```

Then open http://localhost:5187 (or use `dotnet run --launch-profile https` for https://localhost:7153).

On startup in Development (the default for `dotnet run`), the app creates the SQLite database `GalacticSlicer.db` if it is missing, applies all migrations and seeds the planets, characters, the mission questions from `DAL/SeedData/Questions.json` and an admin account. There is no need to run `dotnet ef database update` by hand.

### Logging in

- **Admin:** username `admin`, password `Admin123!` (seeded on startup). The admin can create, edit and delete planets and characters.
- **Standard user:** username `user`, password `User123!` (seeded on startup, no admin rights).
- **New players:** create an account with **Sign up**. New players start with 0 Credits and the Rookie Slicer badge available for free in the Shop.

### Running the tests

From the repository root:

```
dotnet test GalacticSlicer.Tests/GalacticSlicer.Tests.csproj
```

### Main features

- **Galaxy:** six planets in a fixed order. Each planet has a mission on one cybersecurity subject; Endor, the last planet, is a final exam with questions from every subject.
- **Missions:** 10 random questions from the planet's subject with shuffled answers. Answers are graded on the server, and 80% correct is needed to pass. Passing a planet's mission the first time gives 2000 XP and 500 Credits per chapter (Tatooine 500, Naboo 1000, … Endor 3000), which is always enough to unlock the next planet and to buy the next badge.
- **Practice:** practice any subject with instant feedback and explanations. Players' own challenges are included in practice.
- **Own challenges:** players can create, read, edit and delete their own practice questions, and search them by text and subject.
- **Shop:** buy, equip and unequip character badges, with search by text, maximum price, sorting and "owned only".
- **Progress:** pilot panel with rank, Credits, the next mission, a mini starchart and progress bars.
- **Admin:** manage planets and characters (create, edit, delete), with delete rules so the game can't be broken.

### Project structure

- `Controllers/`: one controller per area (Account, Challenge, Character, Home, Planet, Progress)
- `Models/`: the entity classes and `GalacticSlicerDbContext`
- `ViewModels/`: the data passed to the views
- `Views/`: the Razor views; `Views/Shared/` has the layout and shared partials
- `ViewComponents/`: `NavUserViewComponent` (the player's name, Credits and badge in the navbar)
- `Services/`: the game rules (mission rewards, purchases, planet unlocking, ranks, questions), each with an interface
- `DAL/`: the repositories (Challenge, Character, Planet) and `DBInit` (seeding); `DAL/SeedData/` has the mission questions
- `Helpers/`: `CharacterTier` (badge tier from the price) and `LogExceptionFilter` (logs unexpected errors and shows the friendly error page)
- `Migrations/`: the Entity Framework Core migrations
- `wwwroot/`: CSS, JavaScript and libraries

Controllers get data through the repositories and services (dependency injection); no controller or view uses the DbContext directly.

### Database

SQLite with Entity Framework Core. Main tables:

- `Planets`, `Challenges` (mission questions and players' own questions), `Characters`
- `PlayerProgresses`: each player's Credits, XP, rank and equipped badge
- `UserCharacters`: which characters each player owns (the many-to-many relationship between players and characters)
- `CompletedChallenges` and `CompletedMissions`: what each player has answered and passed
- the ASP.NET Core Identity tables (users and roles)

### Validation, error handling and logging

- **Server-side validation:** Data Annotations (`Required`, `StringLength`, `Range`, ...) on the models and ViewModels, checked with `ModelState.IsValid`, plus game rules in the controllers and services (for example a challenge's correct answer must be one of its options).
- **Client-side validation:** jQuery Validation on all forms.
- **Security:** antiforgery tokens on every form that changes data, `[Authorize]` on player actions and `[Authorize(Roles = "Admin")]` on admin pages, owner checks on players' own challenges, and missions are graded on the server.
- **Error handling:** create, edit, delete, purchase, log in and sign up use `try/catch` and show a friendly message instead of crashing. Any other unexpected error in a controller is logged by `LogExceptionFilter` (registered in `Program.cs`) and the player sees a friendly 500 page. Unknown pages and pages without access show a friendly 404 or 403 page.
- **Logging:** Serilog writes to `logs/log-<date>.txt` (configured in `Program.cs`). Controllers, services and repositories log errors and important game events with `ILogger`.

## Subapp 2 (API + React)

See `GalacticSlicer.Subapp2/`. Run instructions for Subapp 2 will be added here.

## Sources and inspiration

The main sources used for the project were course material and demos from ITPE3200 on Canvas.

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

The project ignores generated and local files such as:

- `bin/` and `obj/`
- `node_modules/`
- `logs/`
- the SQLite database files (`*.db`, `*.db-shm`, `*.db-wal`)

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
