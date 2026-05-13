import { createContext, useContext, useState, useCallback, type ReactNode } from 'react'
import { CheckCircle2, XCircle, Info, X } from 'lucide-react'
import { clsx } from 'clsx'

type ToastType = 'success' | 'error' | 'info'

interface Toast {
  id: string
  message: string
  type: ToastType
}

interface ToastContextType {
  toast: (message: string, type?: ToastType) => void
  success: (message: string) => void
  error: (message: string) => void
  info: (message: string) => void
}

const ToastContext = createContext<ToastContextType | null>(null)

export function useToast() {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error('useToast must be used within ToastProvider')
  return ctx
}

const icons: Record<ToastType, typeof CheckCircle2> = {
  success: CheckCircle2,
  error: XCircle,
  info: Info,
}

const toastStyles: Record<ToastType, React.CSSProperties> = {
  success: { borderColor: 'rgba(16,185,129,0.4)', background: 'rgba(16,185,129,0.08)' },
  error: { borderColor: 'rgba(239,68,68,0.4)', background: 'rgba(239,68,68,0.08)' },
  info: { borderColor: 'rgba(99,102,241,0.4)', background: 'rgba(99,102,241,0.08)' },
}

const iconColors: Record<ToastType, string> = {
  success: '#10B981',
  error: '#EF4444',
  info: '#818CF8',
}

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }, [])

  const addToast = useCallback((message: string, type: ToastType = 'info') => {
    const id = Math.random().toString(36).slice(2)
    setToasts((prev) => [...prev, { id, message, type }])
    setTimeout(() => removeToast(id), 3500)
  }, [removeToast])

  return (
    <ToastContext.Provider
      value={{
        toast: addToast,
        success: (m) => addToast(m, 'success'),
        error: (m) => addToast(m, 'error'),
        info: (m) => addToast(m, 'info'),
      }}
    >
      {children}
      {/* Toast Container */}
      <div
        className="fixed bottom-6 right-6 z-[100] flex flex-col gap-2"
        role="region"
        aria-live="polite"
        aria-label="Notifications"
      >
        {toasts.map((t) => {
          const Icon = icons[t.type]
          return (
            <div
              key={t.id}
              className={clsx('toast-enter flex items-center gap-3 px-4 py-3 rounded-xl glass-strong border text-sm font-medium text-white min-w-[280px] max-w-[360px] shadow-2xl')}
              style={toastStyles[t.type]}
              role="alert"
            >
              <Icon size={18} style={{ color: iconColors[t.type], flexShrink: 0 }} />
              <span className="flex-1">{t.message}</span>
              <button
                onClick={() => removeToast(t.id)}
                className="text-white/40 hover:text-white transition-colors"
                aria-label="Dismiss notification"
              >
                <X size={15} />
              </button>
            </div>
          )
        })}
      </div>
    </ToastContext.Provider>
  )
}
