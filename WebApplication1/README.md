# Bowling League Website

This is a web application that displays information about bowlers from the Marlins and Sharks teams in a bowling league. The application uses ASP.NET Core for the backend API and React for the frontend.

## Prerequisites

- .NET 9.0 SDK
- Node.js and npm

## Database

The application uses a SQLite database located at:
```
/Users/zackmcdougal/Downloads/BowlingLeague (1).sqlite
```

Make sure this file exists and is accessible.

## Getting Started

### Running the Application

The application is now configured to automatically check if the React app needs to be built when it starts. You can simply run the ASP.NET Core application:

```bash
# If you have the dotnet CLI available:
dotnet run --project WebApplication1

# Or use your IDE (like Rider) to run the WebApplication1 project
```

If the React app hasn't been built yet, it will be built automatically. This may take a few minutes the first time as it needs to download and install npm packages.

### Manual Building (if needed)

If you prefer to build the React app manually, you can use the provided scripts:

#### On macOS/Linux:

```bash
./WebApplication1/build-react.sh
```

#### On Windows:

```cmd
WebApplication1\build-react.bat
```

The application will be available at:
- https://localhost:7240
- http://localhost:5240

### Troubleshooting

If you encounter any issues with the automatic build process:

1. Make sure Node.js is installed and available in your PATH
2. Try building the React app manually using the provided scripts
3. Check the application logs for any error messages

## Features

- Displays bowler information including name, team, address, city, state, zip, and phone number
- Only shows bowlers from the Marlins and Sharks teams
- Responsive design with a clean, modern interface

## Project Structure

- **WebApplication1/Models**: Contains the data models for Bowler and Team
- **WebApplication1/Data**: Contains the database context
- **WebApplication1/Controllers**: Contains the API controllers
- **WebApplication1/ClientApp**: Contains the React frontend application
  - **src/components**: React components including Header and BowlerTable
  - **src/App.js**: Main React application component

## API Endpoints

- GET `/api/bowlers`: Returns all bowlers from the Marlins and Sharks teams
- GET `/api/bowlers/{id}`: Returns a specific bowler by ID
- GET `/api/bowlers/teams`: Returns the Marlins and Sharks teams
