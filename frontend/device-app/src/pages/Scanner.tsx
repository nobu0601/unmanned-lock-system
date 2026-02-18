import { useEffect, useRef, useState, useCallback } from 'react';
import { Html5Qrcode } from 'html5-qrcode';
import axios from 'axios';

const DEVICE_API_KEY = 'dev-device-key-12345';
const RESET_DELAY = 3000;

interface ScanResult {
  granted: boolean;
  message: string;
  denialReason?: string;
}

export default function Scanner() {
  const [result, setResult] = useState<ScanResult | null>(null);
  const [manualToken, setManualToken] = useState('');
  const scannerRef = useRef<Html5Qrcode | null>(null);
  const processingRef = useRef(false);

  const processScan = useCallback(async (token: string) => {
    if (processingRef.current) return;
    processingRef.current = true;

    try {
      const res = await axios.post<ScanResult>('/api/device/scan',
        { token },
        { headers: { 'X-Device-Key': DEVICE_API_KEY } }
      );
      setResult(res.data);
    } catch (err: any) {
      setResult({
        granted: false,
        message: err.response?.data?.message || 'Scan failed',
        denialReason: 'network_error'
      });
    }

    setTimeout(() => {
      setResult(null);
      processingRef.current = false;
    }, RESET_DELAY);
  }, []);

  useEffect(() => {
    const scanner = new Html5Qrcode('qr-reader');
    scannerRef.current = scanner;

    scanner.start(
      { facingMode: 'environment' },
      { fps: 10, qrbox: { width: 300, height: 300 } },
      (decodedText) => {
        processScan(decodedText);
      },
      () => {} // ignore errors (no QR found)
    ).catch(err => {
      console.error('Camera error:', err);
    });

    return () => {
      scanner.stop().catch(() => {});
    };
  }, [processScan]);

  const handleManualScan = () => {
    if (manualToken.trim()) {
      processScan(manualToken.trim());
      setManualToken('');
    }
  };

  return (
    <div className="scanner-page">
      <div className="scanner-header">
        <h1>QR SCANNER</h1>
        <p>Position QR code in the camera view</p>
      </div>

      <div className="scanner-area">
        <div id="qr-reader" />
      </div>

      <div className="manual-input">
        <input
          type="text"
          placeholder="Or paste token manually..."
          value={manualToken}
          onChange={(e) => setManualToken(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleManualScan()}
        />
        <button onClick={handleManualScan}>Scan</button>
      </div>

      {result && (
        <div className={`result-overlay ${result.granted ? 'granted' : 'denied'}`}>
          <div className="result-icon">
            {result.granted ? '\u2713' : '\u2717'}
          </div>
          <div className="result-text">
            {result.granted ? 'ACCESS GRANTED' : 'ACCESS DENIED'}
          </div>
          {result.denialReason && (
            <div className="result-detail">{result.message}</div>
          )}
        </div>
      )}
    </div>
  );
}
