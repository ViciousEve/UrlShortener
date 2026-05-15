import { motion, AnimatePresence } from 'framer-motion'
import { X, Download } from 'lucide-react'
import { QRCodeSVG } from 'qrcode.react'
import { Button } from './Button'

interface QRCodeModalProps {
  url: string
  onClose: () => void
}

export function QRCodeModal({ url, onClose }: QRCodeModalProps) {
  const handleDownload = () => {
    const svg = document.getElementById('qr-code-svg')
    if (!svg) return
    const svgData = new XMLSerializer().serializeToString(svg)
    const canvas = document.createElement('canvas')
    const ctx = canvas.getContext('2d')
    const img = new Image()
    img.onload = () => {
      canvas.width = img.width
      canvas.height = img.height
      ctx?.drawImage(img, 0, 0)
      const pngFile = canvas.toDataURL('image/png')
      const downloadLink = document.createElement('a')
      downloadLink.download = 'qrcode.png'
      downloadLink.href = `${pngFile}`
      downloadLink.click()
    }
    img.src = `data:image/svg+xml;base64,${btoa(svgData)}`
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      style={{ background: 'rgba(0,0,0,0.6)', backdropFilter: 'blur(4px)' }}
    >
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        exit={{ opacity: 0, scale: 0.95 }}
        className="glass-strong p-6 rounded-2xl w-full max-w-sm flex flex-col items-center"
      >
        <div className="flex items-center justify-between w-full mb-5">
          <h2 className="text-lg font-bold">QR Code</h2>
          <button
            onClick={onClose}
            className="text-[var(--color-text-muted)] hover:text-white transition-colors"
          >
            <X size={18} />
          </button>
        </div>

        <div className="bg-white p-4 rounded-xl mb-6 flex justify-center">
          <QRCodeSVG
            id="qr-code-svg"
            value={url}
            size={200}
            level="H"
            includeMargin={true}
          />
        </div>

        <div className="w-full text-center mb-6 px-4">
          <p className="text-sm font-medium text-[var(--color-indigo-light)] truncate">
            {url}
          </p>
        </div>

        <Button
          onClick={handleDownload}
          className="w-full"
          leftIcon={<Download size={16} />}
        >
          Download PNG
        </Button>
      </motion.div>
    </div>
  )
}
