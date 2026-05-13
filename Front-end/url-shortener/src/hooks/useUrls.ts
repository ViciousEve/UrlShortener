import { useQuery } from '@tanstack/react-query'
import axiosInstance from '../api/axiosInstance'

export interface ShortenedUrl {
  code: string
  originalUrl: string
  shortUrl: string
  createdAt: string
  expiresAt: string | null
  status: 'Active' | 'Expired' | 'Disabled'
}

// Mock data for development (used when backend is offline)
const MOCK_URLS: ShortenedUrl[] = [
  {
    code: 'xyz123',
    originalUrl: 'https://www.example.com/very/long/url/that/needs/shortening',
    shortUrl: 'https://localhost:7127/s/xyz123',
    createdAt: new Date(Date.now() - 86400000 * 2).toISOString(),
    expiresAt: null,
    status: 'Active',
  },
  {
    code: 'abc456',
    originalUrl: 'https://github.com/some-repo/with-a-long-path',
    shortUrl: 'https://localhost:7127/s/abc456',
    createdAt: new Date(Date.now() - 86400000 * 5).toISOString(),
    expiresAt: new Date(Date.now() + 3600000).toISOString(),
    status: 'Active',
  },
  {
    code: 'def789',
    originalUrl: 'https://docs.microsoft.com/some/really/long/documentation/page',
    shortUrl: 'https://localhost:7127/s/def789',
    createdAt: new Date(Date.now() - 86400000 * 10).toISOString(),
    expiresAt: new Date(Date.now() - 3600000).toISOString(),
    status: 'Expired',
  },
  {
    code: 'ghi012',
    originalUrl: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
    shortUrl: 'https://localhost:7127/s/ghi012',
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    expiresAt: null,
    status: 'Active',
  },
]

export function useUrls() {
  return useQuery<ShortenedUrl[]>({
    queryKey: ['urls'],
    queryFn: async () => {
      try {
        const res = await axiosInstance.get('/shortening/urls')
        return res.data.map((item: any) => ({
          code: item.shortCode,
          originalUrl: item.originalUrl,
          shortUrl: item.shortUrl || `${window.location.origin}/s/${item.shortCode}`,
          createdAt: item.createdAt,
          expiresAt: item.expiresAt.startsWith('0001-01-01') ? null : item.expiresAt,
          status: item.status,
        }))
      } catch {
        return MOCK_URLS
      }
    },
    staleTime: 30_000,
  })
}
