import { create } from 'zustand';
import type { Card } from '../types/card';
import type { FinishEntry, GamePlayerInfo } from '../types/gameState';

interface GameState {
  hand: Card[];
  playArea: Card[];
  currentTurnPlayerId: string | null;
  players: GamePlayerInfo[];
  finishOrder: FinishEntry[];
  isGameOver: boolean;
  selectedIndices: Set<number>;
  gameActive: boolean;

  setGameState: (state: {
    hand: Card[];
    playArea: Card[];
    currentTurnPlayerId: string;
    players: GamePlayerInfo[];
    finishOrder: FinishEntry[];
    isGameOver: boolean;
  }) => void;
  toggleCardSelection: (index: number) => void;
  clearSelection: () => void;
  clear: () => void;
}

export const useGameStore = create<GameState>((set) => ({
  hand: [],
  playArea: [],
  currentTurnPlayerId: null,
  players: [],
  finishOrder: [],
  isGameOver: false,
  selectedIndices: new Set(),
  gameActive: false,

  setGameState: (state) =>
    set({
      hand: state.hand,
      playArea: state.playArea,
      currentTurnPlayerId: state.currentTurnPlayerId,
      players: state.players,
      finishOrder: state.finishOrder,
      isGameOver: state.isGameOver,
      gameActive: true,
      selectedIndices: new Set(),
    }),

  toggleCardSelection: (index) =>
    set((prev) => {
      const next = new Set(prev.selectedIndices);
      if (next.has(index)) next.delete(index);
      else next.add(index);
      return { selectedIndices: next };
    }),

  clearSelection: () => set({ selectedIndices: new Set() }),

  clear: () =>
    set({
      hand: [],
      playArea: [],
      currentTurnPlayerId: null,
      players: [],
      finishOrder: [],
      isGameOver: false,
      selectedIndices: new Set(),
      gameActive: false,
    }),
}));
