# MyStockAPIs

A modern, extensible .NET 9 Web API for managing stocks and associated comments, featuring advanced HTTP request/response logging for debugging.

---

## 🚀 Features

* **Stock Management**: CRUD operations for stocks and their related comments.
* **Entity Framework Core**: Code-first data access with migrations.
* **Custom HTTP Logging Middleware**: Configurable, timezone-aware request/response logging to file.
* **Latest .NET Technologies**: Built with .NET 9 and C# 13.
* **Clean Architecture**: Structured into Models, Controllers, Services, and Data layers.

---

## 📁 Project Structure

```
MyStockAPIs/
├── Controllers/
│   └── StockController.cs
├── Models/
│   ├── Stock.cs
│   └── Comment.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Services/
│   └── HttpLoggingMiddleware.cs
├── appsettings.json
└── ... (standard .NET files)
```

---

## 🛠 Getting Started

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

### Setup

1. **Clone the Repository**

```bash
git clone <your-repo-url>
cd MyStockAPIs
```

2. **Configure Database**

Update your connection string in `appsettings.json` under `ConnectionStrings`.

3. **Apply EF Core Migrations**

```bash
dotnet ef database update
```

4. **(Optional) Configure HTTP Logging**

Customize the `CustomHttpLogging` section in `appsettings.json`:

```json
"CustomHttpLogging": {
  "FileName": "HttpLogsFile/http-RequestsAndResponses.json",
  "HttpLogging": {
    "UseBeautifulFormat": true,
    "LogToSeparateFiles": false,
    "TimeZone": "E. Africa Standard Time"
  }
}
```

5. **Run the Application**

```bash
dotnet run
```

---

## 📡 API Endpoints

### Stocks

* `GET /api/v1/stock` – Get all stocks
* `GET /api/v1/stock/{id}` – Get stock by ID

> *Add POST, PUT, DELETE and comment endpoints as needed.*

---

## 📄 HTTP Logging Middleware

Logs all HTTP requests and responses to file.

**Configuration (`appsettings.json`):**

* `FileName`: Log output file path
* `UseBeautifulFormat`: Enables pretty-print logging
* `LogToSeparateFiles`: Splits each log into separate files
* `TimeZone`: Timezone for timestamps

**Enablement:**

* Already added in middleware pipeline
* Ensure `app.UseMiddleware<HttpLoggingMiddleware>()` is placed **after** static files and Swagger

---

## 🧱 Models

### Stock

```csharp
public class Stock {
    public int Id { get; set; }
    public string Symbol { get; set; }
    public string CompanyName { get; set; }
    public decimal Purchase { get; set; }
    public decimal LastDividend { get; set; }
    public string Industry { get; set; }
    public long MarketCap { get; set; }
    public List<Comment> Comments { get; set; }
}
```

### Comment

```csharp
public class Comment {
    public int Id { get; set; }
    public string CommentTitle { get; set; }
    public string Content { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? StockId { get; set; }
    public Stock? Stock { get; set; }
}
```

---

## 🧩 Extending the Project

* Add more endpoints (POST, PUT, DELETE)
* Add comments-related routes
* Implement authentication (JWT, Identity)
* Add model validation
* Write unit and integration tests

---

## 📜 License

This project is open-source and provided for educational/demo purposes.

---

## 🙌 Credits

* Built with [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
* Logging inspired by real-world .NET HTTP logging best practices

---

## 📬 Contact

Have questions or want to contribute?

* Open an [issue](https://mulutx.co.ke)
* Submit a pull request
