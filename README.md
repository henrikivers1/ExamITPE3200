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
- **Progress:** pilot panel with rank, Credits, the next mission, a mini starchart, progress bars and the mission history (the latest attempts, passed or failed).
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
- `MissionAttempts`: every graded mission (score, passed or failed, time) for the mission history
- the ASP.NET Core Identity tables (users and roles)

### Validation, error handling and logging

- **Server-side validation:** Data Annotations (`Required`, `StringLength`, `Range`, ...) on the models and ViewModels, checked with `ModelState.IsValid`, plus game rules in the controllers and services (for example a challenge's correct answer must be one of its options).
- **Client-side validation:** jQuery Validation on all forms.
- **Security:** antiforgery tokens on every form that changes data, accounts locked for 5 minutes after 5 wrong passwords, `[Authorize]` on player actions and `[Authorize(Roles = "Admin")]` on admin pages, owner checks on players' own challenges, and missions are graded on the server.
- **Error handling:** create, edit, delete, purchase, log in and sign up use `try/catch` and show a friendly message instead of crashing. Any other unexpected error in a controller is logged by `LogExceptionFilter` (registered in `Program.cs`) and the player sees a friendly 500 page. Unknown pages and pages without access show a friendly 404 or 403 page.
- **Logging:** Serilog writes to `logs/log-<date>.txt` (configured in `Program.cs`). Controllers, services and repositories log errors and important game events with `ILogger`.

## Subapp 2 (API + React)

Subapp 2 is the same game built as an ASP.NET Core Web API (`GalacticSlicer.Subapp2/GalacticSlicer.Api`) and a React single-page app (`GalacticSlicer.Subapp2/galactic-slicer-client`). It has its own SQLite database and its own accounts, so it runs independently of Subapp 1.

### Technologies

- **API:** .NET SDK 10.0, ASP.NET Core 10.0 Web API, C#, Entity Framework Core 10.0 with SQLite, ASP.NET Core Identity with roles, JWT bearer authentication
- **Client:** React 19, React Router 7, TypeScript 5.9, Vite 8, ESLint
- **Tests:** xUnit and NSubstitute (API tests in `GalacticSlicer.Subapp2/GalacticSlicer.Api.Tests/`)

### How to run

Requirements: .NET SDK 10.0 and Node.js 20.19+ or 22.12+ (Vite 8 needs one of these; developed with Node.js 24).

Start the API in one terminal:

```
cd GalacticSlicer.Subapp2/GalacticSlicer.Api
dotnet run
```

The API runs on http://localhost:5190 (or https://localhost:7211 with `dotnet run --launch-profile https`).

Start the client in a second terminal:

```
cd GalacticSlicer.Subapp2/galactic-slicer-client
npm ci
npm run dev
```

Then open http://localhost:5173. The client calls the API on http://localhost:5190; set `VITE_API_URL` to use another address.

On Windows PowerShell, `npm` can be blocked with "running scripts is disabled on this system". Use `npm.cmd ci` and `npm.cmd run dev` instead, or allow local scripts once with `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned`.

On startup the API creates the SQLite database `GalacticSlicerApi.db` if it is missing, applies all migrations and seeds the six planets, the five characters, the mission questions (the same `Questions.json` as Subapp 1, linked from `GalacticSlicer/DAL/SeedData/`) and the Admin role.

In Development the API creates a temporary key for signing the JWTs, so no setup is needed. Tokens then stop working when the API restarts, and players simply log in again. Outside Development a key must be set in `Jwt:Key`.

### Logging in

Subapp 2 logs in with **email** and password, and its accounts are separate from Subapp 1's.

- **New players:** create an account with **Sign up**, then log in.
- **Admin:** no admin is seeded by default. To get one, set the admin's email and password with user secrets before starting the API:

  ```
  cd GalacticSlicer.Subapp2/GalacticSlicer.Api
  dotnet user-secrets set "BootstrapAdmin:Email" "admin@galacticslicer.local"
  dotnet user-secrets set "BootstrapAdmin:Password" "Admin123!"
  dotnet run
  ```

  Then log in with `admin@galacticslicer.local` / `Admin123!`. The admin gets a "Manage planets" button on the Galaxy page and a "Manage characters" button in the Shop, and receives 1,000,000 Credits once for testing the Shop.

### Running the tests and checks

From the repository root:

```
dotnet test GalacticSlicer.Subapp2/GalacticSlicer.Api.Tests
```

In `GalacticSlicer.Subapp2/galactic-slicer-client`: `npm run lint` (ESLint) and `npm run build` (TypeScript check and production build).

`GalacticSlicer.Subapp2/GalacticSlicer.Api/GalacticSlicer.Api.http` has example requests for the API (Visual Studio, Rider or the VS Code REST Client).

### Main features

The game rules are the same as in Subapp 1: six planets, 10 random questions per mission (Endor mixes every subject), 80% to pass, and 2000 XP plus 500 Credits per chapter for the first pass. A completed mission can be replayed, but the rewards are only given once.

- **Galaxy and missions:** the Starchart shows which planets are locked, unlocked and completed. A mission starts with a briefing and is graded on the server.
- **Practice:** practice any subject with instant feedback and explanations (official questions only).
- **Own challenges:** players can create, read, edit and delete their own practice questions, and search them by text and subject.
- **Shop:** buy, equip and unequip character badges.
- **Progress:** pilot panel with rank, Credits, the next mission, a mini starchart, progress bars and the mission history (the latest attempts, passed or failed, stored in the `MissionAttempts` table).
- **Admin:** manage planets and characters (create, edit, delete), with delete rules so the game can't be broken.

### Project structure

API (`GalacticSlicer.Api/`):

- `Controllers/`: Auth, Challenges, Characters, CharacterGameplay (Shop), Missions, Planets, Practice, Progress
- `DTOs/`: the data sent to and from the client; controllers never return entities
- `Services/`: the game rules and data access for the controllers (missions, practice, progress, purchases, planet unlocking, ranks)
- `DAL/`: the repositories (Challenge, Character, Planet)
- `Models/`: the entity classes, `GalacticSlicerDbContext` and `DBInit` (seeding)
- `Filters/`: `LogExceptionFilter` (logs unexpected errors and returns a 500 with a message)
- `Migrations/`: the Entity Framework Core migrations

Controllers get data through the services and repositories (dependency injection); no controller uses the DbContext directly.

Client (`galactic-slicer-client/src/`):

- `pages/`: one component per page (Galaxy, Mission, Practice, Challenges, Shop, Progress, Log in, Sign up, admin pages)
- `components/`: shared parts such as the navbar, planet cards, badges and progress bars
- `services/`: `api.ts` (base URL, JWT and error handling) and one service per area (auth, planets, characters, challenges, missions, progress); no page calls `fetch` directly
- `validation/`, `types/`, `utils/`, `hooks/` and `styles/`

### Validation, error handling and logging

- **Server-side validation:** Data Annotations on the DTOs (the API returns 400 with the errors per field), plus game rules in the services (unique names, protected official planets and standard characters, owned characters can't be deleted).
- **Client-side validation:** the forms check required fields, lengths and the password rules before sending, and show the API's field errors next to the fields.
- **Security:** JWT authentication, `[Authorize]` on player endpoints and `[Authorize(Roles = "Admin")]` on admin endpoints, owner checks on players' own challenges, and missions are graded on the server (only the questions the API handed out are accepted).
- **Error handling:** expected problems return 400, 403, 404 or 409 with a message that the client shows. Any other unexpected error is logged by `LogExceptionFilter` (registered in `Program.cs`) and returns a 500 with a friendly message.
- **Logging:** every controller and the game services log errors and important events (logins, missions, purchases, admin changes) with `ILogger`.

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
