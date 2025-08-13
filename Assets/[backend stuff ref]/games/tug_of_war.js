module.exports = {
  onConnect(req, session) {
    const { username, team } = req.body;
    if (!session.teams) session.teams = { TeamA: [], TeamB: [] };

    // Auto-assign if team is not manually provided or invalid
    let chosenTeam = team;
    const validTeams = ['TeamA', 'TeamB'];

    if (!validTeams.includes(chosenTeam)) {
      const aCount = session.teams.TeamA.length;
      const bCount = session.teams.TeamB.length;

      if (aCount === 0 && bCount === 0) {
        chosenTeam = 'TeamA'; // First player always goes to TeamA
      } else {
        chosenTeam = aCount <= bCount ? 'TeamA' : 'TeamB';
      }
    }

    const allPlayers = [...session.teams.TeamA, ...session.teams.TeamB];
    const isAlreadyJoined = allPlayers.includes(username);
    const isGameStarted = !!session.hasStarted;

    if (isGameStarted) {
      if (isAlreadyJoined) return { username, team: chosenTeam };
      return { error: 'Game already started. No new players allowed.' };
    }

    if (isAlreadyJoined) {
      return { error: 'Username already taken in this session.' };
    }

    const totalPlayers = allPlayers.length;
    if (totalPlayers >= session.allowedNumberOfPlayers) {
      return { status: "full", error: "Game is full." };
    }

    session.teams[chosenTeam].push(username);
    return { username, team: chosenTeam };
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
    if (!session.teams) return;
    if (session.hasStarted) return;

    for (const teamName of ['TeamA', 'TeamB']) {
      const index = session.teams[teamName].indexOf(username);
      if (index !== -1) {
        session.teams[teamName].splice(index, 1);
        break;
      }
    }
  },

  serialize(session) {
    return {
      code: session.code,
      type: session.type,
      location: session.location,
      allowedNumberOfPlayers: session.allowedNumberOfPlayers,
      timestampStart: session.timestampStart,
      timestampEnd: session.timestampEnd,
      totalPlayersJoined: (session.teams?.TeamA?.length || 0) + (session.teams?.TeamB?.length || 0),
      teams: session.teams
    };
  }
};
