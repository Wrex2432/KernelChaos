module.exports = {
  onConnect(req, session) {
    const { username } = req.body;
    if (!session.players) session.players = {};
    if (!session.roles) session.roles = {};

    const isAlreadyJoined = Object.values(session.players).includes(username);
    const isGameStarted = !!session.hasStarted;

    if (isGameStarted) {
      if (isAlreadyJoined) return { username };
      return { error: 'Game already started. No new players allowed.' };
    }

    if (isAlreadyJoined) {
      return { error: 'Username already taken in this session.' };
    }

    const currentCount = Object.values(session.players).filter(Boolean).length;
    if (currentCount >= session.allowedNumberOfPlayers) {
      return { status: "full", error: "Game is full." };
    }

    let assignedKey = null;
    for (let i = 1; i <= session.allowedNumberOfPlayers; i++) {
      const key = `player${i}`;
      if (!session.players[key]) {
        assignedKey = key;
        break;
      }
    }

    if (!assignedKey) {
      return { status: "full", error: "No available player slots." };
    }

    session.players[assignedKey] = username;
    session.roles[username] = "player"; // ✅ Add role explicitly

    const playerNumber = parseInt(assignedKey.replace('player', ''));
    return { username, playerNumber, role: "player" }; // ✅ Include role here
  },

  onAction(req, session) {
    const { username, action } = req.body;
    return {
      forwardToUnity: {
        type: 'action',
        username,
        action
      }
    };
  },

  onDisconnect(username, session) {
    if (!session.players || session.hasStarted) return;

    const entry = Object.entries(session.players).find(([, value]) => value === username);
    if (entry) {
      const [key] = entry;
      delete session.players[key];
      delete session.roles?.[username];
    }
  },

  serialize(session) {
    return {
      code: session.code,
      type: session.type,
      location: session.location,
      allowedNumberOfPlayers: session.allowedNumberOfPlayers,
      dinorunNumberOfPlayerPicked: session.dinorunNumberOfPlayerPicked || 0,
      timestampStart: session.timestampStart,
      timestampEnd: session.timestampEnd,
      totalPlayersJoined: Object.values(session.players || {}).filter(Boolean).length,
      players: session.players,
      roles: session.roles || {}
    };
  },

  // ✅ Added for polling support
  getGameStatus(username, session) {
    return {
      gameStarted: !!session.hasStarted,
      role: session.roles?.[username] || null
    };
  }
};
