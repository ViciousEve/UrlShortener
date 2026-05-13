import { useMutation, useQueryClient } from '@tanstack/react-query'
import axiosInstance from '../api/axiosInstance'
import { useAuth } from '../context/AuthContext'

interface CreateUrlPayload {
  originalUrl: string
  ttlMinutes?: number
}

interface CreateUrlResponse {
  code: string
  shortUrl: string
  originalUrl: string
  expiresAt: string | null
}

export function useCreateUrl() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  return useMutation<CreateUrlResponse, Error, CreateUrlPayload>({
    mutationFn: async (payload) => {
      try {
        const res = await axiosInstance.post('/shortening/shorten', {
          ...payload,
          userId: user?.id
        })
        return {
          code: res.data.shortCode,
          shortUrl: res.data.shortUrl || `${window.location.origin}/s/${res.data.shortCode}`,
          originalUrl: res.data.originalUrl,
          expiresAt: res.data.expiresAt.startsWith('0001-01-01') ? null : res.data.expiresAt,
        }
      } catch {
        // Mock response for dev
        const code = Math.random().toString(36).slice(2, 8)
        return {
          code,
          shortUrl: `short.ly/${code}`,
          originalUrl: payload.originalUrl,
          expiresAt: payload.ttlMinutes
            ? new Date(Date.now() + payload.ttlMinutes * 60_000).toISOString()
            : null,
        }
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['urls'] })
    },
  })
}
