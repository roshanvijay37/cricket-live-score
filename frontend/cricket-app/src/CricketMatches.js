import React, { useState, useEffect, useCallback } from 'react';
import './CricketMatches.css';

const CricketMatches = () => {
  const [matches, setMatches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [lastUpdated, setLastUpdated] = useState(null);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [filter, setFilter] = useState('all');

  const fetchMatches = useCallback(async () => {
    try {
      setLoading(true);
      const apiUrl = process.env.REACT_APP_API_URL || `http://${window.location.hostname}:8080`;
      const response = await fetch(`${apiUrl}/api/cricket/matches`);
      
      if (!response.ok) throw new Error('Failed to fetch matches');
      
      const data = await response.json();
      
      if (data.success) {
        setMatches(data.matches);
        setError(null);
        setLastUpdated(new Date());
      } else {
        setError(data.message);
      }
    } catch (err) {
      setError('Unable to connect to cricket API. Make sure the backend is running.');
      console.error('Error fetching matches:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchMatches(); }, [fetchMatches]);

  useEffect(() => {
    if (!autoRefresh) return;
    const interval = setInterval(fetchMatches, 30000);
    return () => clearInterval(interval);
  }, [fetchMatches, autoRefresh]);

  const getMatchState = (match) => {
    if (!match.matchStarted) return 'upcoming';
    if (match.matchStarted && !match.matchEnded) return 'live';
    return 'completed';
  };

  const getStatusColor = (state) => {
    switch (state) {
      case 'live': return '#28a745';
      case 'upcoming': return '#007bff';
      case 'completed': return '#6c757d';
      default: return '#6c757d';
    }
  };

  const filteredMatches = matches.filter((match) => {
    if (filter === 'all') return true;
    return getMatchState(match) === filter;
  });

  if (loading && matches.length === 0) {
    return <div className="loading">Loading cricket matches...</div>;
  }

  if (error && matches.length === 0) {
    return (
      <div className="error">
        <p>{error}</p>
        <button onClick={fetchMatches} className="retry-btn">Retry</button>
      </div>
    );
  }

  return (
    <div className="cricket-matches">
      <div className="header">
        <h1>🏏 Cricket Live Scores</h1>
        <div className="controls">
          <div className="auto-refresh-toggle">
            <label>
              <input type="checkbox" checked={autoRefresh} onChange={() => setAutoRefresh(!autoRefresh)} />
              Auto-refresh (30s)
            </label>
          </div>
          {lastUpdated && (
            <div className="last-updated">Last updated: {lastUpdated.toLocaleTimeString()}</div>
          )}
        </div>

        <div className="filter-buttons">
          {['all', 'live', 'upcoming', 'completed'].map((f) => (
            <button
              key={f}
              className={`filter-btn ${filter === f ? 'active' : ''}`}
              onClick={() => setFilter(f)}
            >
              {f.charAt(0).toUpperCase() + f.slice(1)}
            </button>
          ))}
        </div>
      </div>

      {error && <div className="error-banner">⚠️ {error}</div>}

      <div className="matches-container">
        {filteredMatches.length === 0 ? (
          <p className="no-matches">No {filter !== 'all' ? filter : ''} matches available</p>
        ) : (
          filteredMatches.map((match) => {
            const state = getMatchState(match);
            return (
              <div key={match.matchId} className={`match-card ${state}`}>
                <div className="match-header">
                  <span className="match-status" style={{ backgroundColor: getStatusColor(state) }}>
                    {state.toUpperCase()}
                    {state === 'live' && <span className="live-indicator">●</span>}
                  </span>
                  <span className="match-type">{match.matchType}</span>
                </div>

                <div className="teams">
                  <div className="team">
                    <img src={match.team1Img} alt={match.team1} className="team-img"
                      onError={(e) => { e.target.style.display = 'none'; }} />
                    <span>{match.team1}</span>
                  </div>
                  <span className="vs">vs</span>
                  <div className="team">
                    <img src={match.team2Img} alt={match.team2} className="team-img"
                      onError={(e) => { e.target.style.display = 'none'; }} />
                    <span>{match.team2}</span>
                  </div>
                </div>

                <div className="score">
                  {match.score && match.score !== 'Score not available' ? (
                    match.score.split(' | ').map((s, i) => <p key={i}>{s}</p>)
                  ) : (
                    <p>Score not available yet</p>
                  )}
                </div>

                <div className="match-status-text">{match.status}</div>

                <div className="match-details">
                  <p>📍 {match.venue}</p>
                  {match.startTime && <p>📅 {new Date(match.startTime).toLocaleString()}</p>}
                </div>
              </div>
            );
          })
        )}
      </div>

      <div className="action-buttons">
        <button onClick={fetchMatches} className="refresh-btn" disabled={loading}>
          {loading ? '🔄 Updating...' : '🔄 Refresh Now'}
        </button>
      </div>
    </div>
  );
};

export default CricketMatches;