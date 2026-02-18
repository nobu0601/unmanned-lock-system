import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyPasses, PassDto } from '../api/client';

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString('ja-JP', {
    month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
}

export default function PassList() {
  const [passes, setPasses] = useState<PassDto[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    getMyPasses().then(res => {
      setPasses(res.data);
      setLoading(false);
    });
  }, []);

  if (loading) return <div className="loading">Loading passes...</div>;

  if (passes.length === 0) {
    return (
      <div style={{ textAlign: 'center', padding: 40 }}>
        <p style={{ color: '#666', marginBottom: 16 }}>No passes yet</p>
        <button className="btn btn-primary" onClick={() => navigate('/')}>
          Purchase a Plan
        </button>
      </div>
    );
  }

  return (
    <div>
      <h2 style={{ marginBottom: 16 }}>My Passes</h2>
      <div className="pass-list">
        {passes.map(pass => (
          <div key={pass.id} className={`pass-card ${pass.status.toLowerCase()}`}>
            <span className={`status ${pass.status.toLowerCase()}`}>{pass.status}</span>
            <h3>{pass.planName}</h3>
            <div className="info">
              <div>Store: {pass.storeName}</div>
              <div>Door: {pass.doorName}</div>
              <div>Valid: {formatDate(pass.validFrom)} - {formatDate(pass.validTo)}</div>
              <div>Uses: {pass.usedCount} / {pass.maxUses}</div>
            </div>
            {pass.status === 'Active' && (
              <button
                className="btn btn-primary btn-full"
                onClick={() => navigate(`/passes/${pass.id}/qr`)}
              >
                Show QR Code
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
