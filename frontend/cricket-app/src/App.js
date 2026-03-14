import React, { useState, useEffect } from 'react';
import CricketMatches from './CricketMatches';
import './App.css';

function App() {
  const [installPrompt, setInstallPrompt] = useState(null);
  const [showInstall, setShowInstall] = useState(false);

  useEffect(() => {
    const handler = (e) => {
      e.preventDefault();
      setInstallPrompt(e);
      setShowInstall(true);
    };
    window.addEventListener('beforeinstallprompt', handler);
    return () => window.removeEventListener('beforeinstallprompt', handler);
  }, []);

  const handleInstall = async () => {
    if (!installPrompt) return;
    installPrompt.prompt();
    const result = await installPrompt.userChoice;
    if (result.outcome === 'accepted') {
      setShowInstall(false);
    }
    setInstallPrompt(null);
  };

  return (
    <div className="App">
      {showInstall && (
        <div className="install-banner">
          <span>📱 Install Cricket Live Scores as an app!</span>
          <button onClick={handleInstall} className="install-btn">Install</button>
          <button onClick={() => setShowInstall(false)} className="dismiss-btn">✕</button>
        </div>
      )}
      <CricketMatches />
    </div>
  );
}

export default App;