/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        obsidian: {
          DEFAULT: '#0A0A0B',
          50: '#1A1A1F',
          100: '#141418',
          200: '#0F0F12',
          300: '#0A0A0B',
        },
        indigo: {
          DEFAULT: '#6366F1',
          light: '#818CF8',
          dark: '#4F46E5',
        },
        teal: {
          DEFAULT: '#14B8A6',
        },
        emerald: {
          DEFAULT: '#10B981',
        },
        amber: {
          DEFAULT: '#F59E0B',
        },
        ruby: {
          DEFAULT: '#EF4444',
        },
      },
      fontFamily: {
        sans: ['Inter', 'Outfit', 'system-ui', 'sans-serif'],
      },
      backdropBlur: {
        xs: '2px',
      },
      animation: {
        'float': 'float 6s ease-in-out infinite',
        'pulse-glow': 'pulseGlow 2s ease-in-out infinite',
        'slide-up': 'slideUp 0.3s ease-out',
        'fade-in': 'fadeIn 0.4s ease-out',
      },
      keyframes: {
        float: {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-10px)' },
        },
        pulseGlow: {
          '0%, 100%': { boxShadow: '0 0 15px rgba(99,102,241,0.4)' },
          '50%': { boxShadow: '0 0 30px rgba(99,102,241,0.8)' },
        },
        slideUp: {
          from: { opacity: '0', transform: 'translateY(12px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
        fadeIn: {
          from: { opacity: '0' },
          to: { opacity: '1' },
        },
      },
    },
  },
  plugins: [],
}
