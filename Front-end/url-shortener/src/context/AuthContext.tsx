import { createContext, useContext, useState, useEffect, type ReactNode } from 'react'
import axiosInstance from '../api/axiosInstance'

interface User {
  id: string
  email: string
  username: string
}

interface AuthContextType {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, username: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

/** Decode a JWT and extract the user fields embedded as claims. */
function parseUserFromToken(jwt: string): User {
  const payload = JSON.parse(atob(jwt.split('.')[1]))
  return {
    id: payload.sub,
    email: payload.email,
    // JwtRegisteredClaimNames.Name maps to the "name" claim key in the token
    username: payload.name ?? payload.unique_name ?? '',
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [token, setToken] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const storedToken = localStorage.getItem('shortly_token')
    const storedUser = localStorage.getItem('shortly_user')
    if (storedToken && storedUser) {
      setToken(storedToken)
      setUser(JSON.parse(storedUser))
    }
    setIsLoading(false)
  }, [])

  const login = async (email: string, password: string) => {
    const res = await axiosInstance.post('/identity/login', { email, password })
    // Backend returns { accessToken, expiresAtUtc } — no separate user object
    const { accessToken } = res.data as { accessToken: string; expiresAtUtc: string }
    const u = parseUserFromToken(accessToken)
    setToken(accessToken)
    setUser(u)
    localStorage.setItem('shortly_token', accessToken)
    localStorage.setItem('shortly_user', JSON.stringify(u))
  }

  const register = async (email: string, username: string, password: string) => {
    const res = await axiosInstance.post('/identity/register', { email, username, password })
    const { accessToken } = res.data as { accessToken: string; expiresAtUtc: string }
    const u = parseUserFromToken(accessToken)
    setToken(accessToken)
    setUser(u)
    localStorage.setItem('shortly_token', accessToken)
    localStorage.setItem('shortly_user', JSON.stringify(u))
  }

  const logout = () => {
    setToken(null)
    setUser(null)
    localStorage.removeItem('shortly_token')
    localStorage.removeItem('shortly_user')
  }

  return (
    <AuthContext.Provider value={{ user, token, isAuthenticated: !!token, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
