import { type HTMLAttributes, type ReactNode } from 'react'
import { clsx } from 'clsx'

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
  hover?: boolean
  padding?: 'sm' | 'md' | 'lg' | 'none'
}

export function Card({ children, hover = false, padding = 'md', className, ...props }: CardProps) {
  const paddings = {
    none: '',
    sm: 'p-4',
    md: 'p-6',
    lg: 'p-8',
  }

  return (
    <div
      className={clsx(
        'glass',
        paddings[padding],
        hover && 'glass-hover cursor-pointer',
        className
      )}
      {...props}
    >
      {children}
    </div>
  )
}
