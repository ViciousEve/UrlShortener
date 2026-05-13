import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Mail, Lock, User, Eye, EyeOff, Zap, GitBranch } from 'lucide-react'
import { Button } from '../../components/ui/Button'
import { Input } from '../../components/ui/Input'
import { useAuth } from '../../context/AuthContext'
import { useToast } from '../../components/layout/ToastProvider'

export function RegisterPage() {
  const [email, setEmail] = useState('')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPw, setShowPw] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  const { register } = useAuth()
  const navigate = useNavigate()
  const { success, error } = useToast()

  const getPasswordStrength = (pw: string) => {
    if (!pw) return null
    if (pw.length < 6) return { label: 'Too short', color: '#EF4444', width: '25%' }
    if (pw.length < 8) return { label: 'Weak', color: '#F59E0B', width: '45%' }
    if (!/[A-Z]/.test(pw) || !/[0-9]/.test(pw)) return { label: 'Fair', color: '#F59E0B', width: '65%' }
    return { label: 'Strong', color: '#10B981', width: '100%' }
  }

  const strength = getPasswordStrength(password)

  const validate = () => {
    const e: Record<string, string> = {}
    if (!email) e.email = 'Email is required'
    else if (!/\S+@\S+\.\S+/.test(email)) e.email = 'Enter a valid email'
    if (!username) e.username = 'Username is required'
    else if (username.length < 3) e.username = 'Username must be at least 3 characters'
    if (!password) e.password = 'Password is required'
    else if (password.length < 6) e.password = 'Password must be at least 6 characters'
    return e
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const errs = validate()
    if (Object.keys(errs).length) { setErrors(errs); return }
    setIsLoading(true)
    setErrors({})
    try {
      await register(email, username, password)
      success('Account created! Welcome to Shortly.')
      navigate('/dashboard')
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      setErrors({ form: msg ?? 'Registration failed. Please try again.' })
      error('Registration failed')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-12">
      <div className="blob-indigo" />
      <div className="blob-teal" />

      <motion.div
        initial={{ opacity: 0, y: 24, scale: 0.97 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        transition={{ duration: 0.45, ease: 'easeOut' }}
        className="w-full max-w-md page-content"
      >
        <div className="flex justify-center mb-8">
          <Link to="/" className="flex items-center gap-2 font-bold text-xl">
            <span
              className="w-9 h-9 rounded-xl flex items-center justify-center"
              style={{ background: 'linear-gradient(135deg, #6366F1, #14B8A6)' }}
            >
              <Zap size={18} className="text-white" />
            </span>
            <span className="gradient-text">Shortly</span>
          </Link>
        </div>

        <div className="glass-strong p-8 rounded-2xl">
          <h1 className="text-2xl font-bold text-white mb-1">Create account</h1>
          <p className="text-sm text-[var(--color-text-muted)] mb-7">Start shortening links for free</p>

          {errors.form && (
            <div className="mb-4 p-3 rounded-xl text-sm" style={{ background: 'rgba(239,68,68,0.1)', border: '1px solid rgba(239,68,68,0.3)', color: '#EF4444' }}>
              {errors.form}
            </div>
          )}

          <form id="register-form" onSubmit={handleSubmit} className="flex flex-col gap-4">
            <Input
              id="register-email"
              label="Email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              leftIcon={<Mail size={15} />}
              error={errors.email}
              autoComplete="email"
            />
            <Input
              id="register-username"
              label="Username"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="cooluser"
              leftIcon={<User size={15} />}
              error={errors.username}
              autoComplete="username"
            />
            <div className="flex flex-col gap-1.5">
              <Input
                id="register-password"
                label="Password"
                type={showPw ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Min 8 chars, uppercase & number"
                leftIcon={<Lock size={15} />}
                error={errors.password}
                autoComplete="new-password"
                rightElement={
                  <button
                    type="button"
                    onClick={() => setShowPw(!showPw)}
                    className="text-[var(--color-text-muted)] hover:text-white transition-colors"
                    tabIndex={-1}
                  >
                    {showPw ? <EyeOff size={15} /> : <Eye size={15} />}
                  </button>
                }
              />
              {/* Password strength bar */}
              {strength && (
                <div className="flex items-center gap-2">
                  <div className="flex-1 h-1 bg-white/10 rounded-full overflow-hidden">
                    <div
                      className="h-full rounded-full transition-all duration-300"
                      style={{ width: strength.width, backgroundColor: strength.color }}
                    />
                  </div>
                  <span className="text-xs" style={{ color: strength.color }}>{strength.label}</span>
                </div>
              )}
            </div>

            <Button
              id="register-submit-btn"
              type="submit"
              isLoading={isLoading}
              className="w-full mt-1"
              size="lg"
            >
              Create Account
            </Button>
          </form>

          <div className="flex items-center gap-3 my-5">
            <div className="flex-1 h-px bg-white/8" />
            <span className="text-xs text-[var(--color-text-muted)]">or sign up with</span>
            <div className="flex-1 h-px bg-white/8" />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <button id="google-register-btn" className="flex items-center justify-center gap-2 py-2.5 glass rounded-xl text-sm text-[var(--color-text-muted)] opacity-50 cursor-not-allowed" disabled>
              <svg width="16" height="16" viewBox="0 0 24 24"><path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/><path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/><path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/><path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/></svg>
              Google
            </button>
            <button id="github-register-btn" className="flex items-center justify-center gap-2 py-2.5 glass rounded-xl text-sm text-[var(--color-text-muted)] opacity-50 cursor-not-allowed" disabled>
              <GitBranch size={16} />
              GitHub
            </button>
          </div>

          <p className="text-center text-sm text-[var(--color-text-muted)] mt-6">
            Already have an account?{' '}
            <Link to="/login" className="text-[var(--color-indigo-light)] hover:underline font-medium">
              Sign in
            </Link>
          </p>
        </div>
      </motion.div>
    </div>
  )
}
