import { clsx } from 'clsx'

interface SkeletonProps {
  className?: string
  width?: string | number
  height?: string | number
}

export function Skeleton({ className, width, height }: SkeletonProps) {
  return (
    <div
      className={clsx('skeleton', className)}
      style={{ width, height: height ?? '1rem' }}
    />
  )
}

export function SkeletonText({ lines = 3 }: { lines?: number }) {
  return (
    <div className="flex flex-col gap-2">
      {Array.from({ length: lines }).map((_, i) => (
        <Skeleton key={i} width={i === lines - 1 ? '60%' : '100%'} height="0.85rem" />
      ))}
    </div>
  )
}

export function SkeletonCard() {
  return (
    <div className="glass p-6 flex flex-col gap-4">
      <Skeleton height="1.5rem" width="40%" />
      <SkeletonText lines={2} />
    </div>
  )
}

export function SkeletonRow() {
  return (
    <div className="flex items-center gap-4 py-4 border-b border-white/5">
      <Skeleton width="30%" height="0.875rem" />
      <Skeleton width="20%" height="0.875rem" />
      <Skeleton width="60px" height="1.5rem" className="rounded-full" />
      <Skeleton width="80px" height="0.875rem" />
      <Skeleton width="60px" height="1.5rem" className="rounded-full" />
    </div>
  )
}
