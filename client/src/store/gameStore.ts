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
  serverError: string | null;

  setGameState: (state: {
    hand: Card[];
    playArea: Card[];
    currentTurnPlayerId: string;
    players: GamePlayerInfo[];
    finishOrder: FinishEntry[];
    isGameOver: boolean;
  }) => void;
  setAutoPassNotification: (names: string[]) => void;
  setServerError: (message: string | null) => void;
  toggleCardSelection: (index: number) => void;
  clearSelection: () => void;
  clear: () => void;
}

let autoPassTimeout: ReturnType<typeof setTimeout> | null = null;
let serverErrorTimeout: ReturnType<typeof setTimeout> | null = null;

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
  serverError: null,

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
      serverError: null,
    }),

  setAutoPassNotification: (names) => {
    if (autoPassTimeout) clearTimeout(autoPassTimeout);
    set({ autoPassedNames: names });
    autoPassTimeout = setTimeout(() => {
      set({ autoPassedNames: [] });
      autoPassTimeout = null;
    }, 200000);
  },

  setServerError: (message) => {
    if (serverErrorTimeout) clearTimeout(serverErrorTimeout);
    set({ serverError: message });
    if (message !== null) {
      serverErrorTimeout = setTimeout(() => {
        set({ serverError: null });
        serverErrorTimeout = null;
      }, 5000);
    }
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
    if (serverErrorTimeout) { clearTimeout(serverErrorTimeout); serverErrorTimeout = null; }
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
      serverError: null,
    });
  },
}));
