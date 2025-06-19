import React from 'react';
import './App.css'; // Keep App.css for basic app-wide styling if needed, though mostly empty now
import Home from './Home';

function App() {
  return (
    <div className="App">
      {/* App.js now only renders the Home component, which contains the content */}
      <Home />
    </div>
  );
}

export default App;