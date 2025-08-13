require('dotenv').config();
const express = require('express');
const bodyParser = require('body-parser');
const WebSocket = require('ws');
const cors = require('cors');
const AWS = require('aws-sdk');

const dinoRun = require('./games/dino_run');
const tugOfWar = require('./games/tug_of_war');
const kernelChaos = require('./games/kernel_chaos');


const gameHandlers = {
  dino_run: dinoRun,
  tug_of_war: tugOfWar,
  kernel_chaos: kernelChaos,      // <— NEW
};

const app = express();
const PORT = 3000;

app.use(cors());
app.use(bodyParser.json());

const games = {};
const clients = {};       // Unity sockets
const webClients = {};    // code_username → Web client sockets

// AWS S3 Setup
const s3 = new AWS.S3({
  accessKeyId: process.env.AWS_ACCESS_KEY_ID,
  secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY,
  region: process.env.AWS_REGION
});

function getFormattedTime() {
  return new Date().toISOString();
}

function saveSessionToFile(code) {
  const session = games[code];
  if (!session) return;

  const handler = gameHandlers[session.type];
  if (!handler || !handler.serialize) return;

  const output = handler.serialize(session);
  const jsonData = JSON.stringify(output, null, 2);
  const filename = session.filename || `${code}.json`;

  const s3Key = `cinemagames/${session.location}/${session.type}/${filename}`;

  const params = {
    Bucket: process.env.S3_BUCKET_NAME,
    Key: s3Key,
    Body: jsonData,
    ContentType: 'application/json'
  };

  s3.upload(params).promise()
    .then(data => console.log(`✅ Uploaded session to S3: ${data.Location}`))
    .catch(err => console.error('❌ Failed to upload session to S3:', err));
}

// 🔌 HTTP API Routes
app.post('/connect', (req, res) => {
  const { username, code, gameType, location, team } = req.body;

  const session = games[code];
  if (!session) return res.status(404).json({ error: 'Game not found' });
  if (session.type !== gameType || session.location !== location) {
    return res.status(400).json({ error: 'Game type or location mismatch' });
  }

  const handler = gameHandlers[gameType];
  if (!handler) return res.status(400).json({ error: 'Unsupported game type' });

  const result = handler.onConnect(req, session);
  if (result.error) return res.status(400).json({ error: result.error });
  if (result.status === "full") return res.json({ status: "full" });

  const ws = clients[code];
  if (ws && ws.readyState === WebSocket.OPEN) {
    // Send playerJoin to Unity
    ws.send(JSON.stringify({ type: 'playerJoin', ...result }));
    
    // ADD THIS: Send roleAssignment to Unity
    ws.send(JSON.stringify({
      type: 'roleAssignment',
      username: result.username,
      role: result.role,
      code: code
    }));
  }

  return res.json(result);
});

app.post('/trigger', (req, res) => {
  const { code } = req.body;
  const session = games[code];
  if (!session) return res.status(404).json({ error: 'Invalid game code' });

  const handler = gameHandlers[session.type];
  if (!handler) return res.status(400).json({ error: 'Unsupported game type' });

  const result = handler.onAction(req, session);
  if (result.error) return res.status(400).json({ error: result.error });

  const ws = clients[code];
  if (ws && ws.readyState === WebSocket.OPEN && result.forwardToUnity) {
    ws.send(JSON.stringify(result.forwardToUnity));
  }

  return res.json({ success: true });
});

app.post('/disconnect', (req, res) => {
  const { username, code, gameType } = req.body;
  const session = games[code];
  if (!session) return res.status(404).json({ error: 'Game not found' });

  const handler = gameHandlers[gameType];
  if (handler?.onDisconnect) handler.onDisconnect(username, session);

  return res.json({ status: 'ok' });
});

app.get('/game-status', (req, res) => {
  const { code, username } = req.query;
  const session = games[code];

  if (!session) return res.status(404).json({ error: 'Game not found' });

  const handler = gameHandlers[session.type];
  if (!handler || !handler.getGameStatus) {
    return res.status(400).json({ error: 'Unsupported game type or missing handler' });
  }

  const status = handler.getGameStatus(username, session);
  return res.json(status);
});

// 🔌 WebSocket server (Unity + Web Clients)
const wss = new WebSocket.Server({ port: 8080 });

wss.on('connection', (ws, req) => {
  let code = null;
  let username = null;
  let isUnity = false;

  const url = new URL(req.url, `http://${req.headers.host}`);
  const codeFromUrl = url.pathname.split('/').pop();

  ws.on('message', (msg) => {
    try {
      const data = JSON.parse(msg);

      if (data.type === 'registerClient' && data.username && codeFromUrl) {
        username = data.username;
        code = codeFromUrl;
        const key = `${code}_${username}`;
        webClients[key] = ws;
        console.log(`🧍 Web client registered: ${username} (${code})`);
        return;
      }

      if (data.type === 'gameStart') {
        const session = games[data.code];
        if (session) {
          session.hasStarted = true;
          saveSessionToFile(data.code);
          console.log(`▶️ Game marked as started: ${data.code}`);
        }
        return;
      }

      if (data.type === 'gameEnd') {
        const session = games[data.code];
        if (session) {
          session.timestampEnd = getFormattedTime();
          session.totalPlayersJoined = session.type === 'tug_of_war'
            ? (session.teams.TeamA.length + session.teams.TeamB.length)
            : Object.keys(session.players).length;

          saveSessionToFile(data.code);
          console.log(`🛑 Game ended: ${data.code}`);
          delete games[data.code];
          delete clients[data.code];
        }
        return;
      }

      if (data.type === 'roleAssignment' && data.username && data.code) {
        console.log(`📨 Backend received roleAssignment for ${data.username}`);
        const key = `${data.code}_${data.username}`;
        const clientSocket = webClients[key];
        if (clientSocket && clientSocket.readyState === WebSocket.OPEN) {
          clientSocket.send(JSON.stringify(data));
          console.log(`➡️ Sent roleAssignment to ${data.username}`);
        } else {
          console.warn(`⚠️ Could not send roleAssignment to ${data.username}. Socket not open or not registered.`);
        }
        return;
      }

      if (data.code && data.type && data.location && data.allowedNumberOfPlayers && data.filename) {
        isUnity = true;
        code = data.code;

        const session = {
          code: data.code,
          type: data.type,
          location: data.location,
          allowedNumberOfPlayers: data.allowedNumberOfPlayers,
          timestampStart: getFormattedTime(),
          hasStarted: false,
          filename: data.filename
        };

        if (data.type === 'dino_run') session.players = {};
        if (data.type === 'tug_of_war') session.teams = { TeamA: [], TeamB: [] };

        games[data.code] = session;
        clients[data.code] = ws;

        console.log(`🎮 Game initialized: ${data.code} (${data.type} @ ${data.location}) → ${data.filename}`);
        return;
      }

    } catch (err) {
      console.error('❌ WebSocket message error:', err.message);
    }
  });

  ws.on('close', () => {
    if (isUnity && code) {
      const session = games[code];
      if (session) {
        session.timestampEnd = getFormattedTime();
        session.totalPlayersJoined = session.type === 'tug_of_war'
          ? (session.teams.TeamA.length + session.teams.TeamB.length)
          : Object.keys(session.players).length;

        saveSessionToFile(code);
        console.log(`🛑 Game ended (Unity disconnected): ${code}`);
        delete games[code];
        delete clients[code];
      }
    }

    if (!isUnity && code && username) {
      const key = `${code}_${username}`;
      delete webClients[key];
      console.log(`❌ Web client disconnected: ${username}`);
    }
  });
});

app.listen(PORT, () => {
  console.log(`🚀 HTTP server running at http://localhost:${PORT}`);
});
console.log('🔌 WebSocket server running at ws://localhost:8080');
