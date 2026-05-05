export interface Player {
  playerId: string;
  displayName: string;
  seatIndex: number;
  isConnected: boolean;
}

export interface Room {
  roomCode: string;
  gameType: string;
  status: string;
  players: Player[];
  hostPlayerId: string;
  maxPlayers: number;
}
