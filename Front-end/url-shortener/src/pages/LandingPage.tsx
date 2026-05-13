import { useState } from 'react'
import { Link } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'
import { Zap, Copy, Check, ExternalLink, Clock, QrCode, ArrowRight, Shield, BarChart3, Link2 } from 'lucide-react'
import { Button } from '../components/ui/Button'
import { Input } from '../components/ui/Input'
import { Card } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { useCreateUrl } from '../hooks/useCreateUrl'
import { useToast } from '../components/layout/ToastProvider'
import { useAuth } from '../context/AuthContext'

export function LandingPage() {
  const [url, setUrl] = useState('')
  const [copied, setCopied] = useState(false)
  const { mutate: createUrl, data: result, isPending, reset } = useCreateUrl()
  const { success, error } = useToast()
  const { isAuthenticated } = useAuth()

  const handleShorten = (e: React.FormEvent) => {
    e.preventDefault()
    if (!url.trim()) return
    reset()
    createUrl(
      { originalUrl: url, ttlMinutes: isAuthenticated ? 60 : 15 },
      {
        onSuccess: () => success('Link shortened successfully!'),
        onError: () => error('Failed to shorten URL. Please try again.'),
      }
    )
  }

  const handleCopy = () => {
    if (!result?.shortUrl) return
    navigator.clipboard.writeText(`https://${result.shortUrl}`)
    setCopied(true)
    success('Copied to clipboard!')
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <div className="min-h-screen flex flex-col">
      {/* Gradient blobs */}
      <div className="blob-indigo" />
      <div className="blob-teal" />

      {/* Hero */}
      <main className="flex-1 flex flex-col items-center justify-center px-4 pt-28 pb-20 page-content">
        <motion.div
          initial={{ opacity: 0, y: 24 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, ease: 'easeOut' }}
          className="text-center max-w-3xl w-full"
        >
          {/* Badge */}
          <div className="flex justify-center mb-6">
            <Badge variant="indigo" dot>
              ✨ Premium URL Shortener
            </Badge>
          </div>

          <h1 className="text-5xl md:text-6xl font-extrabold leading-tight tracking-tight mb-4">
            Shorten. Share.{' '}
            <span className="gradient-text">Analyze.</span>
          </h1>
          <p className="text-lg text-[var(--color-text-muted)] max-w-xl mx-auto mb-10">
            Create powerful short links with real-time analytics, QR codes, and smart expiry control.
            Built for speed, designed for pros.
          </p>

          {/* Shorten Form */}
          <form onSubmit={handleShorten} className="relative max-w-2xl mx-auto">
            <div className="glass-strong p-2 rounded-2xl flex gap-2">
              <Input
                id="url-input"
                type="url"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
                placeholder="Paste your long URL here..."
                leftIcon={<Link2 size={16} />}
                containerClassName="flex-1"
                className="bg-transparent border-none focus:shadow-none text-base"
                required
              />
              <Button
                id="shorten-btn"
                type="submit"
                size="lg"
                isLoading={isPending}
                rightIcon={<ArrowRight size={16} />}
                className="flex-shrink-0 rounded-xl"
              >
                Shorten
              </Button>
            </div>
          </form>

          {/* Result Card */}
          <AnimatePresence>
            {result && (
              <motion.div
                initial={{ opacity: 0, y: 16, scale: 0.97 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, y: -8, scale: 0.97 }}
                transition={{ duration: 0.3 }}
                className="mt-4 max-w-2xl mx-auto"
              >
                <Card className="text-left">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <span className="text-lg font-bold text-[var(--color-indigo-light)]">
                          https://{result.shortUrl}
                        </span>
                        {!isAuthenticated && (
                          <Badge variant="expiring" dot>15 min lifespan</Badge>
                        )}
                      </div>
                      <p className="text-xs text-[var(--color-text-muted)] truncate">
                        {result.originalUrl}
                      </p>
                    </div>
                    <div className="flex items-center gap-2 flex-shrink-0">
                      <button
                        id="copy-link-btn"
                        onClick={handleCopy}
                        className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-all duration-200 glass glass-hover"
                        style={copied ? { color: '#10B981' } : { color: 'var(--color-text-muted)' }}
                        title="Copy to clipboard"
                      >
                        {copied ? <Check size={14} /> : <Copy size={14} />}
                        {copied ? 'Copied!' : 'Copy'}
                      </button>
                      <a
                        href={`https://${result.shortUrl}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="p-1.5 rounded-lg glass glass-hover text-[var(--color-text-muted)] hover:text-white transition-colors"
                        title="Open link"
                      >
                        <ExternalLink size={14} />
                      </a>
                    </div>
                  </div>

                  {/* QR Placeholder */}
                  <div className="mt-4 pt-4 border-t border-white/5 flex items-center justify-between">
                    <div className="flex items-center gap-2 text-xs text-[var(--color-text-muted)]">
                      <QrCode size={14} />
                      <span>QR Code available</span>
                    </div>
                    {!isAuthenticated && (
                      <Link to="/register" className="text-xs text-[var(--color-indigo-light)] hover:underline flex items-center gap-1">
                        Sign up to keep this link forever
                        <ArrowRight size={12} />
                      </Link>
                    )}
                  </div>
                </Card>
              </motion.div>
            )}
          </AnimatePresence>

          {/* TTL info for guests */}
          {!isAuthenticated && (
            <p className="mt-4 text-xs text-[var(--color-text-muted)] flex items-center justify-center gap-1">
              <Clock size={12} />
              Guest links expire in 15 minutes. Register for 30/60/180 min options.
            </p>
          )}
        </motion.div>

        {/* Feature pills */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4, duration: 0.5 }}
          className="flex flex-wrap justify-center gap-3 mt-16 page-content"
        >
          {[
            { icon: BarChart3, label: 'Real-time Analytics' },
            { icon: Shield, label: 'Secure & Private' },
            { icon: QrCode, label: 'QR Code Generator' },
            { icon: Zap, label: 'Instant Redirect' },
          ].map(({ icon: Icon, label }) => (
            <div
              key={label}
              className="flex items-center gap-2 px-4 py-2 glass rounded-full text-sm text-[var(--color-text-muted)]"
            >
              <Icon size={14} style={{ color: '#6366F1' }} />
              {label}
            </div>
          ))}
        </motion.div>
      </main>

      {/* Footer */}
      <footer className="page-content text-center py-6 text-xs text-[var(--color-text-muted)] border-t border-white/5">
        © 2025 Shortly. Built with ❤️ for speed.
      </footer>
    </div>
  )
}
