# FX Trader Challenge - Mock Server

Simple Express.js mock server for testing backend integration.

## Installation

```bash
npm install
```

## Usage

```bash
npm start
```

Server runs on `http://localhost:3000` by default.

## Endpoints

### POST /auth/login
Authenticates user and returns JWT token.

**Request:**
```json
{
  "username": "testuser",
  "password": "optional"
}
```

**Response:**
```json
{
  "token": "jwt_token_here"
}
```

### GET /api/fx_rates
Returns current FX rates.

**Response:**
```json
{
  "timestamp": "2025-11-27T12:00:00Z",
  "rates": {
    "USD_SGD": 1.3567,
    "USD_INR": 83.45,
    "EUR_USD": 1.088
  }
}
```

### GET /api/fx_rates/history
Returns historical FX rate data.

**Query Parameters:**
- `pair` (optional): Currency pair (default: USD_SGD)
- `since` (optional): ISO timestamp (default: last hour)

**Response:**
```json
[
  {
    "timestamp": "2025-11-27T11:00:00Z",
    "pair": "USD_SGD",
    "rate": 1.3567
  }
]
```

### POST /api/game/score
Persists player score.

**Headers:**
```
Authorization: Bearer <token>
```

**Request:**
```json
{
  "username": "testuser",
  "score": 15200,
  "credits": 124500,
  "stats": {
    "wins": 12,
    "losses": 3
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Score saved"
}
```

### GET /api/leaderboard
Returns top 10 players.

**Response:**
```json
[
  {
    "rank": 1,
    "username": "alice",
    "score": 12500
  },
  {
    "rank": 2,
    "username": "bob",
    "score": 11000
  }
]
```

## Notes

- All authentication accepts any username/password for testing
- FX rates are simulated with sinusoidal variation
- Leaderboard is stored in-memory and resets on server restart
- CORS is enabled for all origins

