import { useEffect } from 'react';
import { getConnection, ensureConnected } from '../services/signalRService';
import { useRoomStore } from '../store/roomStore';
import { useGameStore } from '../store/gameStore';
import type { Room } from '../types/room';
import type { GameStateDto } from '../types/gameState';

export function useSignalR() {
  useEffect(() => {
    const conn = getConnection();

    conn.on('RoomUpdated', (room: Room) => {
      useRoomStore.getState().setRoom(room);
    });

    conn.on('GameStarted', (state: GameStateDto) => {
      useGameStore.getState().setGameState(state);
    });

    conn.on('GameStateUpdated', (state: GameStateDto) => {
      useGameStore.getState().setGameState(state);
    });

    conn.on('GameOver', () => {
      // GameOver is also reflected via GameStarted/GameStateUpdated with isGameOver=true
    });

    conn.on('Error', (message: string) => {
      console.error('Server error:', message);
    });

    ensureConnected();

    return () => {
      conn.off('RoomUpdated');
      conn.off('GameStarted');
      conn.off('GameStateUpdated');
      conn.off('GameOver');
      conn.off('Error');
    };
  }, []);
}
