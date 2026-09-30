Galactic Slicer

Galactic Slicer is a web application made for the ITPE3200 Web Applications course at OsloMet.

The goal of the project is to make cybersecurity learning more interactive through a gamified web application. Users can go through different planets, complete challenges, 
earn credits and track progress.

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

```csharp
ModelState.IsValid




Til README trenger jeg at dere bare skriver kort om hva dere har gjort.

Henrik:
Skriv 3–6 korte punkter eller et lite avsnitt om:
- ChallengeController
- CRUD-funksjonaliteten
- Create / Read / Edit / Delete
- Forms
- Hvordan Challenges hentes fra databasen og vises dynamisk
- Eventuelt hvilke Canvas-demoer eller andre kilder du brukte

Eksempel:
"I worked mainly on the Challenge CRUD functionality. This included creating the ChallengeController and the Create, Edit, Details and Delete views. Entity Framework Core is used to retrieve and update Challenge data in the SQLite database."

Filip:
Skriv 3–6 korte punkter eller et lite avsnitt om:
- Frontend/design
- Navigation
- Home page
- Challenge/Practice pages
- Planet/Galaxy page
- Progress page
- Login/Signup
- CSS / Bootstrap / JavaScript du har brukt
- Eventuelt hvilke kilder eller inspirasjon du brukte

Eksempel:
"I worked mainly on the frontend and navigation. I created and styled several pages, including Home, Galaxy, Progress and the Challenge pages. CSS, Bootstrap and JavaScript were used for the layout, styling and quiz interaction."

Hvis dere har brukt:
- Canvas-demoer
- nettsider/tutorials
- kodeeksempler
- AI

så skriv kort hva dere brukte det til, så legger vi det inn under Sources / Use of AI.
