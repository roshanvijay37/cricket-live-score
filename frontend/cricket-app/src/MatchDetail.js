import React, { useState, useEffect } from 'react';
import './MatchDetail.css';

const MatchDetail = ({ matchId, onBack }) => {
  const [match, setMatch] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeTab, setActiveTab] = useState('score');

  useEffect(() => {
    const fetchDetail = async () => {
      try {
        setLoading(true);
        const apiUrl = process.env.REACT_APP_API_URL || `http://${window.location.hostname}:8080`;
        const response = await fetch(`${apiUrl}/api/cricket/matches/${matchId}`);
        const data = await response.json();
        if (data.success) { setMatch(data.match); setError(null); }
        else setError(data.message);
      } catch (err) { setError('Failed to load match details'); }
      finally { setLoading(false); }
    };
    fetchDetail();
  }, [matchId]);

  const getMatchState = () => {
    if (!match) return 'unknown';
    if (!match.matchStarted) return 'upcoming';
    if (match.matchStarted && !match.matchEnded) return 'live';
    return 'completed';
  };

  const getStateColor = (state) => {
    switch (state) {
      case 'live': return '#28a745';
      case 'upcoming': return '#007bff';
      default: return '#6c757d';
    }
  };

  const getRoleIcon = (role) => {
    if (!role) return '🏏';
    const r = role.toLowerCase();
    if (r.includes('wk') || r.includes('keeper')) return '🧤';
    if (r.includes('bowl')) return '🎳';
    if (r.includes('allrounder')) return '⭐';
    return '🏏';
  };

  if (loading) return <div className="detail-loading">Loading match details...</div>;
  if (error) return (
    <div className="detail-error">
      <p>{error}</p>
      <button onClick={onBack} className="back-btn">← Back to Matches</button>
    </div>
  );
  if (!match) return null;

  const state = getMatchState();
  const hasSquad = match.squads && match.squads.length > 0;

  return (
    <div className="match-detail">
      <button onClick={onBack} className="back-btn">← Back to Matches</button>

      <div className="detail-card">
        <div className="detail-header">
          <span className="detail-status" style={{ backgroundColor: getStateColor(state) }}>
            {state.toUpperCase()}
            {state === 'live' && <span className="live-dot">●</span>}
          </span>
          <span className="detail-type">{match.matchType}</span>
        </div>

        <h2 className="detail-name">{match.name}</h2>

        <div className="detail-teams">
          <div className="detail-team">
            <img src={match.team1Img} alt={match.team1} className="detail-team-img"
              onError={(e) => { e.target.style.display = 'none'; }} />
            <span className="detail-team-name">{match.team1}</span>
          </div>
          <span className="detail-vs">VS</span>
          <div className="detail-team">
            <img src={match.team2Img} alt={match.team2} className="detail-team-img"
              onError={(e) => { e.target.style.display = 'none'; }} />
            <span className="detail-team-name">{match.team2}</span>
          </div>
        </div>

        <div className="detail-result">{match.status}</div>

        {/* Tabs */}
        <div className="detail-tabs">
          <button className={`tab-btn ${activeTab === 'score' ? 'active' : ''}`} onClick={() => setActiveTab('score')}>
            📊 Scorecard
          </button>
          <button className={`tab-btn ${activeTab === 'info' ? 'active' : ''}`} onClick={() => setActiveTab('info')}>
            📋 Info
          </button>
          {hasSquad && (
            <button className={`tab-btn ${activeTab === 'squad' ? 'active' : ''}`} onClick={() => setActiveTab('squad')}>
              👥 Squad
            </button>
          )}
        </div>

        {/* Score Tab */}
        {activeTab === 'score' && (
          <>
            {match.scoreDetails && match.scoreDetails.length > 0 ? (
              <div className="scorecard">
                <table>
                  <thead>
                    <tr><th>Innings</th><th>Runs</th><th>Wkts</th><th>Overs</th></tr>
                  </thead>
                  <tbody>
                    {match.scoreDetails.map((s, i) => (
                      <tr key={i}>
                        <td>{s.inning}</td>
                        <td className="score-num">{s.runs}</td>
                        <td className="score-num">{s.wickets}</td>
                        <td className="score-num">{s.overs}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : match.score && match.score !== 'Score not available' ? (
              <div className="score-summary">
                {match.score.split(' | ').map((s, i) => (
                  <div key={i} className="score-line">{s}</div>
                ))}
              </div>
            ) : (
              <p className="no-score">Score not available yet</p>
            )}
          </>
        )}

        {/* Info Tab */}
        {activeTab === 'info' && (
          <div className="detail-info">
            <div className="info-grid">
              <div className="info-item">
                <span className="info-label">📍 Venue</span>
                <span className="info-value">{match.venue || 'N/A'}</span>
              </div>
              <div className="info-item">
                <span className="info-label">📅 Date</span>
                <span className="info-value">
                  {match.startTime ? new Date(match.startTime).toLocaleDateString('en-US', {
                    weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
                  }) : 'N/A'}
                </span>
              </div>
              {match.tossWinner && (
                <div className="info-item">
                  <span className="info-label">🪙 Toss</span>
                  <span className="info-value">{match.tossWinner} won the toss and chose to {match.tossChoice}</span>
                </div>
              )}
              <div className="info-item">
                <span className="info-label">🏏 Format</span>
                <span className="info-value">{match.matchType}</span>
              </div>
            </div>
          </div>
        )}

        {/* Squad Tab */}
        {activeTab === 'squad' && hasSquad && (
          <div className="squad-section">
            {match.squads.map((team, ti) => (
              <div key={ti} className="team-squad">
                <div className="squad-team-header">
                  <img src={team.img} alt={team.teamName} className="squad-team-img"
                    onError={(e) => { e.target.style.display = 'none'; }} />
                  <h3>{team.teamName}</h3>
                </div>
                <div className="players-list">
                  {team.players && team.players.map((player, pi) => (
                    <div key={pi} className="player-card">
                      <div className="player-info">
                        <span className="player-role-icon">{getRoleIcon(player.role)}</span>
                        <div>
                          <div className="player-name">{player.name}</div>
                          <div className="player-role">{player.role || 'Player'}</div>
                        </div>
                      </div>
                      <div className="player-styles">
                        {player.battingStyle && <span className="style-tag bat">🏏 {player.battingStyle}</span>}
                        {player.bowlingStyle && <span className="style-tag bowl">🎳 {player.bowlingStyle}</span>}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default MatchDetail;