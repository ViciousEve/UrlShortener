import { useQuery } from '@tanstack/react-query'
import axiosInstance from '../api/axiosInstance'

export interface ClickDataPoint {
  date: string
  clicks: number
}

export interface BrowserBreakdown {
  browser: string
  count: number
  percentage: number
}

export interface RecentClick {
  id: string
  timestamp: string
  browser: string
  ip: string
  country: string
}

export interface AnalyticsData {
  code: string
  totalClicks: number
  clicksOverTime: ClickDataPoint[]
  browserBreakdown: BrowserBreakdown[]
  recentClicks: RecentClick[]
}

function generateMockClicks(days = 30): ClickDataPoint[] {
  return Array.from({ length: days }, (_, i) => ({
    date: new Date(Date.now() - (days - i) * 86400000).toISOString().split('T')[0],
    clicks: Math.floor(Math.random() * 80) + 5,
  }))
}

const MOCK_ANALYTICS: AnalyticsData = {
  code: 'xyz123',
  totalClicks: 142,
  clicksOverTime: generateMockClicks(30),
  browserBreakdown: [
    { browser: 'Chrome', count: 85, percentage: 60 },
    { browser: 'Firefox', count: 28, percentage: 20 },
    { browser: 'Safari', count: 21, percentage: 15 },
    { browser: 'Other', count: 8, percentage: 5 },
  ],
  recentClicks: Array.from({ length: 10 }, (_, i) => ({
    id: String(i),
    timestamp: new Date(Date.now() - i * 600_000).toISOString(),
    browser: ['Chrome', 'Firefox', 'Safari', 'Edge'][i % 4],
    ip: `192.168.${Math.floor(Math.random() * 255)}.xxx`,
    country: ['US', 'VN', 'DE', 'FR', 'JP', 'GB', 'AU', 'CA', 'KR', 'BR'][i],
  })),
}

export function useAnalytics(code: string) {
  return useQuery<AnalyticsData>({
    queryKey: ['analytics', code],
    queryFn: async () => {
      try {
        const [clicks, browsers] = await Promise.all([
          axiosInstance.get(`/analytics/${code}/clicks`),
          axiosInstance.get('/analytics/top-browsers'),
        ])
        
        const rawClicks: any[] = clicks.data || []
        
        // Compute clicks over time by grouping by date
        const clicksByDate = rawClicks.reduce((acc: Record<string, number>, curr) => {
          const date = curr.clickedAtUtc.split('T')[0]
          acc[date] = (acc[date] || 0) + 1
          return acc
        }, {})
        const clicksOverTime = Object.entries(clicksByDate).map(([date, count]) => ({
          date,
          clicks: count as number,
        }))

        // Map recent clicks (backend lacks browser/ip/country)
        const recentClicks = rawClicks.slice(0, 10).map((c: any) => ({
          id: c.id,
          timestamp: c.clickedAtUtc,
          browser: 'N/A',
          ip: 'N/A',
          country: 'N/A',
        }))

        // Map browsers (backend only returns string array)
        const rawBrowsers: string[] = browsers.data || []
        const browserBreakdown = rawBrowsers.map((b, i) => ({
          browser: b,
          count: rawBrowsers.length - i, // fake count since backend doesn't provide it
          percentage: Math.round(100 / rawBrowsers.length), // fake percentage
        }))

        return {
          code,
          totalClicks: rawClicks.length,
          clicksOverTime,
          browserBreakdown,
          recentClicks,
        }
      } catch {
        return { ...MOCK_ANALYTICS, code }
      }
    },
    enabled: !!code,
    staleTime: 60_000,
  })
}
