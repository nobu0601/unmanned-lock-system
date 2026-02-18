import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { mockComplete } from '../api/client';

export default function CheckoutReturn() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState<'processing' | 'success' | 'error'>('processing');

  useEffect(() => {
    const sessionId = searchParams.get('session_id');
    const isMock = searchParams.get('mock') === 'true';

    if (!sessionId) {
      setStatus('error');
      return;
    }

    if (isMock) {
      // Mock: payment already completed in PlanSelect
      setStatus('success');
      setTimeout(() => navigate('/passes'), 2000);
      return;
    }

    // Real Stripe: payment confirmed via webhook
    setStatus('success');
    setTimeout(() => navigate('/passes'), 3000);
  }, [searchParams, navigate]);

  return (
    <div style={{ textAlign: 'center', padding: 40 }}>
      {status === 'processing' && (
        <div className="message">Processing payment...</div>
      )}
      {status === 'success' && (
        <div className="message success">
          Payment successful! Redirecting to your passes...
        </div>
      )}
      {status === 'error' && (
        <div className="message error">
          Payment failed. Please try again.
          <button className="btn btn-primary btn-full" onClick={() => navigate('/')}>
            Back to Plans
          </button>
        </div>
      )}
    </div>
  );
}
