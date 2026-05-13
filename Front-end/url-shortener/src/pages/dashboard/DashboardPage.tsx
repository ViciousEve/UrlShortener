import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'
import {
  MousePointerClick, Link2, TrendingUp, Plus, Copy, Check,
  Trash2, BarChart3, QrCode, X, ExternalLink
} from 'lucide-react'
import { Sidebar } from '../../components/layout/Sidebar'
import { Button } from '../../components/ui/Button'
import { Input } from '../../components/ui/Input'
import { Card } from '../../components/ui/Card'
import { Badge } from '../../components/ui/Badge'
import { SkeletonRow } from '../../components/ui/SkeletonLoaders'
import { useUrls, type ShortenedUrl } from '../../hooks/useUrls'
import { useCreateUrl } from '../../hooks/useCreateUrl'
import { useUserStats } from '../../hooks/useUserStats'
import { useToast } from '../../components/layout/ToastProvider'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import axiosInstance from '../../api/axiosInstance'

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

function truncate(s: string, n = 45) {
  return s.length > n ? s.slice(0, n) + '…' : s
}

function statusToVariant(status: ShortenedUrl['status']) {
  if (status === 'Active') return 'active' as const
  if (status === 'Disabled') return 'expired' as const
  return 'expired' as const
}

function statusLabel(status: ShortenedUrl['status']) {
  return status
}

// ── Stat Card ────────────────────────────────────
function StatCard({ label, value, icon: Icon, color }: {
  label: string; value: string | number; icon: typeof MousePointerClick; color: string
}) {
  return (
    <Card className="flex items-center gap-4">
      <div
        className="w-11 h-11 rounded-xl flex items-center justify-center flex-shrink-0"
        style={{ background: `${color}18` }}
      >
        <Icon size={20} style={{ color }} />
      </div>
      <div>
        <p className="text-xs text-[var(--color-text-muted)] mb-0.5">{label}</p>
        <p className="text-xl font-bold text-white">{value}</p>
      </div>
    </Card>
  )
}

// ── New Link Modal ────────────────────────────────
function NewLinkModal({ onClose }: { onClose: () => void }) {
  const [urlVal, setUrlVal] = useState('')
  const [ttl, setTtl] = useState(60)
  const { mutate, isPending, data: result } = useCreateUrl()
  const { success, error } = useToast()
  const [copied, setCopied] = useState(false)

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault()
    if (!urlVal.trim()) return
    mutate(
      { originalUrl: urlVal, ttlMinutes: ttl },
      {
        onSuccess: () => success('Short link created!'),
        onError: () => error('Failed to create link'),
      }
    )
  }

  const handleCopy = () => {
    if (!result?.shortUrl) return
    navigator.clipboard.writeText(result.shortUrl)
    setCopied(true)
    success('Copied!')
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4" style={{ background: 'rgba(0,0,0,0.6)', backdropFilter: 'blur(4px)' }}>
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        exit={{ opacity: 0, scale: 0.95 }}
        className="glass-strong p-6 rounded-2xl w-full max-w-md"
      >
        <div className="flex items-center justify-between mb-5">
          <h2 className="text-lg font-bold">New Short Link</h2>
          <button onClick={onClose} className="text-[var(--color-text-muted)] hover:text-white transition-colors">
            <X size={18} />
          </button>
        </div>

        {!result ? (
          <form onSubmit={handleCreate} className="flex flex-col gap-4">
            <Input
              id="modal-url-input"
              label="Destination URL"
              type="url"
              value={urlVal}
              onChange={(e) => setUrlVal(e.target.value)}
              placeholder="https://example.com/long-url"
              required
            />
            <div>
              <label className="text-sm font-medium text-[var(--color-text-muted)] block mb-2">
                TTL (minutes)
              </label>
              <div className="flex gap-2">
                {[30, 60, 180].map((t) => (
                  <button
                    key={t}
                    type="button"
                    onClick={() => setTtl(t)}
                    className="flex-1 py-2 rounded-xl text-sm font-medium transition-all"
                    style={ttl === t
                      ? { background: 'rgba(99,102,241,0.2)', color: '#818CF8', border: '1px solid rgba(99,102,241,0.4)' }
                      : { background: 'rgba(255,255,255,0.05)', color: 'var(--color-text-muted)', border: '1px solid rgba(255,255,255,0.08)' }
                    }
                  >
                    {t}m
                  </button>
                ))}
              </div>
            </div>
            <Button id="modal-create-btn" type="submit" isLoading={isPending} className="w-full" size="lg">
              Create Link
            </Button>
          </form>
        ) : (
          <div className="flex flex-col gap-4">
            <p className="text-sm text-[var(--color-text-muted)]">Your short link is ready!</p>
            <div className="glass p-4 rounded-xl flex items-center justify-between gap-3">
              <span className="text-[var(--color-indigo-light)] font-semibold truncate">
                {result.shortUrl}
              </span>
              <button
                onClick={handleCopy}
                className="flex items-center gap-1.5 px-3 py-1.5 glass rounded-lg text-xs font-medium transition-colors flex-shrink-0"
                style={copied ? { color: '#10B981' } : { color: 'var(--color-text-muted)' }}
              >
                {copied ? <Check size={14} /> : <Copy size={14} />}
                {copied ? 'Copied' : 'Copy'}
              </button>
            </div>
            <Button variant="secondary" onClick={onClose} className="w-full">Done</Button>
          </div>
        )}
      </motion.div>
    </div>
  )
}

// ── Main Dashboard ─────────────────────────────────
export function DashboardPage() {
  const [showModal, setShowModal] = useState(false)
  const [copiedCode, setCopiedCode] = useState<string | null>(null)
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const { success } = useToast()

  const { data: urls, isLoading: urlsLoading } = useUrls()
  const { data: stats } = useUserStats()

  const queryClient = useQueryClient()
  const { mutate: deleteUrl } = useMutation({
    mutationFn: (code: string) => axiosInstance.delete(`/shortening/urls/${code}`).catch(() => {}),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['urls'] }); success('Link deleted') },
  })

  // Open modal if navigated with ?new=1
  const isNew = searchParams.get('new') === '1'

  const handleCopy = (url: ShortenedUrl) => {
    navigator.clipboard.writeText(url.shortUrl)
    setCopiedCode(url.code)
    success('Copied to clipboard!')
    setTimeout(() => setCopiedCode(null), 2000)
  }

  return (
    <div className="min-h-screen" style={{ background: 'var(--color-bg)' }}>
      <Sidebar />

      <main className="ml-64 p-8 page-content">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-2xl font-bold text-white">Dashboard</h1>
            <p className="text-sm text-[var(--color-text-muted)] mt-0.5">Manage and track all your links</p>
          </div>
          <Button
            id="dashboard-new-link-btn"
            leftIcon={<Plus size={16} />}
            onClick={() => setShowModal(true)}
          >
            New Link
          </Button>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          <StatCard label="Total Clicks" value={stats?.totalClicks ?? '—'} icon={MousePointerClick} color="#6366F1" />
          <StatCard label="Active Links" value={stats?.activeLinks ?? '—'} icon={Link2} color="#10B981" />
          <StatCard label="Top Performer" value={stats?.topPerformingClicks ? `${stats.topPerformingClicks} clicks` : '—'} icon={TrendingUp} color="#14B8A6" />
        </div>

        {/* Links Table */}
        <Card padding="none">
          <div className="flex items-center justify-between px-6 py-4 border-b border-white/5">
            <h2 className="text-sm font-semibold text-white">All Links</h2>
            <span className="text-xs text-[var(--color-text-muted)]">{urls?.length ?? 0} total</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-white/5">
                  {['Original URL', 'Short Link', 'Created', 'Status', 'Actions'].map((h) => (
                    <th key={h} className="px-6 py-3 text-left text-xs font-semibold text-[var(--color-text-muted)] uppercase tracking-wider">
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {urlsLoading ? (
                  Array.from({ length: 4 }).map((_, i) => (
                    <tr key={i}><td colSpan={5} className="px-6"><SkeletonRow /></td></tr>
                  ))
                ) : urls?.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="px-6 py-16 text-center text-[var(--color-text-muted)]">
                      No links yet. Create your first short link!
                    </td>
                  </tr>
                ) : (
                  urls?.map((url, i) => (
                    <motion.tr
                      key={url.code}
                      initial={{ opacity: 0, y: 8 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: i * 0.05 }}
                      className="border-b border-white/4 hover:bg-white/2 transition-colors"
                    >
                      <td className="px-6 py-4 max-w-[220px]">
                        <span className="text-[var(--color-text-muted)] text-xs" title={url.originalUrl}>
                          {truncate(url.originalUrl, 40)}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        <a
                          href={url.shortUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="text-[var(--color-indigo-light)] hover:underline flex items-center gap-1 text-xs"
                        >
                          {url.shortUrl.replace(/^https?:\/\//, '')}
                          <ExternalLink size={11} />
                        </a>
                      </td>

                      <td className="px-6 py-4 text-xs text-[var(--color-text-muted)]">
                        {formatDate(url.createdAt)}
                      </td>
                      <td className="px-6 py-4">
                        <Badge variant={statusToVariant(url.status)} dot>
                          {statusLabel(url.status)}
                        </Badge>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-1">
                          <button
                            id={`copy-${url.code}`}
                            onClick={() => handleCopy(url)}
                            className="p-1.5 rounded-lg hover:bg-white/8 transition-colors"
                            title="Copy link"
                            style={copiedCode === url.code ? { color: '#10B981' } : { color: 'var(--color-text-muted)' }}
                          >
                            {copiedCode === url.code ? <Check size={14} /> : <Copy size={14} />}
                          </button>
                          <button
                            id={`stats-${url.code}`}
                            onClick={() => navigate(`/analytics?code=${url.code}`)}
                            className="p-1.5 rounded-lg hover:bg-white/8 text-[var(--color-text-muted)] hover:text-white transition-colors"
                            title="View analytics"
                          >
                            <BarChart3 size={14} />
                          </button>
                          <button
                            id={`qr-${url.code}`}
                            className="p-1.5 rounded-lg hover:bg-white/8 text-[var(--color-text-muted)] hover:text-white transition-colors"
                            title="Download QR (coming soon)"
                          >
                            <QrCode size={14} />
                          </button>
                          <button
                            id={`delete-${url.code}`}
                            onClick={() => deleteUrl(url.code)}
                            className="p-1.5 rounded-lg hover:bg-red-500/10 text-[var(--color-text-muted)] hover:text-[var(--color-ruby)] transition-colors"
                            title="Delete link"
                          >
                            <Trash2 size={14} />
                          </button>
                        </div>
                      </td>
                    </motion.tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </Card>
      </main>

      {/* New Link Modal */}
      <AnimatePresence>
        {(showModal || isNew) && (
          <NewLinkModal onClose={() => { setShowModal(false); navigate('/dashboard') }} />
        )}
      </AnimatePresence>
    </div>
  )
}
