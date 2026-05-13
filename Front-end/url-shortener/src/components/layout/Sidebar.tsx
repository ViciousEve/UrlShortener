import { NavLink, useNavigate } from 'react-router-dom'
import { LayoutDashboard, Link2, BarChart3, Settings, Plus, LogOut, Zap } from 'lucide-react'
import { clsx } from 'clsx'
import { useAuth } from '../../context/AuthContext'

const navItems = [
  { to: '/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
  { to: '/dashboard/links', icon: Link2, label: 'My Links' },
  { to: '/analytics', icon: BarChart3, label: 'Analytics' },
  { to: '/dashboard/settings', icon: Settings, label: 'Settings' },
]

export function Sidebar() {
  const { logout, user } = useAuth()
  const navigate = useNavigate()

  return (
    <aside
      className="fixed left-0 top-0 bottom-0 w-64 flex flex-col z-40"
      style={{
        background: 'rgba(10,10,11,0.85)',
        backdropFilter: 'blur(20px)',
        borderRight: '1px solid rgba(255,255,255,0.06)',
      }}
    >
      {/* Logo */}
      <div className="flex items-center gap-2.5 px-6 py-5 border-b border-white/5">
        <span
          className="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
          style={{ background: 'linear-gradient(135deg, #6366F1, #14B8A6)' }}
        >
          <Zap size={16} className="text-white" />
        </span>
        <span className="font-bold text-lg gradient-text">Shortly</span>
      </div>

      {/* New Link Button */}
      <div className="px-4 py-4">
        <button
          id="sidebar-new-link-btn"
          onClick={() => navigate('/dashboard?new=1')}
          className="w-full flex items-center justify-center gap-2 py-2.5 rounded-xl text-sm font-semibold text-white transition-all duration-200 btn-glow"
          style={{ background: 'linear-gradient(135deg, #6366F1, #4F46E5)', boxShadow: '0 4px 15px rgba(99,102,241,0.3)' }}
        >
          <Plus size={16} />
          New Link
        </button>
      </div>

      {/* Nav */}
      <nav className="flex-1 px-3 py-2 flex flex-col gap-1">
        {navItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/dashboard'}
            className={({ isActive }) =>
              clsx(
                'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-200',
                isActive
                  ? 'text-white'
                  : 'text-[var(--color-text-muted)] hover:text-white hover:bg-white/5'
              )
            }
            style={({ isActive }) =>
              isActive
                ? { background: 'rgba(99,102,241,0.15)', color: '#818CF8', border: '1px solid rgba(99,102,241,0.2)' }
                : {}
            }
          >
            <Icon size={17} />
            {label}
          </NavLink>
        ))}
      </nav>

      {/* User + Logout */}
      <div className="px-4 py-4 border-t border-white/5">
        <div className="flex items-center gap-3 mb-3 px-1">
          <div
            className="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold text-white"
            style={{ background: 'linear-gradient(135deg, #6366F1, #14B8A6)' }}
          >
            {user?.username?.charAt(0).toUpperCase() ?? 'U'}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-white truncate">{user?.username ?? 'User'}</p>
            <p className="text-xs text-[var(--color-text-muted)] truncate">{user?.email ?? ''}</p>
          </div>
        </div>
        <button
          id="sidebar-logout-btn"
          onClick={logout}
          className="w-full flex items-center gap-2 px-3 py-2 rounded-xl text-sm text-[var(--color-text-muted)] hover:text-white hover:bg-white/5 transition-all"
        >
          <LogOut size={15} />
          Logout
        </button>
      </div>
    </aside>
  )
}
