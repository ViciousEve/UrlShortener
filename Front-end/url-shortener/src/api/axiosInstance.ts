import axios from 'axios'

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Attach JWT on every request
axiosInstance.interceptors.request.use((config) => {
  const token = localStorage.getItem('shortly_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Handle 401 → clear token
axiosInstance.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('shortly_token')
      localStorage.removeItem('shortly_user')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

export default axiosInstance
