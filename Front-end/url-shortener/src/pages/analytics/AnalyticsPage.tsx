import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { motion } from 'framer-motion'
import { ArrowLeft, MousePointerClick, Globe, Clock } from 'lucide-react'
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, PieChart, Pie, Cell, Legend
} from 'recharts'
import { Sidebar } from '../../components/layout/Sidebar'
import { Button } from '../../components/ui/Button'
import { Card } from '../../components/ui/Card'
import { SkeletonCard } from '../../components/ui/SkeletonLoaders'
import { useAnalytics } from '../../hooks/useAnalytics'
import { useUrls } from '../../hooks/useUrls'

const BROWSER_COLORS = ['#6366F1', '#14B8A6', '#F59E0B', '#EF4444']

const PERIOD_OPTIONS = [
  { label: '7D', days: 7 },
  { label: '30D', days: 30 },
  { label: '90D', days: 90 },
]

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
}

function formatDay(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

// Custom tooltip for area chart
function CustomTooltip({ active, payload, label }: { active?: boolean; payload?: { value: number }[]; label?: string }) {
  if (!active || !payload?.length) return null
  return (
    <div className="glass-strong px-3 py-2 rounded-xl text-xs shadow-2xl">
      <p className="text-[var(--color-text-muted)] mb-1">{formatDay(label ?? '')}</p>
      <p className="font-bold text-white">{payload[0].value} clicks</p>
    </div>
  )
}

export function AnalyticsPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [period, setPeriod] = useState(30)

  const { data: urls } = useUrls()
  const selectedCode = searchParams.get('code') ?? urls?.[0]?.code ?? 'xyz123'
  const selectedUrl = urls?.find(u => u.code === selectedCode)

  const { data: analytics, isLoading } = useAnalytics(selectedCode)

  const filteredClicks = analytics?.clicksOverTime.slice(-period) ?? []

  return (
    <div className="min-h-screen" style={{ background: 'var(--color-bg)' }}>
      <Sidebar />

      <main className="ml-64 p-8 page-content">
        {/* Header */}
        <div className="flex items-center gap-4 mb-8">
          <Button
            id="analytics-back-btn"
            variant="ghost"
            size="sm"
            leftIcon={<ArrowLeft size={15} />}
            onClick={() => navigate('/dashboard')}
          >
            Dashboard
          </Button>
          <div className="h-4 w-px bg-white/10" />
          <div>
            <h1 className="text-2xl font-bold text-white flex items-center gap-2">
              Analytics
              <span
                className="text-sm font-mono font-normal px-2 py-0.5 rounded-lg"
                style={{ background: 'rgba(99,102,241,0.15)', color: '#818CF8' }}
              >
                /{selectedCode}
              </span>
            </h1>
            <p className="text-sm text-[var(--color-text-muted)] mt-0.5">
              {selectedUrl?.shortUrl.replace(/^https?:\/\//, '') ?? `/s/${selectedCode}`}
            </p>
          </div>

          {/* URL selector */}
          {urls && urls.length > 1 && (
            <select
              id="analytics-url-select"
              value={selectedCode}
              onChange={(e) => navigate(`/analytics?code=${e.target.value}`)}
              className="ml-auto glass rounded-xl px-3 py-2 text-sm text-white cursor-pointer"
              style={{ background: 'rgba(255,255,255,0.05)', border: '1px solid rgba(255,255,255,0.08)' }}
            >
              {urls.map((u) => (
                <option key={u.code} value={u.code} style={{ background: '#111114' }}>
                  {u.shortUrl.replace(/^https?:\/\//, '')}
                </option>
              ))}
            </select>
          )}
        </div>

        {isLoading ? (
          <div className="grid gap-4">
            <SkeletonCard />
            <SkeletonCard />
          </div>
        ) : (
          <>
            {/* Summary Stat */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <Card className="flex items-center gap-4">
                <div className="w-11 h-11 rounded-xl flex items-center justify-center" style={{ background: 'rgba(99,102,241,0.15)' }}>
                  <MousePointerClick size={20} style={{ color: '#6366F1' }} />
                </div>
                <div>
                  <p className="text-xs text-[var(--color-text-muted)]">Total Clicks</p>
                  <p className="text-2xl font-bold text-white">{analytics?.totalClicks ?? 0}</p>
                </div>
              </Card>
              <Card className="flex items-center gap-4">
                <div className="w-11 h-11 rounded-xl flex items-center justify-center" style={{ background: 'rgba(20,184,166,0.15)' }}>
                  <Globe size={20} style={{ color: '#14B8A6' }} />
                </div>
                <div>
                  <p className="text-xs text-[var(--color-text-muted)]">Top Browser</p>
                  <p className="text-2xl font-bold text-white">{analytics?.browserBreakdown[0]?.browser ?? '—'}</p>
                </div>
              </Card>
              <Card className="flex items-center gap-4">
                <div className="w-11 h-11 rounded-xl flex items-center justify-center" style={{ background: 'rgba(245,158,11,0.15)' }}>
                  <Clock size={20} style={{ color: '#F59E0B' }} />
                </div>
                <div>
                  <p className="text-xs text-[var(--color-text-muted)]">Last Click</p>
                  <p className="text-lg font-bold text-white">
                    {analytics?.recentClicks[0]
                      ? formatTime(analytics.recentClicks[0].timestamp)
                      : '—'}
                  </p>
                </div>
              </Card>
            </div>

            {/* Area Chart */}
            <Card className="mb-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-sm font-semibold text-white">Clicks Over Time</h2>
                <div className="flex gap-1">
                  {PERIOD_OPTIONS.map((opt) => (
                    <button
                      key={opt.label}
                      id={`period-${opt.label}`}
                      onClick={() => setPeriod(opt.days)}
                      className="px-3 py-1 rounded-lg text-xs font-medium transition-all"
                      style={period === opt.days
                        ? { background: 'rgba(99,102,241,0.2)', color: '#818CF8', border: '1px solid rgba(99,102,241,0.3)' }
                        : { color: 'var(--color-text-muted)' }
                      }
                    >
                      {opt.label}
                    </button>
                  ))}
                </div>
              </div>
              <ResponsiveContainer width="100%" height={240}>
                <AreaChart data={filteredClicks} margin={{ top: 4, right: 4, left: -20, bottom: 0 }}>
                  <defs>
                    <linearGradient id="clicksGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#6366F1" stopOpacity={0.35} />
                      <stop offset="95%" stopColor="#6366F1" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" />
                  <XAxis
                    dataKey="date"
                    tickFormatter={(d) => new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
                    tick={{ fill: 'var(--color-text-muted)', fontSize: 11 }}
                    axisLine={false}
                    tickLine={false}
                    interval="preserveStartEnd"
                  />
                  <YAxis
                    tick={{ fill: 'var(--color-text-muted)', fontSize: 11 }}
                    axisLine={false}
                    tickLine={false}
                    allowDecimals={false}
                  />
                  <Tooltip content={<CustomTooltip />} />
                  <Area
                    type="monotone"
                    dataKey="clicks"
                    stroke="#6366F1"
                    strokeWidth={2}
                    fill="url(#clicksGradient)"
                    dot={false}
                    activeDot={{ r: 5, fill: '#6366F1', strokeWidth: 2, stroke: '#fff' }}
                  />
                </AreaChart>
              </ResponsiveContainer>
            </Card>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Browser Donut */}
              <Card>
                <h2 className="text-sm font-semibold text-white mb-5">Browser Breakdown</h2>
                <ResponsiveContainer width="100%" height={200}>
                  <PieChart>
                    <Pie
                      data={analytics?.browserBreakdown}
                      dataKey="count"
                      nameKey="browser"
                      cx="50%"
                      cy="50%"
                      innerRadius={55}
                      outerRadius={80}
                      paddingAngle={3}
                    >
                      {analytics?.browserBreakdown.map((_, i) => (
                        <Cell key={i} fill={BROWSER_COLORS[i % BROWSER_COLORS.length]} />
                      ))}
                    </Pie>
                    <Legend
                      formatter={(value) => (
                        <span style={{ color: 'var(--color-text-muted)', fontSize: 12 }}>{value}</span>
                      )}
                    />
                    <Tooltip
                      formatter={(value, name) => [`${value} clicks`, name]}
                      contentStyle={{
                        background: 'var(--color-surface-2)',
                        border: '1px solid rgba(255,255,255,0.08)',
                        borderRadius: '0.75rem',
                        color: 'white',
                        fontSize: 12,
                      }}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </Card>

              {/* Recent Activity */}
              <Card>
                <h2 className="text-sm font-semibold text-white mb-4">Recent Activity</h2>
                <div className="flex flex-col gap-2">
                  {analytics?.recentClicks.map((click, i) => (
                    <motion.div
                      key={click.id}
                      initial={{ opacity: 0, x: -8 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: i * 0.04 }}
                      className="flex items-center justify-between py-2.5 border-b border-white/5 last:border-0"
                    >
                      <div className="flex items-center gap-3">
                        <div
                          className="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold"
                          style={{ background: `${BROWSER_COLORS[i % 4]}22`, color: BROWSER_COLORS[i % 4] }}
                        >
                          {click.browser.charAt(0)}
                        </div>
                        <div>
                          <p className="text-xs font-medium text-white">{click.browser}</p>
                          <p className="text-xs text-[var(--color-text-muted)]">{click.ip}</p>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="text-xs text-[var(--color-text-muted)]">{click.country}</p>
                        <p className="text-xs text-[var(--color-text-muted)]">{formatTime(click.timestamp)}</p>
                      </div>
                    </motion.div>
                  ))}
                </div>
              </Card>
            </div>
          </>
        )}
      </main>
    </div>
  )
}
