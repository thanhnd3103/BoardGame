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
  autoPassedNames: string[];

  setGameState: (state: {
    hand: Card[];
    playArea: Card[];
    currentTurnPlayerId: string;
    players: GamePlayerInfo[];
    finishOrder: FinishEntry[];
    isGameOver: boolean;
  }) => void;
  setAutoPassNotification: (names: string[]) => void;
  toggleCardSelection: (index: number) => void;
  clearSelection: () => void;
  clear: () => void;
}

let autoPassTimeout: ReturnType<typeof setTimeout> | null = null;

export const useGameStore = create<GameState>((set) => ({
  hand: [],
  playArea: [],
  currentTurnPlayerId: null,
  players: [],
  finishOrder: [],
  isGameOver: false,
  selectedIndices: new Set(),
  gameActive: false,
  autoPassedNames: [],

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

  setAutoPassNotification: (names) => {
    if (autoPassTimeout) clearTimeout(autoPassTimeout);
    set({ autoPassedNames: names });
    autoPassTimeout = setTimeout(() => {
      set({ autoPassedNames: [] });
      autoPassTimeout = null;
    }, 200000);
  },

  toggleCardSelection: (index) =>
    set((prev) => {
      const next = new Set(prev.selectedIndices);
      if (next.has(index)) next.delete(index);
      else next.add(index);
      return { selectedIndices: next };
    }),

  clearSelection: () => set({ selectedIndices: new Set() }),

  clear: () => {
    if (autoPassTimeout) { clearTimeout(autoPassTimeout); autoPassTimeout = null; }
    set({
      hand: [],
      playArea: [],
      currentTurnPlayerId: null,
      players: [],
      finishOrder: [],
      isGameOver: false,
      selectedIndices: new Set(),
      gameActive: false,
      autoPassedNames: [],
    });
  },
}));
