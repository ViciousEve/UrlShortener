import { clsx } from 'clsx'

type BadgeVariant = 'active' | 'expiring' | 'expired' | 'disabled' | 'indigo' | 'teal'

interface BadgeProps {
  variant?: BadgeVariant
  children: React.ReactNode
  className?: string
  dot?: boolean
}

const variantStyles: Record<BadgeVariant, string> = {
  active: 'bg-emerald/15 text-emerald border border-emerald/30',
  expiring: 'bg-amber/15 text-amber border border-amber/30',
  expired: 'bg-white/5 text-white/40 border border-white/10',
  disabled: 'bg-white/5 text-white/40 border border-white/10',
  indigo: 'bg-[var(--color-indigo)]/15 text-[var(--color-indigo-light)] border border-[var(--color-indigo)]/30',
  teal: 'bg-[var(--color-teal)]/15 text-[var(--color-teal)] border border-[var(--color-teal)]/30',
}


export function Badge({ variant = 'active', children, className, dot = false }: BadgeProps) {
  return (
    <span
      className={clsx(
        'inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold',
        variantStyles[variant],
        className
      )}
      style={
        variant === 'active'
          ? { backgroundColor: 'rgba(16,185,129,0.15)', color: '#10B981', border: '1px solid rgba(16,185,129,0.3)' }
          : variant === 'expiring'
          ? { backgroundColor: 'rgba(245,158,11,0.15)', color: '#F59E0B', border: '1px solid rgba(245,158,11,0.3)' }
          : variant === 'indigo'
          ? { backgroundColor: 'rgba(99,102,241,0.15)', color: '#818CF8', border: '1px solid rgba(99,102,241,0.3)' }
          : variant === 'teal'
          ? { backgroundColor: 'rgba(20,184,166,0.15)', color: '#14B8A6', border: '1px solid rgba(20,184,166,0.3)' }
          : undefined
      }
    >
      {dot && (
        <span
          className={clsx('w-1.5 h-1.5 rounded-full', variant === 'expiring' && 'animate-pulse')}
          style={
            variant === 'active' ? { backgroundColor: '#10B981' }
            : variant === 'expiring' ? { backgroundColor: '#F59E0B' }
            : variant === 'indigo' ? { backgroundColor: '#818CF8' }
            : variant === 'teal' ? { backgroundColor: '#14B8A6' }
            : { backgroundColor: 'rgba(255,255,255,0.3)' }
          }
        />
      )}
      {children}
    </span>
  )
}
