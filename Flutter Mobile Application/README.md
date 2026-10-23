# Ancora Counselling Services: Flutter Mobile App

Flutter mobile app for the **CUT Wellness Centre** appointment system (Group D). Students request counselling sessions from their phones. Admins and psychologists can also sign in to manage appointments. The app shares one Supabase backend (database, login, file storage) with the Ancora ASP.NET web app, so both apps use the same accounts and data.

## Features

**Student**
- Register and sign in; reset password by email
- Request a session: reason, mood, urgency, weekday date, one-hour slot (08:00 to 17:00)
- View current appointments and history
- Read announcements (with images) in Messages, with an unread badge
- Edit own profile

**Psychologist**
- See only appointments assigned to them
- Confirm, decline or mark appointments complete
- Reschedule own appointments using the availability calendar
- Write private session notes
- View assigned student records

**Admin**
- Approve requests and assign a psychologist
- Reschedule appointments using the availability calendar
- Approve or reject staff role requests
- Search and edit student records
- Publish announcements with an optional image (JPEG, PNG, WebP or GIF, up to 8 MB)
- View reports

## Tech stack

| Area | Technology |
|---|---|
| App | Flutter (Dart, SDK ^3.11.4), Material 3 |
| Architecture | MVVM with `ChangeNotifier` view model and repository interfaces |
| Backend | Supabase: PostgreSQL, Auth, Storage, Realtime |
| Packages | `supabase_flutter`, `image_picker`, `shared_preferences` |
| Tests | `flutter_test` with fake repositories |

## Project structure

```
lib/
  main.dart                          App start-up, Supabase initialisation
  appointment_app.dart               Theme and root widget
  supabase_runtime_config.dart       Supabase URL and publishable key
  models/appointment.dart            AppRole, UserProfile, Appointment, date/time validation helpers
  repositories/
    auth_repository.dart             Auth interface
    appointment_repository.dart      Appointment and announcement interface
    supabase_auth_repository.dart    Supabase sign-in, sign-up, profiles, role requests
    supabase_appointment_repository.dart  Appointments, clinical notes, announcements, storage
  viewmodels/appointment_view_model.dart  App state and workflows for all roles
  views/
    appointment_home_view.dart       Screens for student, psychologist and admin
    admin_availability_view.dart     Availability calendar
supabase/
  schema.sql                         Tables, triggers, row-level security policies
  prevent_double_booking.sql         Unique index against double booking
  bootstrap_first_admin.sql          Promote the first admin account
test/
  viewmodels/appointment_view_model_test.dart
  widget_test.dart
  fakes/fake_repositories.dart
```

## Getting started

### Prerequisites
- Flutter SDK (Dart ^3.11.4) and an Android emulator, iOS simulator or physical device
- A Supabase project (the team's shared project is already configured by default)

### 1. Install dependencies
```bash
flutter pub get
```

### 2. Set up the database
In the Supabase dashboard open **SQL Editor** and run, in this order:
1. `supabase/schema.sql`
2. `supabase/prevent_double_booking.sql`

The web app has its own `supabase/schema.sql` with extra tables (for example psychologist days off). Use the same Supabase project for both apps and keep the schema files consistent. Do not run an older script over a newer database.

### 3. Create the first admin
1. In Supabase go to **Authentication > Users** and create a user. Confirm the email.
2. Put that email into `supabase/bootstrap_first_admin.sql` and run it in the SQL Editor.

Students register inside the app. Psychologist and admin accounts are requested at registration and approved by an admin, so nobody can give themselves a staff role.

### 4. Configure Supabase (optional)
`lib/supabase_runtime_config.dart` contains defaults for the project URL and the **publishable** key. To use another project:

```bash
flutter run \
  --dart-define=SUPABASE_URL=https://your-project.supabase.co \
  --dart-define=SUPABASE_PUBLISHABLE_KEY=sb_publishable_your_key
```

Never put the `service_role` or secret key in this app.

### 5. Run
```bash
flutter run
```

## Tests and checks
```bash
flutter analyze
flutter test
```

## How it works

- **Roles:** `student`, `psychologist`, `admin`, stored in the `profiles` table. The login screen checks that the account's real role matches the chosen role.
- **Appointments:** requests start as `Pending`. Dates use `YYYY-MM-DD`, weekends are unavailable, and sessions are one-hour slots starting 08:00 to 16:00.
- **Security:** row-level security in Supabase decides who can read or change each row. Students see only their own appointments, psychologists only those assigned to them, and admins see all. Database triggers stop users changing their own role and reject invalid dates, times and double bookings.
- **Clinical notes:** stored in a separate `clinical_notes` table, readable only by an admin or the assigned psychologist.
- **Live updates:** appointments and announcements are streamed with Supabase Realtime.



The staff web application (ASP.NET Core 8 MVC) uses the same Supabase project, accounts and tables.

## Team

(Group D) F Mahlumba (Student ID: 219015487) IM Molefi (Student ID: 223048296) TC Selepe (Student ID: 224004059) N Dlali (Student ID: 220039413) BBL Ntsutle (Student ID: 224085810) T Mofokeng (Student ID: 224079447)
