import { useEffect, useState } from 'react';
import { api } from '../App';

interface Pass {
  id: string;
  planName: string;
  status: string;
  validFrom: string;
  validTo: string;
  usedCount: number;
  maxUses: number;
  storeName: string;
  doorName: string;
  createdAt: string;
}

interface LogEntry {
  id: string;
  accessPassId: string;
  userDisplayName: string | null;
  doorName: string;
  result: string;
  denialReason: string | null;
  scannedAt: string;
}

interface Props {
  onLogout: () => void;
}

type Tab = 'passes' | 'logs';

export default function Dashboard({ onLogout }: Props) {
  const [tab, setTab] = useState<Tab>('passes');
  const [passes, setPasses] = useState<Pass[]>([]);
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchPasses = async () => {
    setLoading(true);
    try {
      const res = await api.get('/admin/passes');
      setPasses(res.data);
    } catch (err) {
      console.error('Failed to fetch passes', err);
    }
    setLoading(false);
  };

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const res = await api.get('/admin/logs');
      setLogs(res.data.data || []);
    } catch (err) {
      console.error('Failed to fetch logs', err);
    }
    setLoading(false);
  };

  useEffect(() => {
    if (tab === 'passes') fetchPasses();
    else fetchLogs();
  }, [tab]);

  const revokePass = async (id: string) => {
    if (!confirm('Revoke this pass?')) return;
    try {
      await api.post(`/admin/passes/${id}/revoke`);
      fetchPasses();
    } catch (err) {
      console.error('Failed to revoke', err);
    }
  };

  const fmtDate = (iso: string) =>
    new Date(iso).toLocaleString('ja-JP', {
      month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit'
    });

  return (
    <div className="admin-app">
      <div className="admin-header">
        <h1>Admin Dashboard</h1>
        <button className="btn btn-sm" onClick={onLogout}>Logout</button>
      </div>

      <div className="admin-nav">
        <button className={tab === 'passes' ? 'active' : ''} onClick={() => setTab('passes')}>
          Passes
        </button>
        <button className={tab === 'logs' ? 'active' : ''} onClick={() => setTab('logs')}>
          Access Logs
        </button>
      </div>

      {loading && <p>Loading...</p>}

      {tab === 'passes' && !loading && (
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Plan</th>
                <th>Status</th>
                <th>Valid From</th>
                <th>Valid To</th>
                <th>Uses</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {passes.map(p => (
                <tr key={p.id}>
                  <td>{p.planName}</td>
                  <td><span className={`badge ${p.status.toLowerCase()}`}>{p.status}</span></td>
                  <td>{fmtDate(p.validFrom)}</td>
                  <td>{fmtDate(p.validTo)}</td>
                  <td>{p.usedCount}/{p.maxUses}</td>
                  <td>
                    {p.status === 'Active' && (
                      <button className="btn btn-danger btn-sm" onClick={() => revokePass(p.id)}>
                        Revoke
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {passes.length === 0 && (
                <tr><td colSpan={6} style={{ textAlign: 'center', padding: 40 }}>No passes found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {tab === 'logs' && !loading && (
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Time</th>
                <th>User</th>
                <th>Door</th>
                <th>Result</th>
                <th>Reason</th>
              </tr>
            </thead>
            <tbody>
              {logs.map(l => (
                <tr key={l.id}>
                  <td>{fmtDate(l.scannedAt)}</td>
                  <td>{l.userDisplayName || '-'}</td>
                  <td>{l.doorName}</td>
                  <td>
                    <span className={`badge ${l.result.toLowerCase().startsWith('granted') ? 'granted' : 'denied'}`}>
                      {l.result}
                    </span>
                  </td>
                  <td>{l.denialReason || '-'}</td>
                </tr>
              ))}
              {logs.length === 0 && (
                <tr><td colSpan={5} style={{ textAlign: 'center', padding: 40 }}>No logs found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
