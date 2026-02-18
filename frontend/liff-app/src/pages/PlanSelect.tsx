import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getPlans, createCheckout, mockComplete, PlanDto } from '../api/client';

const DEFAULT_STORE_ID = '11111111-1111-1111-1111-111111111111';
const DEFAULT_DOOR_ID = '22222222-2222-2222-2222-222222222222';

function formatDuration(minutes: number): string {
  if (minutes >= 43200) return '30 days';
  if (minutes >= 1440) return '24 hours';
  return `${minutes} min`;
}

export default function PlanSelect() {
  const [plans, setPlans] = useState<PlanDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [purchasing, setPurchasing] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    getPlans(DEFAULT_STORE_ID).then(res => {
      setPlans(res.data);
      setLoading(false);
    });
  }, []);

  const handlePurchase = async (plan: PlanDto) => {
    setPurchasing(plan.id);
    try {
      const res = await createCheckout(plan.id, DEFAULT_STORE_ID, DEFAULT_DOOR_ID);
      // Mock mode: auto-complete payment
      if (res.data.checkoutUrl.includes('mock=true')) {
        await mockComplete(res.data.orderId);
        navigate('/passes');
      } else {
        window.location.href = res.data.checkoutUrl;
      }
    } catch (err) {
      console.error('Purchase error:', err);
      alert('Purchase failed. Please try again.');
    } finally {
      setPurchasing(null);
    }
  };

  if (loading) return <div className="loading">Loading plans...</div>;

  return (
    <div>
      <h2 style={{ marginBottom: 16 }}>Select a Plan</h2>
      <div className="plan-grid">
        {plans.map(plan => (
          <div key={plan.id} className="plan-card" onClick={() => handlePurchase(plan)}>
            <h3>{plan.name}</h3>
            <div className="price">
              ¥{plan.priceYen.toLocaleString()}
              <span> / {formatDuration(plan.durationMinutes)}</span>
            </div>
            <div className="duration">{formatDuration(plan.durationMinutes)} access</div>
            <button
              className="btn btn-primary btn-full"
              disabled={purchasing === plan.id}
            >
              {purchasing === plan.id ? 'Processing...' : 'Purchase'}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
