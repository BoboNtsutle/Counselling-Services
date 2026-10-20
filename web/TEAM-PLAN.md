# Team plan: who builds what

Six people, six modules. Each module has its **own service file, controller(s) and views**, so two people
should never need to edit the same file. That's what keeps GitHub merge conflicts away.

Write your name next to your module and commit this file as your first PR (good practice for the workflow).

| # | Module | Owner | SRS | Your files |
|---|---|---|---|---|
| **M1** | Foundation, login, my profile + **GitHub lead** | T Mofokeng (Tshepiso) | FR-01, FR-10 | `Controllers/AccountController.cs`, `Controllers/ProfileController.cs`, `Services/ProfileService.cs`, `Views/Account/*`, `Views/Profile/*`, plus everything in `Services/Supabase/`, `Program.cs`, `_Layout.cshtml`, `site.css` |
| **M2** | Admin: appointments + double-booking guard | __________ | FR-03, FR-04 | `Services/AppointmentAdminService.cs`, `Areas/Admin/Controllers/AppointmentsController.cs`, `Areas/Admin/Views/Appointments/*` |
| **M3** | Admin: users, staff role requests, student records + search | __________ | FR-06, FR-07, FR-10 | `Services/UserAdminService.cs`, `Areas/Admin/Controllers/UsersController.cs`, `Areas/Admin/Views/Users/*` |
| **M4** | Admin: announcements, programmes & events (image upload) | __________ | FR-08, FR-12 | `Services/AnnouncementService.cs`, `Areas/Admin/Controllers/AnnouncementsController.cs`, `Areas/Admin/Views/Announcements/*` |
| **M5** | Psychologist portal: my appointments, status, session notes | __________ | FR-05, FR-09 | `Services/PsychologistService.cs`, `Areas/Psychologist/**` |
| **M6** | Admin dashboard + statistics & reports | __________ | FR-11 | `Services/ReportService.cs`, `Areas/Admin/Controllers/DashboardController.cs`, `Areas/Admin/Controllers/ReportsController.cs`, `Areas/Admin/Views/Dashboard/*`, `Areas/Admin/Views/Reports/*` |

Group members: F Mahlumba · IM Molefi · TC Selepe · N Dlali · BBL Ntsutle · T Mofokeng

**Tip:** Give the psychologist portal (M5) to whoever designed the psychologist prototype screens, and the admin
modules to whoever designed the admin screens. They already know how the pages should look.

---

## What each module must deliver

Tick these off in your PR description. ✅ = already working in the starter.

### M1 · Foundation & login (lead)
- ✅ Login with Supabase (same accounts as the Flutter app); students are blocked
- ✅ Logout, forgot password, access-denied page, role-based redirect
- ✅ Shared layout, sidebar, colours, error messages
- [ ] My profile: edit name and phone (`ProfileService.UpdateMyProfileAsync`)
- [ ] Set up the GitHub repo, branch protection and collaborators (see `CONTRIBUTING.md`)
- [ ] Review and merge everyone's PRs; own the shared files
- [ ] Stretch: deploy (e.g. Azure App Service free tier) for the demo

### M2 · Appointments (Must have)
- ✅ List all appointments, filter by status; details page
- [ ] Assign psychologist + **Approve**
- [ ] **FR-04 double-booking guard**: refuse approval if that psychologist already has an approved session at the same date and time
- [ ] **Reject**, **Reschedule**
- [ ] Search by student name/number; urgent requests first

### M3 · Users & records (Must have)
- ✅ List pending staff role requests + list psychologists
- [ ] **Approve / Reject** role requests (FR-10)
- [ ] **Student search** by name, student number, email (FR-07)
- [ ] **Student record** page: profile + appointment history (FR-06)

### M4 · Announcements (Could have, but small and visible in the demo)
- ✅ List announcements (with images)
- [ ] **New announcement** form with optional image (uploads to the `announcement-images` bucket)
- [ ] Delete announcement
- [ ] Check it appears in the Flutter app straight away

### M5 · Psychologist portal (Must have)
- ✅ My appointments list with counters; details page
- [ ] **Status buttons**: Approve / Reject / Mark complete (FR-09)
- [ ] **Session notes**: private, save/load (`clinical_notes` table)
- [ ] Filter: upcoming / completed

### M6 · Dashboard & statistics (Must have: "Statistics (raw)")
- ✅ Dashboard page with "pending requests" count + latest requests
- [ ] Fill the other 3 dashboard cards
- [ ] **Statistics page**: tables by status, urgency, reason, mood, psychologist, month, with a date filter
- [ ] Numbers/tables only: the SRS lists graphs as *Won't have*
- [ ] Stretch: Download as CSV

---

## Decisions to make together (before M2 and M6 start)

1. **Date & time format (important).** The Flutter booking form saves `appointment_date` as text like
   `"Fri, Oct 16"` (from `formatMediumDate`). That has **no year** and can't be sorted, and the time is saved as `"10:00 AM"`.
   This will break the double-booking check (M2) and the monthly statistics (M6).
   👉 Ask the Flutter team to save `yyyy-MM-dd` (e.g. `2026-10-16`) and 24-hour `HH:mm` (e.g. `14:00`).
   It's a two-line change in `appointment_home_view.dart` around line 1496.
2. **Double-booking in the database too (recommended).** The web check (M2) only protects the website. Running this
   once in the Supabase SQL Editor protects both apps:
   ```sql
   create unique index if not exists one_approved_session_per_slot
     on public.appointments (psychologist_id, appointment_date, appointment_time)
     where status = 'Approved';
   ```
   (If it errors with "could not create unique index", there's already a double booking in the data. Fix that row first.)
3. **Psychologist availability** (*Should have*) needs a new table. Leave it until all *Must haves* are merged.

## Suggested order

| Phase | Goal |
|---|---|
| **0 · Setup (day 1)** | M1 creates the repo. Everyone clones, runs the app and logs in. Everyone opens a tiny PR (add your name above). |
| **1 · Must haves** | M2 approve/reject + clash check · M3 role requests + search · M5 status + notes · M6 dashboard cards |
| **2 · Should/Could** | Reschedule, announcements with images, statistics page, profile edit |
| **3 · Test & demo** | Run through the test checklist below on the shared Supabase project, fix bugs, prepare the demo |

## Test checklist (do this before the demo)

- [ ] Student account cannot log in to the website
- [ ] Psychologist cannot open any `/Admin` page (gets "You can't open that page")
- [ ] Psychologist only sees **their own** appointments; opening someone else's appointment id shows 404
- [ ] Admin approves a request → the student sees it as *Approved* in the Flutter app
- [ ] Two approvals for the same psychologist + slot → the second is refused (FR-04)
- [ ] Announcement published on the web → shows in the Flutter app
- [ ] Session notes are **not** visible to students
- [ ] Statistics numbers match a manual count in the Supabase table editor
