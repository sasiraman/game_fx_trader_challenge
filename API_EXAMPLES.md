# API Examples

This document provides curl and Postman examples for the FX Trader Challenge backend API.

## Base URL

```
http://localhost:3000
```

## Authentication

### Login

**Endpoint:** `POST /auth/login`

**cURL:**
```bash
curl -X POST http://localhost:3000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "optional"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

## FX Rates

### Get Current Rates

**Endpoint:** `GET /api/fx_rates`

**cURL:**
```bash
curl http://localhost:3000/api/fx_rates
```

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

### Get Historical Rates

**Endpoint:** `GET /api/fx_rates/history`

**Query Parameters:**
- `pair` (optional): Currency pair (e.g., `USD_SGD`, `USD_INR`, `EUR_USD`)
- `since` (optional): ISO timestamp (e.g., `2025-11-27T11:00:00Z`)

**cURL:**
```bash
curl "http://localhost:3000/api/fx_rates/history?pair=USD_SGD&since=2025-11-27T11:00:00Z"
```

**Response:**
```json
[
  {
    "timestamp": "2025-11-27T11:00:00Z",
    "pair": "USD_SGD",
    "rate": 1.3567
  },
  {
    "timestamp": "2025-11-27T11:00:36Z",
    "pair": "USD_SGD",
    "rate": 1.3568
  }
]
```

## Game Score

### Post Score

**Endpoint:** `POST /api/game/score`

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**cURL:**
```bash
curl -X POST http://localhost:3000/api/game/score \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "score": 15200,
    "credits": 124500,
    "stats": {
      "wins": 12,
      "losses": 3
    }
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Score saved"
}
```

## Leaderboard

### Get Leaderboard

**Endpoint:** `GET /api/leaderboard`

**cURL:**
```bash
curl http://localhost:3000/api/leaderboard
```

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
  },
  {
    "rank": 3,
    "username": "charlie",
    "score": 9500
  }
]
```

## Postman Collection

Import the following JSON into Postman:

```json
{
  "info": {
    "name": "FX Trader Challenge API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Login",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"username\": \"testuser\",\n  \"password\": \"optional\"\n}"
        },
        "url": {
          "raw": "http://localhost:3000/auth/login",
          "protocol": "http",
          "host": ["localhost"],
          "port": "3000",
          "path": ["auth", "login"]
        }
      }
    },
    {
      "name": "Get FX Rates",
      "request": {
        "method": "GET",
        "url": {
          "raw": "http://localhost:3000/api/fx_rates",
          "protocol": "http",
          "host": ["localhost"],
          "port": "3000",
          "path": ["api", "fx_rates"]
        }
      }
    },
    {
      "name": "Get Historical Rates",
      "request": {
        "method": "GET",
        "url": {
          "raw": "http://localhost:3000/api/fx_rates/history?pair=USD_SGD",
          "protocol": "http",
          "host": ["localhost"],
          "port": "3000",
          "path": ["api", "fx_rates", "history"],
          "query": [
            {
              "key": "pair",
              "value": "USD_SGD"
            }
          ]
        }
      }
    },
    {
      "name": "Post Score",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}",
            "type": "text"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"username\": \"testuser\",\n  \"score\": 15200,\n  \"credits\": 124500,\n  \"stats\": {\n    \"wins\": 12,\n    \"losses\": 3\n  }\n}"
        },
        "url": {
          "raw": "http://localhost:3000/api/game/score",
          "protocol": "http",
          "host": ["localhost"],
          "port": "3000",
          "path": ["api", "game", "score"]
        }
      }
    },
    {
      "name": "Get Leaderboard",
      "request": {
        "method": "GET",
        "url": {
          "raw": "http://localhost:3000/api/leaderboard",
          "protocol": "http",
          "host": ["localhost"],
          "port": "3000",
          "path": ["api", "leaderboard"]
        }
      }
    }
  ]
}
```

## Testing with Mock Server

1. Start the mock server:
```bash
cd mock_server
npm install
npm start
```

2. Test endpoints using the examples above.

3. The mock server accepts any username/password for authentication.

4. FX rates are simulated and change slightly over time.

5. Leaderboard is stored in-memory and resets on server restart.

