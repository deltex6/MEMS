---
applyTo: `**/*.*`
description: `Structure.`
---
# Project structure for Medical Equipment CRUD (Razor Pages, .NET 8, PostgreSQL/Npgsql)

Scope
- Small, simple CRUD to manage medical equipment data
- Frontend: Razor Pages + Bootstrap
- Backend: ASP.NET Core (.NET 8)
- Database: PostgreSQL (desktop), driver: Npgsql (ADO.NET)

Solution layout
- `Models/Equipment.cs` — domain model + validation attributes
- `Data/EquipmentRepository.cs` — ADO.NET CRUD for `equipment` table (uses `AddNpgsqlDataSource`, no factory class)
- `Database/schema.pgsql` — SQL to create database objects (PostgreSQL)
- `Pages/Equipment/Index.cshtml` + `.cshtml.cs` — list/search
- `Pages/Equipment/Create.cshtml` + `.cshtml.cs` — create
- `Pages/Equipment/Edit.cshtml` + `.cshtml.cs` — edit
- `Pages/Equipment/Details.cshtml` + `.cshtml.cs` — read one
- `Pages/Equipment/Delete.cshtml` + `.cshtml.cs` — delete
- `appsettings.json` — `ConnectionStrings:DefaultConnection` for PostgreSQL (placeholder only; actual secret via User Secrets/env vars)

NuGet packages (required)
- `Npgsql`
- `Npgsql.DependencyInjection` (for `AddNpgsqlDataSource`)

Configuration
- `appsettings.json` example (DO NOT commit real passwords):
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Port=5432;Database=crud_test;Username=postgres;Password=CHANGE_ME"
    }
  }
  ```
- `Program.cs` service wiring (replacement for factory + raw connections):
  ```csharp
  using Npgsql;

  // ... in Program.cs before builder.Build()
  builder.Services.AddNpgsqlDataSource(
      builder.Configuration.GetConnectionString("DefaultConnection")!);
  builder.Services.AddScoped<CRUD_test.Data.EquipmentRepository>();
  ```

Database schema
- File: `Database/schema.pgsql` (PostgreSQL syntax)
- Table: `equipment`
  ```sql
  create table if not exists equipment (
      id serial primary key,
      name varchar(100) not null,
      category varchar(50) not null,
      manufacturer varchar(100) null,
      model varchar(100) null,
      serial_number varchar(100) null,
      purchase_date date null,
      location varchar(100) null,
      status varchar(20) not null default 'Active',
      notes text null,
      last_service_date date null,
      next_service_due date null
  );
  create index if not exists ix_equipment_name on equipment(name);
  ```

Model
- File: `Models/Equipment.cs`
  ```csharp
  using System.ComponentModel.DataAnnotations;

  namespace CRUD_test.Models;

  public enum EquipmentStatus { Active, InMaintenance, Retired }

  public class Equipment
  {
      public int Id { get; set; }

      [Required, StringLength(100)]
      public string Name { get; set; } = string.Empty;

      [Required, StringLength(50)]
      public string Category { get; set; } = string.Empty;

      [StringLength(100)]
      public string? Manufacturer { get; set; }

      [StringLength(100)]
      public string? Model { get; set; }

      [StringLength(100)]
      public string? SerialNumber { get; set; }

      [DataType(DataType.Date)]
      public DateOnly? PurchaseDate { get; set; }

      [StringLength(100)]
      public string? Location { get; set; }

      [Required]
      public EquipmentStatus Status { get; set; } = EquipmentStatus.Active;

      [DataType(DataType.MultilineText)]
      public string? Notes { get; set; }

      [DataType(DataType.Date)]
      public DateOnly? LastServiceDate { get; set; }

      [DataType(DataType.Date)]
      public DateOnly? NextServiceDue { get; set; }
  }
  ```

Data access (uses AddNpgsqlDataSource)
- File: `Data/EquipmentRepository.cs`
  ```csharp
  using CRUD_test.Models;
  using Npgsql;

  namespace CRUD_test.Data;

  public class EquipmentRepository
  {
      private readonly NpgsqlDataSource _dataSource;
      public EquipmentRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

      public async Task<List<Equipment>> GetAllAsync(string? search)
      {
          await using var conn = await _dataSource.OpenConnectionAsync();
          await using var cmd = conn.CreateCommand();
          if (!string.IsNullOrWhiteSpace(search))
          {
              cmd.CommandText = "select id,name,category,manufacturer,model,serial_number,purchase_date,location,status,notes,last_service_date,next_service_due from equipment where name ilike @p order by name";
              cmd.Parameters.AddWithValue("p", $"%{search}%");
          }
          else
          {
              cmd.CommandText = "select id,name,category,manufacturer,model,serial_number,purchase_date,location,status,notes,last_service_date,next_service_due from equipment order by name";
          }

          var list = new List<Equipment>();
          await using var reader = await cmd.ExecuteReaderAsync();
          while (await reader.ReadAsync())
          {
              list.Add(Map(reader));
          }
          return list;
      }

      public async Task<Equipment?> GetAsync(int id)
      {
          await using var conn = await _dataSource.OpenConnectionAsync();
          await using var cmd = new NpgsqlCommand("select id,name,category,manufacturer,model,serial_number,purchase_date,location,status,notes,last_service_date,next_service_due from equipment where id=@id", conn);
          cmd.Parameters.AddWithValue("id", id);
          await using var reader = await cmd.ExecuteReaderAsync();
          return await reader.ReadAsync() ? Map(reader) : null;
      }

      public async Task<int> CreateAsync(Equipment e)
      {
          await using var conn = await _dataSource.OpenConnectionAsync();
          const string sql = @"insert into equipment (name,category,manufacturer,model,serial_number,purchase_date,location,status,notes,last_service_date,next_service_due)
                               values (@name,@category,@manufacturer,@model,@serial_number,@purchase_date,@location,@status,@notes,@last_service_date,@next_service_due)
                               returning id";
          await using var cmd = new NpgsqlCommand(sql, conn);
          FillParams(cmd, e);
          var id = (int)(await cmd.ExecuteScalarAsync() ?? 0);
          return id;
      }

      public async Task<bool> UpdateAsync(Equipment e)
      {
          await using var conn = await _dataSource.OpenConnectionAsync();
          const string sql = @"update equipment set name=@name, category=@category, manufacturer=@manufacturer, model=@model,
                               serial_number=@serial_number, purchase_date=@purchase_date, location=@location, status=@status,
                               notes=@notes, last_service_date=@last_service_date, next_service_due=@next_service_due where id=@id";
          await using var cmd = new NpgsqlCommand(sql, conn);
          FillParams(cmd, e);
          cmd.Parameters.AddWithValue("id", e.Id);
          var rows = await cmd.ExecuteNonQueryAsync();
          return rows == 1;
      }

      public async Task<bool> DeleteAsync(int id)
      {
          await using var conn = await _dataSource.OpenConnectionAsync();
          await using var cmd = new NpgsqlCommand("delete from equipment where id=@id", conn);
          cmd.Parameters.AddWithValue("id", id);
          var rows = await cmd.ExecuteNonQueryAsync();
          return rows == 1;
      }

      private static Equipment Map(NpgsqlDataReader r)
      {
          return new Equipment
          {
              Id = r.GetInt32(0),
              Name = r.GetString(1),
              Category = r.GetString(2),
              Manufacturer = r.IsDBNull(3) ? null : r.GetString(3),
              Model = r.IsDBNull(4) ? null : r.GetString(4),
              SerialNumber = r.IsDBNull(5) ? null : r.GetString(5),
              PurchaseDate = r.IsDBNull(6) ? null : DateOnly.FromDateTime(r.GetDateTime(6)),
              Location = r.IsDBNull(7) ? null : r.GetString(7),
              Status = Enum.Parse<EquipmentStatus>(r.GetString(8)),
              Notes = r.IsDBNull(9) ? null : r.GetString(9),
              LastServiceDate = r.IsDBNull(10) ? null : DateOnly.FromDateTime(r.GetDateTime(10)),
              NextServiceDue = r.IsDBNull(11) ? null : DateOnly.FromDateTime(r.GetDateTime(11))
          };
      }

      private static void FillParams(NpgsqlCommand cmd, Equipment e)
      {
          cmd.Parameters.AddWithValue("name", e.Name);
          cmd.Parameters.AddWithValue("category", e.Category);
          cmd.Parameters.AddWithValue("manufacturer", (object?)e.Manufacturer ?? DBNull.Value);
          cmd.Parameters.AddWithValue("model", (object?)e.Model ?? DBNull.Value);
          cmd.Parameters.AddWithValue("serial_number", (object?)e.SerialNumber ?? DBNull.Value);
          cmd.Parameters.AddWithValue("purchase_date", e.PurchaseDate.HasValue ? e.PurchaseDate.Value.ToDateTime(TimeOnly.MinValue) : (object)DBNull.Value);
          cmd.Parameters.AddWithValue("location", (object?)e.Location ?? DBNull.Value);
          cmd.Parameters.AddWithValue("status", e.Status.ToString());
          cmd.Parameters.AddWithValue("notes", (object?)e.Notes ?? DBNull.Value);
          cmd.Parameters.AddWithValue("last_service_date", e.LastServiceDate.HasValue ? e.LastServiceDate.Value.ToDateTime(TimeOnly.MinValue) : (object)DBNull.Value);
          cmd.Parameters.AddWithValue("next_service_due", e.NextServiceDue.HasValue ? e.NextServiceDue.Value.ToDateTime(TimeOnly.MinValue) : (object)DBNull.Value);
      }
  }
  ```

Razor Pages
- Folder: `Pages/Equipment`
- `Index`
  - Displays table of equipment
  - Optional search by `Name`
  - Actions: `Details`, `Edit`, `Delete`, `Create new`
- `Create`
  - Form fields: `Name`, `Category`, `Manufacturer`, `Model`, `SerialNumber`, `PurchaseDate`, `Location`, `Status`, `Notes`, `LastServiceDate`, `NextServiceDue`
- `Edit`
  - Same fields as `Create`
- `Details`
  - Read-only view of all fields
- `Delete`
  - Confirmation screen, then POST to delete

Page models (pattern)
- In each `*.cshtml.cs`, inject `EquipmentRepository`
- Use async handlers: `OnGetAsync`, `OnPostAsync`
- Apply `[BindProperty]` for posted `Equipment`
- Redirect to `Index` after create/update/delete

Validation/UI
- Use Bootstrap form classes
- Enable client-side validation scripts
- Use tag helpers for inputs and validation messages

Routing
- Root menu entry in `_Layout.cshtml` linking to `asp-page="/Equipment/Index"`

Security
- Anti-forgery enabled by default for POST

Secrets and configuration management (do not commit secrets)
- Use .NET Secret Manager for local development:
  - Initialize once in the project directory: `dotnet user-secrets init`
  - Set connection string: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=crud_test;Username=postgres;Password=YOUR_SECRET"`
- Environment variables (e.g., staging/production):
  - Set `ConnectionStrings__DefaultConnection` to the connection string
- GitHub repository secrets (if using CI/CD):
  - In GitHub repo: Settings → Secrets and variables → Actions → New repository secret
  - Name: `DEFAULT_CONNECTION` (example) with the connection string value
  - Reference in workflows as `${{ secrets.DEFAULT_CONNECTION }}` and set env var `ConnectionStrings__DefaultConnection`
- Ensure you never commit real passwords; keep `appsettings.json` with placeholders only

Testing (manual)
- Run `Database/schema.pgsql` in PostgreSQL
- Ensure `ConnectionStrings:DefaultConnection` is provided by User Secrets or env var
- Create, list, edit, delete equipment
