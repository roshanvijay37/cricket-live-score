import React, { useState, useEffect, useCallback } from 'react';
import './CricketMatches.css';

const CricketMatches = () => {
  const [matches, setMatches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [lastUpdated, setLastUpdated] = useState(null);
  const [autoRefresh, setAutoRefresh] = useState(true);

  const fetchMatches = useCallback(async () => {
    try {
      setLoading(true);
      const apiUrl = `http://${window.location.hostname}:8080`;
      const response = await fetch(`${apiUrl}/api/cricket/matches`);
      
      if (!response.ok) {
        throw new Error('Failed to fetch matches');
      }
      
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

  // Initial fetch
  useEffect(() => {
    fetchMatches();
  }, [fetchMatches]);

  // Auto-refresh every 30 seconds
  useEffect(() => {
    if (!autoRefresh) return;

    const interval = setInterval(() => {
      fetchMatches();
    }, 30000); // 30 seconds

    return () => clearInterval(interval);
  }, [fetchMatches, autoRefresh]);

  const toggleAutoRefresh = () => {
    setAutoRefresh(!autoRefresh);
  };

  const getStatusColor = (status) => {
    switch (status.toLowerCase()) {
      case 'live': return '#28a745';
      case 'upcoming': return '#007bff';
      case 'completed': return '#6c757d';
      default: return '#6c757d';
    }
  };

  if (loading && matches.length === 0) {
    return <div className="loading">Loading cricket matches...</div>;
  }

  if (error && matches.length === 0) {
    return (
      <div className="error">
        <p>{error}</p>
        <button onClick={fetchMatches} className="retry-btn">
          Retry
        </button>
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
              <input
                type="checkbox"
                checked={autoRefresh}
                onChange={toggleAutoRefresh}
              />
              Auto-refresh (30s)
            </label>
          </div>
          
          {lastUpdated && (
            <div className="last-updated">
              Last updated: {lastUpdated.toLocaleTimeString()}
            </div>
          )}
        </div>
      </div>
      
      {error && (
        <div className="error-banner">
          ⚠️ {error}
        </div>
      )}
      
      <div className="matches-container">
        {matches.length === 0 ? (
          <p>No matches available</p>
        ) : (
          matches.map((match) => (
            <div key={match.matchId} className="match-card">
              <div className="match-header">
                <span 
                  className="match-status"
                  style={{ backgroundColor: getStatusColor(match.status) }}
                >
                  {match.status}
                  {match.status.toLowerCase() === 'live' && (
                    <span className="live-indicator">●</span>
                  )}
                </span>
                <span className="match-type">{match.matchType}</span>
              </div>
              
              <div className="teams">
                <h3>{match.team1} vs {match.team2}</h3>
              </div>
              
              <div className="score">
                <p>{match.score}</p>
              </div>
              
              <div className="match-details">
                <p><strong>Venue:</strong> {match.venue}</p>
                <p><strong>Time:</strong> {new Date(match.startTime).toLocaleString()}</p>
              </div>
            </div>
          ))
        )}
      </div>
      
      <div className="action-buttons">
        <button 
          onClick={fetchMatches} 
          className="refresh-btn"
          disabled={loading}
        >
          {loading ? '🔄 Updating...' : '🔄 Refresh Now'}
        </button>
      </div>
    </div>
  );
};

export default CricketMatches;