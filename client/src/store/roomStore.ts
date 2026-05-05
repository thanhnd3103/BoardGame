import { create } from 'zustand';
import type { Player, Room } from '../types/room';

interface RoomState {
  roomCode: string | null;
  gameType: string | null;
  players: Player[];
  hostPlayerId: string | null;
  status: string | null;
  setRoom: (room: Room) => void;
  clear: () => void;
}

export const useRoomStore = create<RoomState>((set) => ({
  roomCode: null,
  gameType: null,
  players: [],
  hostPlayerId: null,
  status: null,
  setRoom: (room) =>
    set({
      roomCode: room.roomCode,
      gameType: room.gameType,
      players: room.players,
      hostPlayerId: room.hostPlayerId,
      status: room.status,
    }),
  clear: () =>
    set({
      roomCode: null,
      gameType: null,
      players: [],
      hostPlayerId: null,
      status: null,
    }),
}));
