// games/kernel_chaos.js
// Minimal, DinoRun-style handler: per-user roles (P1..Pn), simple start/end,
// and optional "action" messages for future web controls.

function ensureSessionShape(session) {
  if (!session.players) session.players = {};            // role -> username
  if (!session.roles) session.roles = {};                // username -> role
  if (!session.hasStarted) session.hasStarted = false;
  if (!session.actions) session.actions = [];            // optional action log
}

function nextAvailableRole(session, maxPlayers) {
  for (let i = 1; i <= maxPlayers; i++) {
    const key = `P${i}`;
    if (!session.players[key]) return key;
  }
  return null;
}

module.exports = {
  /**
   * HTTP join handler (called by /connect in server.js)
   * Contract: returns { username, role } OR { error, status? }
   */
  onConnect(req, session) {
    ensureSessionShape(session);

    const { username } = req.body;
    if (!username || typeof username !== "string") {
      return { error: "Username is required." };
    }

    const isGameStarted = !!session.hasStarted;
    const alreadyJoined = Object.values(session.players).includes(username);

    // Lock session after start, but allow rejoin for the same username
    if (isGameStarted && !alreadyJoined) {
      return { error: "Game already started. No new players allowed." };
    }

    // Fast path: user rejoining
    if (alreadyJoined) {
      return { username, role: session.roles[username] };
    }

    // Capacity check
    const max = Number(session.allowedNumberOfPlayers) || 1;
    const availableRole = nextAvailableRole(session, max);
    if (!availableRole) {
      return { status: "full", error: "Game is full." };
    }

    // Assign role
    session.players[availableRole] = username;
    session.roles[username] = availableRole;

    return { username, role: availableRole };
  },

  /**
   * Optional: called when Unity (or host) starts the match.
   * You likely trigger this from your server.js when receiving a "start" WS msg from Unity.
   */
  onStart(session) {
    ensureSessionShape(session);
    if (!session.timestampStart) session.timestampStart = new Date().toISOString();
    session.hasStarted = true;
    return { ok: true };
  },

  /**
   * Optional: called when Unity (or host) ends the match.
   */
  onEnd(session) {
    ensureSessionShape(session);
    session.hasStarted = false;
    session.timestampEnd = new Date().toISOString();
    return { ok: true };
  },

  /**
   * Optional: handle messages coming from web clients
   * Expected payload: { type: "action", username, action }
   * For now we just log/store; Unity can poll or receive relays if you wire that in server.js.
   */
  onWebMessage(ws, data, session) {
    ensureSessionShape(session);
    try {
      const msg = typeof data === "string" ? JSON.parse(data) : data;
      if (!msg || typeof msg !== "object") return;

      if (msg.type === "action" && msg.username && msg.action) {
        session.actions.push({
          t: Date.now(),
          username: msg.username,
          action: msg.action,
        });
        // (Optional) Relay to Unity here if your server.js supports it.
        // ws.send(JSON.stringify({ type: "ack", action: msg.action }));
      }

      if (msg.type === "start") this.onStart(session);
      if (msg.type === "end") this.onEnd(session);
    } catch (e) {
      // ignore bad JSON
    }
  },

  /**
   * Optional: handle messages from Unity client (if you use that flow)
   * e.g., Unity drives the state: start/end, player events, etc.
   */
  onUnityMessage(ws, data, session) {
    ensureSessionShape(session);
    try {
      const msg = typeof data === "string" ? JSON.parse(data) : data;
      if (!msg || typeof msg !== "object") return;

      if (msg.type === "start") this.onStart(session);
      if (msg.type === "end") this.onEnd(session);
      // You can add Unity-driven scoring/events here later.
    } catch (e) {
      // ignore bad JSON
    }
  },

  /**
   * Compact snapshot for /game-status (mirrors your other games)
   */
  serialize(session) {
    ensureSessionShape(session);
    const totalPlayersJoined = Object.values(session.players).filter(Boolean).length;

    return {
      code: session.code,
      type: session.type,
      location: session.location,
      allowedNumberOfPlayers: session.allowedNumberOfPlayers,
      timestampStart: session.timestampStart,
      timestampEnd: session.timestampEnd,
      hasStarted: !!session.hasStarted,
      totalPlayersJoined,
      players: session.players,
    };
  },
};
