-- =====================================================================
--  Ancora — student records for psychologists
--  Run ONCE in the Supabase SQL Editor (safe to run again).
--
--  1. Creates the permanent "student_records" table (entries psychologists save on a student's file).
--  2. Lets psychologists and admins READ every student's profile and appointments
--     (so the Student records page shows all students, not only the psychologist's own clients).
--  3. Lets psychologists add entries, and edit only the entries they wrote.
--     There is deliberately NO delete policy: records are kept.
--  Session notes (clinical_notes) are NOT changed: they stay private to the assigned psychologist and admin.
-- =====================================================================

-- Helper: is the signed-in user a psychologist or admin?
-- SECURITY DEFINER so it can read profiles without triggering profiles' own row-level security (no recursion).
create or replace function public.ancora_is_staff()
returns boolean
language sql
stable
security definer
set search_path = public
as $$
  select exists (
    select 1 from public.profiles p
    where p.id = auth.uid() and p.role in ('psychologist', 'admin')
  );
$$;

create or replace function public.ancora_is_admin()
returns boolean
language sql
stable
security definer
set search_path = public
as $$
  select exists (
    select 1 from public.profiles p
    where p.id = auth.uid() and p.role = 'admin'
  );
$$;

-- ---------- The records table ----------
create table if not exists public.student_records (
  id             uuid primary key default gen_random_uuid(),
  student_id     uuid not null,                       -- profiles.id of the student (kept even if the account is removed)
  student_name   text not null default '',
  student_number text not null default '',
  appointment_id uuid,                                -- optional: the session this entry relates to
  author_id      uuid not null,                       -- the psychologist who wrote it
  author_name    text not null default '',
  category       text not null default 'General note',
  title          text not null,
  notes          text not null,
  created_at     timestamptz not null default now(),
  updated_at     timestamptz not null default now()
);

create index if not exists student_records_student_idx on public.student_records (student_id, created_at desc);
create index if not exists student_records_number_idx  on public.student_records (student_number);

alter table public.student_records enable row level security;

drop policy if exists "staff read student records"   on public.student_records;
drop policy if exists "psychologists add records"    on public.student_records;
drop policy if exists "authors edit own records"     on public.student_records;

create policy "staff read student records" on public.student_records
  for select to authenticated using (public.ancora_is_staff());

create policy "psychologists add records" on public.student_records
  for insert to authenticated
  with check (
    author_id = auth.uid()
    and exists (select 1 from public.profiles p where p.id = auth.uid() and p.role = 'psychologist')
  );

create policy "authors edit own records" on public.student_records
  for update to authenticated
  using (author_id = auth.uid() and public.ancora_is_staff())
  with check (author_id = auth.uid());

-- (no DELETE policy on purpose: records are permanent)

-- ---------- Let staff see every student (profiles + appointments) ----------
drop policy if exists "staff read student profiles" on public.profiles;
create policy "staff read student profiles" on public.profiles
  for select to authenticated using (public.ancora_is_staff());

drop policy if exists "staff read all appointments" on public.appointments;
create policy "staff read all appointments" on public.appointments
  for select to authenticated using (public.ancora_is_staff());
