# Ancora Counselling Services — Staff Web App (ASP.NET Core MVC)

ITC327W · Group D · CUT Wellness Centre

This is the **web** half of the system described in our SRS (slide 06, *System Architecture*):

| Part | Built with | Who uses it |
|---|---|---|
| Mobile app (already built) | Flutter | Students: book appointments, see announcements |
| **Web app (this repo)** | **ASP.NET Core 8 MVC** | **Admin / front office and psychologists** |
| Shared backend | Supabase (PostgreSQL + Auth + Storage) | Both apps use the *same* database and accounts |

> Students are blocked from this website on purpose. They use the Flutter app.

---

## 1. Run it on your laptop (first time)

**You need:** Visual Studio 2022 (with the *ASP.NET and web development* workload) **or** the .NET 8 SDK + VS Code, plus Git.

1. Clone the repo (see `CONTRIBUTING.md`, step 2).
2. Open `AncoraWeb.sln` in Visual Studio.
3. Press **F5** (or run `dotnet run --project src/Ancora.Web` in a terminal).
4. The browser opens on the login page. Sign in with an **admin** or **psychologist** account (section 3).

No extra NuGet packages are needed. The app talks to Supabase with plain `HttpClient`.

## 2. Supabase settings

`src/Ancora.Web/appsettings.json` already has the same Supabase project as the Flutter app
(`lib/supabase_runtime_config.dart`):

```json
"Supabase": {
  "Url": "https://fyulvurlkbdehvwiykuz.supabase.co",
  "PublishableKey": "sb_publishable_..."
}
```

- The **publishable** key can be committed. Supabase's row-level security still protects the data.
- **Never** commit the `service_role` / secret key, or the database password.

## 3. Test accounts

There are no demo users. Make them like this:

| Role | How to get one |
|---|---|
| **Admin** | Create the user in Supabase Dashboard → Authentication → Users, then run `supabase/bootstrap_first_admin.sql` (from the Flutter project) with that email. |
| **Psychologist** | Register in the Flutter app and choose *Psychologist*. The admin then approves it (Module 3 builds that button; until then, change `role` in the `profiles` table in the Supabase dashboard). |
| Student | Register in the Flutter app. They can't log in here, which is correct. |

Share the test logins in the group chat, **not** in the repo.

## 4. How the code is organised

```
src/Ancora.Web/
├── Program.cs                     ← setup (already registers every module's service)
├── appsettings.json               ← Supabase URL + publishable key
├── Models/                        ← C# classes matching the Supabase tables
│   ├── Profile.cs  Appointment.cs  ClinicalNote.cs  Announcement.cs  Roles.cs
│   └── ViewModels/
├── Services/
│   ├── Supabase/                  ← SHARED: login, tokens, the database client (lead owns)
│   ├── ProfileService.cs          ← Module 1
│   ├── AppointmentAdminService.cs ← Module 2
│   ├── UserAdminService.cs        ← Module 3
│   ├── AnnouncementService.cs     ← Module 4
│   ├── PsychologistService.cs     ← Module 5
│   └── ReportService.cs           ← Module 6
├── Controllers/                   ← Account (login) + Profile          — Module 1
├── Areas/Admin/                   ← /Admin/...  (admin only)
│   ├── Controllers/  AppointmentsController (M2) UsersController (M3)
│   │                 AnnouncementsController (M4) DashboardController + ReportsController (M6)
│   └── Views/
├── Areas/Psychologist/            ← /Psychologist/...  (psychologist only) — Module 5
├── Views/Shared/_Layout.cshtml    ← sidebar menu (already has every module's link)
└── wwwroot/css/site.css           ← colours from the Flutter app (navy / wine / cream)
```

Every unfinished piece is marked in two places:
- `// TODO (Module X)` in the C# file, with a hint showing the exact Supabase call.
- A yellow dashed **"To build"** box on the page itself. Delete the box when your part is done.

## 5. Reading and writing data (cheat sheet)

Inject your module's service, which uses `SupabaseClient` (`_db`):

```csharp
// SELECT
var pending = await _db.GetAsync<Appointment>("appointments", "status=eq.Pending&order=created_at.desc");
var one     = await _db.GetSingleAsync<Profile>("profiles", $"id=eq.{id}");
var total   = await _db.CountAsync("appointments", "status=eq.Complete");

// INSERT / UPDATE / UPSERT / DELETE  (use snake_case names, the same as the database columns)
await _db.InsertAsync<Announcement>("announcements", new { title, message, image_url = url });
await _db.UpdateAsync("appointments", $"id=eq.{id}", new { status = "Approved" });
await _db.UpsertAsync("clinical_notes", new { appointment_id = id, notes }, onConflict: "appointment_id");
await _db.DeleteAsync("announcements", $"id=eq.{id}");
```

Filters: `eq.` equals · `neq.` not equal · `gte.` / `lte.` ≥ / ≤ · `ilike.*text*` contains ·
`in.(a,b)` one of · `is.null` · `order=col.desc` · `limit=10` · `or=(a.eq.1,b.eq.2)`

**Errors:** If Supabase refuses something (for example, a psychologist trying to edit someone else's
appointment), a red message appears at the top of the page automatically. You don't need try/catch
unless you want a custom message.

**Logged-in user:** `User.GetUserId()` gives their Supabase id. `User.IsInRole(Roles.Admin)` checks the role.

## 6. Requirements → where they live

| SRS | Requirement | Module |
|---|---|---|
| FR-01 | User authentication | M1 ✅ working |
| FR-10 | Role-based access | M1 ✅ working · M3 approves role requests |
| FR-03 | Appointment management (view, approve, reschedule) | M2 |
| FR-04 | Double-booking guard | M2 |
| FR-06 / FR-07 | Student records + search | M3 |
| FR-08 / FR-12 | Digital communication / announcements | M4 |
| FR-05 / FR-09 | Psychologist view + status updates (+ session notes) | M5 |
| FR-11 | Statistics (non-graphical) | M6 |

See **`TEAM-PLAN.md`** for who does what, and **`CONTRIBUTING.md`** for how we use GitHub.


## Student records (psychologist + admin)

Run `supabase/student_records.sql` **once** in the Supabase SQL Editor. It creates the permanent
`student_records` table and lets psychologists read every student's profile and appointments.
Until it is run, the Student records page still lists students but cannot save or show record entries.
