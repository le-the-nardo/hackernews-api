# Hacker News Best Stories API

## Overview

This project implements a RESTful API using ASP.NET Core (.NET 9) that retrieves the first **n** best stories from the official Hacker News API.

The API:
- Fetches the list of best story IDs
- Retrieves story details
- Returns the first n stories sorted by score in descending order
- Applies caching and resilience strategies to avoid overloading the Hacker News API

---

## Endpoint

GET /api/stories/best?n={number}

Example:

GET /api/stories/best?n=10

Constraints:
- n must be greater than 0

---

## How to Run

### Prerequisites

- .NET 9 SDK

### Run the application

```bash
dotnet restore
dotnet run
```

### Running tests
```
dotnet tests
```
---

## Architecture

### The project follows a simple and clean structure:

- Clients → Responsible for external API communication
- Services → Business logic orchestration
- Models → Domain models and DTOs
- Program.cs → Minimal API configuration and dependency injection
This separation ensures clarity, testability, and maintainability.

---

## Performance & Scalability

To efficiently handle multiple requests without overloading the Hacker News API:

### In-Memory Caching

- Best story IDs are cached for 5 minutes
- Individual stories are cached for 5 minutes
- Reduces repeated external calls

### Controlled Parallelism

- Story detail requests are limited using SemaphoreSlim
- Maximum concurrent external requests: 10
- Prevents resource exhaustion and connection saturation


---

## Resilience Strategy

### Polly is used to implement:

- Retry policy (3 retries with exponential backoff)
- Timeout protection on HttpClient

This ensures transient HTTP failures do not immediately cause request failure.

---

## Testing Strategy
The test suite focuses on the application's business logic layer (StoryService).

Infrastructure components such as HttpClient, Polly policies and ASP.NET pipeline are not tested directly, as they belong to framework-level behavior.

The following scenarios are covered:

- Returns only the requested number of stories (Take(n))
- Orders stories by score in descending order
- Ignores null stories returned by the external API
- Returns an empty list when no story IDs are available
- Ensures caching prevents redundant API calls
---

## Future Improvements

In a real production environment, the following improvements would be considered:

- Distributed caching (e.g., Redis) for multi-instance deployments
- Circuit breaker policy
- Background cache refresh to avoid potential cache stampede
- Structured logging (Serilog)
- Health checks endpoint