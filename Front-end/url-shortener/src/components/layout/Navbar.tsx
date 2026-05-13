import { Link, useLocation } from 'react-router-dom'
import { Zap } from 'lucide-react'
import { useAuth } from '../../context/AuthContext'
import { Button } from '../ui/Button'

export function Navbar() {
  const { isAuthenticated, logout, user } = useAuth()
  const location = useLocation()
  const isDashboard = location.pathname.startsWith('/dashboard') || location.pathname.startsWith('/analytics')

  if (isDashboard) return null // Sidebar replaces navbar in app views

  return (
    <header
      className="fixed top-0 left-0 right-0 z-50 flex items-center justify-between px-6 py-4"
      style={{
        background: 'rgba(10,10,11,0.7)',
        backdropFilter: 'blur(20px)',
        borderBottom: '1px solid rgba(255,255,255,0.06)',
      }}
    >
      <Link to="/" className="flex items-center gap-2 font-bold text-lg">
        <span
          className="w-8 h-8 rounded-lg flex items-center justify-center"
          style={{ background: 'linear-gradient(135deg, #6366F1, #14B8A6)' }}
        >
          <Zap size={16} className="text-white" />
        </span>
        <span className="gradient-text">Shortly</span>
      </Link>

      <nav className="hidden md:flex items-center gap-6 text-sm text-[var(--color-text-muted)]">
        <a href="#features" className="hover:text-white transition-colors">Features</a>
        <a href="#pricing" className="hover:text-white transition-colors">Pricing</a>
      </nav>

      <div className="flex items-center gap-3">
        {isAuthenticated ? (
          <>
            <span className="text-sm text-[var(--color-text-muted)] hidden sm:block">
              Hi, {user?.username}
            </span>
            <Link to="/dashboard">
              <Button size="sm">Dashboard</Button>
            </Link>
            <Button size="sm" variant="ghost" onClick={logout}>Logout</Button>
          </>
        ) : (
          <>
            <Link to="/login">
              <Button size="sm" variant="ghost">Login</Button>
            </Link>
            <Link to="/register">
              <Button size="sm">Get Started</Button>
            </Link>
          </>
        )}
      </div>
    </header>
  )
}
