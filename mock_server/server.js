const express = require('express');
const cors = require('cors');
const jwt = require('jsonwebtoken');

const app = express();
const PORT = process.env.PORT || 3000;
const JWT_SECRET = 'mock-secret-key-change-in-production';

// Middleware
app.use(cors());
app.use(express.json());

// In-memory storage
let leaderboard = [
  { rank: 1, username: 'alice', score: 12500 },
  { rank: 2, username: 'bob', score: 11000 },
  { rank: 3, username: 'charlie', score: 9500 },
  { rank: 4, username: 'diana', score: 8200 },
  { rank: 5, username: 'eve', score: 7500 }
];

let scores = {}; // username -> score data

// Mock FX rates (simulated with slight variations)
const baseRates = {
  USD_SGD: 1.3567,
  USD_INR: 83.45,
  EUR_USD: 1.088
};

function getCurrentRates() {
  const now = new Date();
  const variation = Math.sin(now.getTime() / 10000) * 0.01; // Small variation
  
  return {
    timestamp: now.toISOString(),
    rates: {
      USD_SGD: baseRates.USD_SGD * (1 + variation),
      USD_INR: baseRates.USD_INR * (1 + variation * 0.5),
      EUR_USD: baseRates.EUR_USD * (1 + variation * 0.8)
    }
  };
}

// Routes

// POST /auth/login
app.post('/auth/login', (req, res) => {
  const { username, password } = req.body;
  
  // Accept any username/password for mock server
  const token = jwt.sign(
    { username: username || 'guest', userId: Math.random().toString(36) },
    JWT_SECRET,
    { expiresIn: '24h' }
  );
  
  res.json({ token });
});

// GET /api/fx_rates
app.get('/api/fx_rates', (req, res) => {
  res.json(getCurrentRates());
});

// GET /api/fx_rates/history
app.get('/api/fx_rates/history', (req, res) => {
  const { pair = 'USD_SGD', since } = req.query;
  
  // Generate mock historical data
  const history = [];
  const now = new Date();
  const sinceDate = since ? new Date(since) : new Date(now.getTime() - 3600000); // Default: last hour
  
  const baseRate = baseRates[pair] || baseRates.USD_SGD;
  
  for (let i = 0; i < 100; i++) {
    const timestamp = new Date(sinceDate.getTime() + i * 36000); // Every 36 seconds
    const variation = Math.sin(i / 10) * 0.01;
    
    history.push({
      timestamp: timestamp.toISOString(),
      pair: pair,
      rate: baseRate * (1 + variation)
    });
  }
  
  res.json(history);
});

// POST /api/game/score
app.post('/api/game/score', (req, res) => {
  const { username, score, credits, stats } = req.body;
  
  // Store score
  scores[username] = {
    username,
    score,
    credits,
    stats,
    updatedAt: new Date().toISOString()
  };
  
  // Update leaderboard
  updateLeaderboard();
  
  res.json({ success: true, message: 'Score saved' });
});

// GET /api/leaderboard
app.get('/api/leaderboard', (req, res) => {
  res.json(leaderboard);
});

// Helper function to update leaderboard
function updateLeaderboard() {
  const entries = Object.values(scores)
    .map(score => ({
      username: score.username,
      score: score.score || score.credits || 0
    }))
    .sort((a, b) => b.score - a.score)
    .slice(0, 10)
    .map((entry, index) => ({
      rank: index + 1,
      username: entry.username,
      score: entry.score
    }));
  
  leaderboard = entries.length > 0 ? entries : leaderboard;
}

// Health check
app.get('/health', (req, res) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

// Start server
app.listen(PORT, () => {
  console.log(`FX Trader Mock Server running on http://localhost:${PORT}`);
  console.log('Endpoints:');
  console.log('  POST /auth/login');
  console.log('  GET  /api/fx_rates');
  console.log('  GET  /api/fx_rates/history?pair=USD_SGD');
  console.log('  POST /api/game/score');
  console.log('  GET  /api/leaderboard');
});

