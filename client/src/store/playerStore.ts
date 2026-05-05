import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface PlayerState {
  playerId: string | null;
  displayName: string;
  setPlayerId: (id: string) => void;
  setDisplayName: (name: string) => void;
  clear: () => void;
}

export const usePlayerStore = create<PlayerState>()(
  persist(
    (set) => ({
      playerId: null,
      displayName: '',
      setPlayerId: (id) => set({ playerId: id }),
      setDisplayName: (name) => set({ displayName: name }),
      clear: () => set({ playerId: null, displayName: '' }),
    }),
    { name: 'player-store', partialize: (state) => ({ displayName: state.displayName, playerId: state.playerId }) }
  )
);
