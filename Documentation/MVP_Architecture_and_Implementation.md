# AnCora Startup MVP (Minimal but Scalable)

## 1) System architecture

### High-level components
- **Web Client (React + TypeScript)**: student/counsellor booking UI.
- **API (ASP.NET Core Web API)**: auth, counsellor availability, appointment lifecycle.
- **PostgreSQL (Supabase-compatible)**: durable transactional data store.

### Runtime flow
1. User registers or logs in via `/api/auth/*` and receives JWT.
2. Student loads available counsellors and open slots via `/api/counsellors`.
3. Student books a slot via `/api/appointments`; API locks the slot as booked.
4. Users retrieve their appointment list via `/api/appointments/me`.
5. Counsellors create availability via `/api/availability`.

### Scalability choices
- Stateless API with JWT authentication for horizontal scaling.
- PostgreSQL relational schema with indexes and uniqueness constraints.
- Layered backend structure (`Domain`, `Infrastructure`, `Controllers`, `Services`) for extensibility.
- CORS + health endpoint + centralized error boundary for production operations.

## 2) File structure

```text
src/
  CounsellingServices.slnx
  backend/
    CounsellingServices.Api/
      Controllers/
        AuthController.cs
        CounsellorsController.cs
        AvailabilityController.cs
        AppointmentsController.cs
      Domain/Entities/
        User.cs
        UserRole.cs
        CounsellorProfile.cs
        AvailabilitySlot.cs
        Appointment.cs
        AppointmentStatus.cs
      DTOs/
      Extensions/
      Infrastructure/
        AppDbContext.cs
      Services/
        ITokenService.cs
        JwtTokenService.cs
      Program.cs
      appsettings.json
  frontend/
    counselling-web/
      src/
        App.tsx
        App.css
        index.css
```

## 3) Database schema (PostgreSQL)

### users
- `id` (uuid, PK)
- `full_name` (varchar(120), required)
- `email` (varchar(160), unique, required)
- `password_hash` (text, required)
- `role` (int enum: Student, Counsellor, Admin)

### counsellor_profiles
- `id` (uuid, PK)
- `user_id` (uuid, unique FK -> users.id)
- `registration_number` (varchar, unique)
- `specialty` (varchar(120), required)

### availability_slots
- `id` (uuid, PK)
- `counsellor_profile_id` (uuid FK -> counsellor_profiles.id)
- `starts_at` (timestamptz)
- `ends_at` (timestamptz)
- `is_booked` (bool)
- Unique index on (`counsellor_profile_id`, `starts_at`, `ends_at`)

### appointments
- `id` (uuid, PK)
- `student_id` (uuid FK -> users.id)
- `counsellor_profile_id` (uuid FK -> counsellor_profiles.id)
- `availability_slot_id` (uuid FK -> availability_slots.id)
- `status` (int enum: Pending, Confirmed, Completed, Cancelled)
- `reason` (varchar(400), required)
- `created_at` (timestamptz)

## 4) API endpoints

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`

### Counsellors and availability
- `GET /api/counsellors`
- `POST /api/availability` (Counsellor/Admin)

### Appointments
- `POST /api/appointments` (Student/Admin)
- `GET /api/appointments/me` (Authenticated)
- `PATCH /api/appointments/{appointmentId}/cancel` (Student/Admin)
- `PATCH /api/appointments/{appointmentId}/status` (Counsellor/Admin)

### Ops
- `GET /health`

## 5) UI architecture

- **Single-page client** with three domains:
  - Authentication (register/login)
  - Availability discovery (counsellor + slot list)
  - Appointment management (book + list own bookings)
- State model: local component state for MVP (`token`, `counsellors`, `appointments`).
- API integration: explicit fetch actions for each use case, JWT attached in `Authorization` header.

## 6) Production-ready baseline included

- Backend: JWT auth, CORS policy, health checks, typed DTOs, EF Core + PostgreSQL provider, validation guards, consistent HTTP responses.
- Frontend: typed React app for registration/login/booking flows.
- This is intentionally minimal and ready for next-phase additions (notifications, analytics, role dashboards, audit logs).
