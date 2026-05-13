import { type ButtonHTMLAttributes, type ReactNode } from 'react'
import { clsx } from 'clsx'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
  size?: 'sm' | 'md' | 'lg'
  isLoading?: boolean
  leftIcon?: ReactNode
  rightIcon?: ReactNode
}

export function Button({
  variant = 'primary',
  size = 'md',
  isLoading = false,
  leftIcon,
  rightIcon,
  children,
  className,
  disabled,
  ...props
}: ButtonProps) {
  const base =
    'inline-flex items-center justify-center gap-2 font-semibold rounded-xl transition-all duration-200 cursor-pointer select-none focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-indigo-DEFAULT disabled:opacity-50 disabled:pointer-events-none'

  const variants = {
    primary:
      'bg-indigo-DEFAULT text-white hover:bg-indigo-dark btn-glow shadow-lg shadow-indigo-DEFAULT/30',
    secondary:
      'glass border border-white/10 text-white hover:border-indigo-DEFAULT/50 hover:shadow-[0_0_20px_rgba(99,102,241,0.25)]',
    ghost: 'text-[var(--color-text-muted)] hover:text-white hover:bg-white/5',
    danger: 'bg-ruby text-white hover:brightness-110 shadow-lg shadow-ruby/20',
  }

  const sizes = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-5 py-2.5 text-sm',
    lg: 'px-7 py-3.5 text-base',
  }

  return (
    <button
      disabled={disabled || isLoading}
      className={clsx(base, variants[variant], sizes[size], className)}
      {...props}
    >
      {isLoading ? (
        <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
      ) : (
        leftIcon
      )}
      {children}
      {!isLoading && rightIcon}
    </button>
  )
}
