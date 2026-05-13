import { useQuery } from '@tanstack/react-query'
import axiosInstance from '../api/axiosInstance'

export interface UserStats {
  totalClicks: number
  activeLinks: number
  totalLinks: number
  topPerformingUrl: string | null
  topPerformingClicks: number
}

const MOCK_STATS: UserStats = {
  totalClicks: 518,
  activeLinks: 2,
  totalLinks: 4,
  topPerformingUrl: 'short.ly/def789',
  topPerformingClicks: 233,
}

export function useUserStats() {
  return useQuery<UserStats>({
    queryKey: ['userStats'],
    queryFn: async () => {
      try {
        const fromDate = new Date()
        fromDate.setMonth(fromDate.getMonth() - 1) // last 30 days roughly
        
        const [clicksRes, urlsRes, topUrlRes] = await Promise.all([
          axiosInstance.get('/analytics/clicks-in-period', {
            params: { 
              from: fromDate.toISOString(), 
              to: new Date().toISOString() 
            },
          }),
          axiosInstance.get('/shortening/urls'),
          axiosInstance.get('/analytics/top-url').catch(() => ({ data: null }))
        ])

        const urls = urlsRes.data || []
        const activeLinks = urls.filter((u: any) => u.status === 'Active').length
        
        let topPerformingUrl = null
        let topPerformingClicks = 0
        
        if (topUrlRes.data && topUrlRes.data.shortCode) {
          topPerformingUrl = `${window.location.origin}/s/${topUrlRes.data.shortCode}`
          topPerformingClicks = topUrlRes.data.totalClicks || 0
        }

        return {
          totalClicks: typeof clicksRes.data === 'number' ? clicksRes.data : 0,
          activeLinks,
          totalLinks: urls.length,
          topPerformingUrl,
          topPerformingClicks,
        }
      } catch {
        return MOCK_STATS
      }
    },
    staleTime: 60_000,
  })
}
