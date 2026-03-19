# RupeeFlow — Expense Management App
### Complete Project Documentation (Viva/Presentation Ready)

---

## 1. What Is This Project?

**RupeeFlow** is a full-stack personal expense management web application. Users can register, log in, and track their daily expenses by category. They can filter their expense history, see a spending chart, and delete entries. The app supports a light and dark mode.

---

## 2. Tech Stack

### Backend
| Technology | Version / Detail | Why Used |
|---|---|---|
| **ASP.NET Core Web API** | .NET 8 | Backend framework to build REST APIs in C# |
| **Entity Framework Core** | EF Core 8 | ORM to talk to the database using C# objects instead of raw SQL |
| **SQL Server (LocalDB)** | SQL Server Express LocalDB | Relational database to persist all data |
| **JWT (JSON Web Tokens)** | `System.IdentityModel.Tokens.Jwt` | Stateless authentication — user logs in once, gets a token, every API call uses that token |
| **Swagger / OpenAPI** | Swashbuckle | Auto-generated API documentation and testing UI at `/swagger` |

### Frontend
| Technology | Detail | Why Used |
|---|---|---|
| **Angular 17** | Standalone component, no NgModules | Frontend SPA framework |
| **TypeScript** | Strongly typed JS | Safer, more readable code |
| **Angular HttpClient** | Built-in | For making HTTP calls to the backend API |
| **FormsModule** | Two-way binding via `[(ngModel)]` | Simple form handling |
| **ng2-charts + Chart.js** | Bar chart | Visual spending breakdown by category |
| **Vanilla CSS** | Custom, no library | Full control over styling, dark/light themes |

---

## 3. Project Architecture

The architecture follows a **3-tier separation**: Frontend → Backend API → Database.

```
┌─────────────────────────────────────┐
│         Angular Frontend            │
│  (Browser, localhost:4200)          │
│  - Single Page Application (SPA)    │
│  - Sends HTTP requests with JWT     │
└───────────────┬─────────────────────┘
                │  HTTP (REST API calls)
                ▼
┌─────────────────────────────────────┐
│      ASP.NET Core Web API           │
│  (Backend, localhost:5098)          │
│                                     │
│   ┌─────────┐   ┌──────────────┐   │
│   │Controllers│  │Repositories  │   │
│   │(API Layer)│→ │(Data Layer)  │   │
│   └─────────┘   └──────┬───────┘   │
│                         │           │
│              ┌──────────▼────────┐  │
│              │  EF Core DbContext │  │
│              └──────────┬────────┘  │
└─────────────────────────┼───────────┘
                          │  SQL queries
                          ▼
         ┌─────────────────────────────┐
         │     SQL Server (LocalDB)     │
         │     Database: TranDB         │
         └─────────────────────────────┘
```

### Backend Layer Pattern: Repository Pattern
The backend uses the **Repository Pattern**. This means:
- The Controller does NOT directly query the database.
- The Controller asks the Repository.
- The Repository talks to [ApplicationDbContext](file:///d:/Expense_managment/Data/ApplicationDbContext.cs#8-11) (EF Core).
- This keeps things clean and testable.

---

## 4. Database Design (Tables & Relationships)

### Tables

| Table | Description |
|---|---|
| `Users` | Stores registered users (name, email, password) |
| [Categories](file:///d:/Expense_managment/frontend/src/app/app.component.ts#213-224) | Stores expense categories created per user (Food, Transport, etc.) |
| [Transactions](file:///d:/Expense_managment/Controllers/TransactionController.cs#39-47) | Stores each expense entry (amount, date, note, category) |
| `Budgets` | (Model exists) Monthly budget limits per category |
| `Wallets` | (Model exists) Wallet/account tracking |
| `SavingsGoals` | (Model exists) Goal tracking |

### Key Relationships
- A **User** has many **Categories**, **Transactions**, **Budgets**, **Wallets**, **SavingsGoals**
- A **Transaction** belongs to one **User** and one **Category**
- A **Category** can have many **Transactions**
- **OnDelete(Restrict)** is used for Category → Transaction (you can't delete a category that has transactions). This avoids accidental data loss.
- **OnDelete(SetNull)** is used for Wallet → Transaction (if a wallet is deleted, transaction keeps the expense but wallet reference becomes null).

---

## 5. File Structure & What Each File Does

### Backend (`d:\Expense_managment\`)

```
Expense_managment/
│
├── Program.cs                          ← Entry point. Wires everything together.
│                                         Registers DB, JWT auth, CORS, Repositories,
│                                         Background Service, Swagger
│
├── appsettings.json                    ← Config file: DB connection string + JWT secret key
│
├── Models/                             ← C# classes that map to database tables
│   ├── User.cs                         ← User table: UserId, Name, Email, Password
│   ├── Category.cs                     ← Category: CategoryId, Title, Icon, Type, UserId
│   ├── Transaction.cs                  ← Transaction: Amount, Note, Date, CategoryId, UserId
│   ├── Budget.cs                       ← Budget model (limit per category/month)
│   ├── Wallet.cs                       ← Wallet model (e.g., Cash, UPI)
│   └── SavingsGoal.cs                  ← Savings goal model
│
├── Data/
│   └── ApplicationDbContext.cs         ← EF Core bridge between C# models & SQL Server.
│                                         Defines DbSets (tables). Configures delete behavior.
│
├── Repositories/                       ← Data access layer (Repository Pattern)
│   ├── IUserRepository.cs              ← Interface: defines what user operations exist
│   ├── UserRepository.cs              ← Implements: authenticate, register, get user, update
│   ├── ICategoryRepository.cs
│   ├── CategoryRepository.cs          ← Get all categories for a user, add, delete
│   ├── ITransactionRepository.cs
│   ├── TransactionRepository.cs       ← Add/get/delete transactions, get recurring ones
│   ├── IBudgetRepository.cs
│   ├── BudgetRepository.cs
│   ├── IWalletRepository.cs
│   ├── WalletRepository.cs
│   ├── ISavingsGoalRepository.cs
│   └── SavingsGoalRepository.cs
│
├── Controllers/                        ← API endpoints (what the frontend calls)
│   ├── AuthController.cs              ← POST /api/Auth/login, /register, PUT /profile
│   ├── TransactionController.cs       ← GET/POST/PUT/DELETE /api/Transaction
│   ├── CategoryController.cs          ← GET/POST/DELETE /api/Category
│   ├── BudgetController.cs
│   ├── WalletController.cs
│   ├── DashboardController.cs         ← Summary stats endpoint
│   ├── DataExportController.cs        ← Export expenses
│   └── SavingsGoalController.cs
│
├── Services/
│   └── RecurrenceBackgroundService.cs ← Runs daily in background. Auto-creates
│                                         instances of recurring transactions.
│
└── Migrations/                         ← EF Core migration files (auto-generated SQL)
```

### Frontend (`d:\Expense_managment\frontend\src\`)

```
frontend/src/
│
├── index.html                          ← Root HTML file. Loads the Angular app.
├── main.ts                             ← Angular bootstrap entry point
├── styles.css                          ← Global CSS. Light/Dark mode body-level overrides.
│
└── app/
    ├── app.component.html              ← The entire UI template (auth page + dashboard)
    ├── app.component.ts                ← All logic: login, register, load data, filters, chart
    ├── app.component.css               ← Scoped CSS for the component (layout, table, cards)
    ├── app.config.ts                   ← Registers HttpClient provider for Angular
    ├── app.routes.ts                   ← Route config (currently single-page, no routing)
    └── app.component.spec.ts           ← Auto-generated unit test file
```

---

## 6. How Authentication Works (JWT Flow)

> This is one of the most commonly asked concepts.

1. User fills in email + password in the login form.
2. Angular sends a `POST /api/Auth/login` request with the credentials.
3. The backend's [AuthController](file:///d:/Expense_managment/Controllers/AuthController.cs#11-140) calls `UserRepository.AuthenticateAsync()` which queries the DB for a matching email + password.
4. If found, the backend **generates a JWT token** using `JwtSecurityTokenHandler`. The token contains:
   - `sub` (Subject) = User's ID
   - `email` = User's email
   - `jti` = unique token ID (prevents replay)
   - Expiry = 2 hours
   - Signed with HMAC SHA-256 using a secret key from [appsettings.json](file:///d:/Expense_managment/appsettings.json)
5. The token is sent back to the Angular frontend in the response.
6. Angular stores this token in `localStorage`.
7. **Every subsequent API call** (get transactions, add expense, etc.) sends the token in the HTTP header: `Authorization: Bearer <token>`.
8. The backend's `[Authorize]` attribute on controllers checks this token automatically.
9. Inside a controller, `User.FindFirstValue(ClaimTypes.NameIdentifier)` reads the User ID from inside the token — no extra DB call needed.

**Why JWT?**
JWT is **stateless** — the server does not store sessions. The token itself carries the user's identity. The server just verifies the signature.

---

## 7. How Adding an Expense Works (Full Flow)

1. User fills in Amount, Category, Date, Note in the sidebar form.
2. Angular calls [addExpense()](file:///d:/Expense_managment/frontend/src/app/app.component.ts#254-277) in [app.component.ts](file:///d:/Expense_managment/frontend/src/app/app.component.ts).
3. It sends a `POST /api/Transaction` request with the JWT token in the header.
4. `TransactionController.PostTransaction()` receives the request.
5. It calls [GetUserId()](file:///d:/Expense_managment/Controllers/TransactionController.cs#23-27) which reads the user ID from the JWT token's claims.
6. It verifies the selected category actually belongs to that user (security check).
7. It creates a new [Transaction](file:///d:/Expense_managment/Models/Transaction.cs#6-42) object and saves it via `TransactionRepository.AddAsync()`.
8. EF Core generates an `INSERT INTO Transactions ...` SQL statement.
9. The response comes back, Angular re-fetches all transactions and re-renders the table.

---

## 8. How Filters Work (Frontend Only)

All filtering happens **on the frontend** — no extra API calls per filter.

1. On login, Angular fetches ALL the user's expenses once and stores them in `allTransactions[]`.
2. Every filter change calls [applyFilters()](file:///d:/Expense_managment/frontend/src/app/app.component.ts#283-327).
3. [applyFilters()](file:///d:/Expense_managment/frontend/src/app/app.component.ts#283-327) creates a copy of `allTransactions` and applies:
   - **Category filter**: compare `t.categoryId === filterCategory`
   - **Month filter**: parse year/month from the selected value, check date
   - **Date range filter**: compare from/to dates
   - **Note search**: `.includes()` on the note string (case-insensitive)
   - **Sort**: JS `.sort()` comparing dates or amounts
4. The result is stored in `filteredTransactions[]` which the HTML renders.

**Why this approach?** Since a personal expense list is typically small (hundreds, not millions of records), it's much faster to load once and filter in-memory than to call the API on every filter change.

---

## 9. How the Chart Works

- After loading transactions, [buildChart()](file:///d:/Expense_managment/frontend/src/app/app.component.ts#338-358) is called.
- It loops through all transactions and groups the total amount by category using a `Map<string, number>`.
- The map is sorted by total amount (descending).
- The sorted data is passed into `barChartData` (used by `ng2-charts`/Chart.js).
- Chart.js renders a bar chart inside a `<canvas>` element.
- When dark mode toggles, [applyTheme()](file:///d:/Expense_managment/frontend/src/app/app.component.ts#115-141) updates `barChartOptions` to change text color (`#aaa`) and grid color (`#333`) for good visibility.

---

## 10. How Dark Mode Works

- A `isDarkMode` boolean is tracked in the component.
- On toggle, [applyTheme()](file:///d:/Expense_managment/frontend/src/app/app.component.ts#115-141) adds or removes the `dark` CSS class on `document.body` using Angular's `Renderer2`.
- The user's preference is saved in `localStorage` and restored on next visit.
- [styles.css](file:///d:/Expense_managment/frontend/src/styles.css) contains all `body.dark .class-name { ... }` overrides.
- `!important` is used on some table cell styles because Angular's "View Encapsulation" makes component-scoped styles have higher specificity than global styles.

---

## 11. How Recurring Transactions Work

- A transaction can be marked `IsRecurring = true` with a `RecurrenceFrequency` (Daily/Weekly/Monthly/Yearly).
- When saved, [TransactionController](file:///d:/Expense_managment/Controllers/TransactionController.cs#9-168) calculates `NextRecurrenceDate` (e.g., today + 1 month).
- [RecurrenceBackgroundService](file:///d:/Expense_managment/Services/RecurrenceBackgroundService.cs#13-109) is an ASP.NET [BackgroundService](file:///d:/Expense_managment/Services/RecurrenceBackgroundService.cs#13-109) that runs automatically:
  - It checks once on startup and then every 24 hours.
  - It fetches all transactions where `IsRecurring = true` AND `NextRecurrenceDate <= Today`.
  - For each, it creates a **new, non-recurring transaction** as the actual expense record.
  - The original "template" transaction's `NextRecurrenceDate` is advanced to the next period.
  - If the app was offline for a while, it catches up by running the loop until the date reaches today.

---

## 12. Security Concepts Applied

| Concept | Where | How |
|---|---|---|
| **JWT Authentication** | All API controllers | `[Authorize]` attribute blocks unauthenticated calls |
| **User Isolation** | All data queries | Every query includes `UserId = GetUserId()` — users can ONLY see their own data |
| **Category Ownership Check** | `TransactionController.PostTransaction` | Before adding a transaction, verifies the category belongs to the requesting user |
| **CORS Policy** | [Program.cs](file:///d:/Expense_managment/Program.cs) | `AllowAll` configured for local development |
| **ReferenceHandler.IgnoreCycles** | [Program.cs](file:///d:/Expense_managment/Program.cs) JSON options | Prevents infinite loops when serializing circular EF navigation properties (e.g., User→Transaction→User) |

---

## 13. Key Concepts to Know for Questions

**Q: What is Entity Framework Core?**
EF Core is an ORM (Object-Relational Mapper). Instead of writing `SELECT * FROM Users WHERE...`, you write `_context.Users.FirstOrDefaultAsync(u => u.Email == email)`. EF Core converts that to SQL automatically.

**Q: What is the Repository Pattern?**
Instead of putting all database code inside controllers (messy), you create a separate class (Repository) that handles all DB operations. The Controller just calls the Repository. This makes code clean, testable, and easy to change.

**Q: What are Migrations?**
When you change a Model in C#, you run `dotnet ef migrations add <Name>` and `dotnet ef database update`. EF Core generates a SQL file that updates the database schema automatically without writing SQL by hand.

**Q: Why JWT over sessions?**
Sessions store state on the server (memory or DB). JWT stores state inside the token itself (on the client). JWT is scalable — you can run multiple servers without sharing session state.

**Q: What is a BackgroundService in ASP.NET?**
It's a class that extends [BackgroundService](file:///d:/Expense_managment/Services/RecurrenceBackgroundService.cs#13-109) and runs automatically in the background while the web server is running. [ExecuteAsync](file:///d:/Expense_managment/Services/RecurrenceBackgroundService.cs#24-36) is the method that runs. It's registered in [Program.cs](file:///d:/Expense_managment/Program.cs) with `AddHostedService<>()`.

**Q: What is CORS?**
Cross-Origin Resource Sharing. Browsers block requests from one origin (localhost:4200) to another (localhost:5098) by default. The backend must explicitly allow it. `AllowAll` policy in [Program.cs](file:///d:/Expense_managment/Program.cs) tells the browser it's okay.

**Q: What is View Encapsulation in Angular?**
Angular scopes CSS inside a component to that component only (adds a random attribute like `_ngcontent-abc`). This means global CSS can sometimes fail to override component CSS. The solution is using `!important` in global styles or using `::ng-deep`.

---

## 14. How to Run the Project

**Backend:**
```
cd d:\Expense_managment
dotnet run
→ Runs on http://localhost:5098
→ Swagger at http://localhost:5098/swagger
```

**Frontend:**
```
cd d:\Expense_managment\frontend
npm start
→ Runs on http://localhost:4200
```

The database is SQL Server LocalDB. The DB name is `TranDB` as set in [appsettings.json](file:///d:/Expense_managment/appsettings.json).

---

## 15. What "AI-ish" Things to Review

Before your presentation, here are things that may look over-engineered or "AI-made" that you should be aware of or consider toning down. **Ask about each before removing:**

- **[favicon.ico](file:///d:/Expense_managment/frontend/src/favicon.ico)** — the custom web icon in the browser tab
- **`assets/logo.png`** — the logo image used in the nav and auth page
- **Swagger UI** — auto-generated API docs visible at `/swagger` (looks very professional)
- **[RecurrenceBackgroundService](file:///d:/Expense_managment/Services/RecurrenceBackgroundService.cs#13-109)** — may seem complex for a simple expense tracker
- **[DataExportController.cs](file:///d:/Expense_managment/Controllers/DataExportController.cs)** — exports feature that isn't shown in the UI
- **`BudgetController`, `WalletController`, `SavingsGoalController`** — these features exist in the backend but aren't used in the current frontend UI

> **These unused backend controllers are actually a great talking point** — you can say "I built the backend to be extensible, these modules are ready for future features."
