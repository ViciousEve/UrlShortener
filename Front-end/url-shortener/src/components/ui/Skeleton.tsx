import { clsx } from 'clsx'

type BadgeVariant = 'active' | 'expiring' | 'expired' | 'disabled' | 'indigo' | 'teal'

interface BadgeProps {
  variant?: BadgeVariant
  children: React.ReactNode
  className?: string
  dot?: boolean
}

const variantStyles: Record<BadgeVariant, string> = {
  active: 'bg-emerald-DEFAULT/15 text-emerald-DEFAULT border border-emerald-DEFAULT/30',
  expiring: 'bg-amber-DEFAULT/15 text-amber-DEFAULT border border-amber-DEFAULT/30',
  expired: 'bg-white/5 text-white/40 border border-white/10',
  disabled: 'bg-white/5 text-white/40 border border-white/10',
  indigo: 'bg-indigo-DEFAULT/15 text-indigo-light border border-indigo-DEFAULT/30',
  teal: 'bg-teal-DEFAULT/15 text-teal-DEFAULT border border-teal-DEFAULT/30',
}

const dotStyles: Record<BadgeVariant, string> = {
  active: 'bg-emerald-DEFAULT',
  expiring: 'bg-amber-DEFAULT animate-pulse',
  expired: 'bg-white/30',
  disabled: 'bg-white/30',
  indigo: 'bg-indigo-light',
  teal: 'bg-teal-DEFAULT',
}

export function Badge({ variant = 'active', children, className, dot = false }: BadgeProps) {
  return (
    <span
      className={clsx(
        'inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold',
        variantStyles[variant],
        className
      )}
    >
      {dot && <span className={clsx('w-1.5 h-1.5 rounded-full', dotStyles[variant])} />}
      {children}
    </span>
  )
}
