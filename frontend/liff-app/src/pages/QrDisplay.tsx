import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import { generateQr, QrTokenResponse } from '../api/client';

export default function QrDisplay() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [qr, setQr] = useState<QrTokenResponse | null>(null);
  const [remaining, setRemaining] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchQr = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      const res = await generateQr(id);
      setQr(res.data);
      setRemaining(res.data.ttlSeconds);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to generate QR code');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchQr();
  }, [fetchQr]);

  useEffect(() => {
    if (remaining <= 0) return;
    const timer = setInterval(() => {
      setRemaining(prev => {
        if (prev <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [remaining]);

  const expired = remaining <= 0 && qr !== null;

  return (
    <div className="qr-container">
      <h2>QR Code</h2>

      {loading && <div className="loading">Generating QR...</div>}

      {error && (
        <div className="message error">{error}</div>
      )}

      {qr && !loading && (
        <>
          <div className="qr-code" style={{ opacity: expired ? 0.3 : 1 }}>
            <QRCodeSVG value={qr.token} size={256} level="M" />
          </div>

          <div className={`countdown ${remaining <= 15 ? 'warning' : 'ok'}`}>
            {expired ? 'EXPIRED' : `${remaining}s`}
          </div>

          {expired && (
            <button className="btn btn-primary btn-full" onClick={fetchQr}>
              Regenerate QR Code
            </button>
          )}

          <p style={{ color: '#666', fontSize: 13, marginTop: 16 }}>
            Show this QR code to the scanner at the entrance.
            {!expired && ` Expires in ${remaining} seconds.`}
          </p>
        </>
      )}

      <button
        className="btn btn-full"
        style={{ marginTop: 16, background: '#eee', color: '#333' }}
        onClick={() => navigate('/passes')}
      >
        Back to Passes
      </button>
    </div>
  );
}
