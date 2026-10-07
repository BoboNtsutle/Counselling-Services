import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type UserRole = 'Student' | 'Counsellor' | 'Admin'

type AuthResponse = {
  accessToken: string
  expiresAt: string
  role: UserRole
  userId: string
}

type CounsellorSlot = {
  slotId: string
  startsAt: string
  endsAt: string
}

type Counsellor = {
  counsellorProfileId: string
  name: string
  specialty: string
  availableSlots: CounsellorSlot[]
}

type Appointment = {
  appointmentId: string
  counsellor: string
  startsAt: string
  status: string
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

function App() {
  const [email, setEmail] = useState('student@ancora.local')
  const [password, setPassword] = useState('Passw0rd!')
  const [fullName, setFullName] = useState('Student User')
  const [token, setToken] = useState<string | null>(null)
  const [role, setRole] = useState<UserRole>('Student')
  const [counsellors, setCounsellors] = useState<Counsellor[]>([])
  const [appointments, setAppointments] = useState<Appointment[]>([])
  const [selectedSlot, setSelectedSlot] = useState<string>('')
  const [reason, setReason] = useState('Stress management support')
  const [message, setMessage] = useState<string>('')

  const availableSlots = useMemo(
    () => counsellors.flatMap((c) => c.availableSlots.map((slot) => ({ ...slot, counsellorName: c.name }))),
    [counsellors],
  )

  const authHeaders = new Headers()
  if (token) {
    authHeaders.set('Authorization', ['Bearer', token].join(' '))
  }

  async function register(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setMessage('Registering...')
    const response = await fetch(`${apiBaseUrl}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, fullName, role }),
    })

    if (!response.ok) {
      setMessage(`Register failed: ${await response.text()}`)
      return
    }

    const payload: AuthResponse = await response.json()
    setToken(payload.accessToken)
    setMessage(`Registered and logged in as ${payload.role}.`)
  }

  async function login() {
    setMessage('Signing in...')
    const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    })

    if (!response.ok) {
      setMessage('Login failed. Check credentials.')
      return
    }

    const payload: AuthResponse = await response.json()
    setToken(payload.accessToken)
    setMessage(`Signed in as ${payload.role}.`)
  }

  async function loadCounsellors() {
    const response = await fetch(`${apiBaseUrl}/api/counsellors`)
    if (!response.ok) {
      setMessage('Unable to load counsellors at the moment.')
      return
    }

    const payload: Counsellor[] = await response.json()
    setCounsellors(payload)
    setSelectedSlot(payload[0]?.availableSlots[0]?.slotId ?? '')
  }

  async function bookAppointment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!token || !selectedSlot) {
      setMessage('Please sign in and choose a slot first.')
      return
    }

    const response = await fetch(`${apiBaseUrl}/api/appointments`, {
      method: 'POST',
      headers: {
        Authorization: authHeaders.get('Authorization') ?? '',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ slotId: selectedSlot, reason }),
    })

    if (!response.ok) {
      setMessage(`Booking failed: ${await response.text()}`)
      return
    }

    setMessage('Appointment booked successfully.')
    await loadAppointments()
  }

  async function loadAppointments() {
    if (!token) {
      setMessage('Sign in to view your appointments.')
      return
    }

    const response = await fetch(`${apiBaseUrl}/api/appointments/me`, {
      headers: {
        Authorization: authHeaders.get('Authorization') ?? '',
      },
    })

    if (!response.ok) {
      setMessage('Unable to load appointments.')
      return
    }

    const payload: Appointment[] = await response.json()
    setAppointments(payload)
  }

  return (
    <main className="app">
      <header>
        <h1>AnCora Counselling MVP</h1>
        <p>Minimal scalable booking flow for students and counsellors.</p>
      </header>

      <section className="card">
        <h2>Authentication</h2>
        <form onSubmit={register}>
          <label>
            Full name
            <input value={fullName} onChange={(e) => setFullName(e.target.value)} required />
          </label>
          <label>
            Email
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>
          <label>
            Password
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </label>
          <label>
            Role
            <select value={role} onChange={(e) => setRole(e.target.value as UserRole)}>
              <option value="Student">Student</option>
              <option value="Counsellor">Counsellor</option>
            </select>
          </label>
          <div className="actions">
            <button type="submit">Register</button>
            <button type="button" onClick={login}>Login</button>
          </div>
        </form>
      </section>

      <section className="card">
        <h2>Discover availability</h2>
        <div className="actions">
          <button type="button" onClick={loadCounsellors}>Load counsellors</button>
          <button type="button" onClick={loadAppointments}>Load my appointments</button>
        </div>

        <ul>
          {counsellors.map((counsellor) => (
            <li key={counsellor.counsellorProfileId}>
              <strong>{counsellor.name}</strong> — {counsellor.specialty} ({counsellor.availableSlots.length} slots)
            </li>
          ))}
        </ul>

        <form onSubmit={bookAppointment}>
          <label>
            Slot
            <select value={selectedSlot} onChange={(e) => setSelectedSlot(e.target.value)} required>
              <option value="">Choose a slot</option>
              {availableSlots.map((slot) => (
                <option key={slot.slotId} value={slot.slotId}>
                  {slot.counsellorName} — {new Date(slot.startsAt).toLocaleString()} to {new Date(slot.endsAt).toLocaleTimeString()}
                </option>
              ))}
            </select>
          </label>
          <label>
            Reason
            <textarea value={reason} onChange={(e) => setReason(e.target.value)} required />
          </label>
          <button type="submit">Book appointment</button>
        </form>
      </section>

      <section className="card">
        <h2>My appointments</h2>
        <ul>
          {appointments.map((appointment) => (
            <li key={appointment.appointmentId}>
              <strong>{appointment.counsellor}</strong> — {new Date(appointment.startsAt).toLocaleString()} ({appointment.status})
            </li>
          ))}
        </ul>
      </section>

      {message && <p className="message">{message}</p>}
    </main>
  )
}

export default App
