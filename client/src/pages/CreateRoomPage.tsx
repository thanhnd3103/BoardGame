import { useState } from 'react';
import { useNavigate } from 'react-router';
import { ensureConnected } from '../services/signalRService';
import { usePlayerStore } from '../store/playerStore';
import { useRoomStore } from '../store/roomStore';
import clsx from 'clsx';

const GAME_TYPES = [
  { id: 'TienLen', name: 'Tiến Lên Miền Nam', enabled: true },
  { id: 'Blackjack', name: 'Blackjack', enabled: false },
  { id: 'Poker', name: 'Poker', enabled: false },
];

export function CreateRoomPage() {
  const navigate = useNavigate();
  const [name, setName] = useState(usePlayerStore.getState().displayName);
  const [selectedGame, setSelectedGame] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const canCreate = name.trim() && selectedGame && !loading;

  async function handleCreate() {
    if (!canCreate) return;
    setLoading(true);
    setError('');

    try {
      const conn = await ensureConnected();
      const result = await conn.invoke('CreateRoom', name.trim(), selectedGame) as {
        roomCode: string;
        playerId: string;
        room: { roomCode: string; gameType: string; status: string; players: []; hostPlayerId: string; maxPlayers: number };
      };

      usePlayerStore.getState().setDisplayName(name.trim());
      usePlayerStore.getState().setPlayerId(result.playerId);
      useRoomStore.getState().setRoom(result.room);

      navigate(`/room/${result.roomCode}`);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create room');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4">
      <div className="w-full max-w-md">
        <h1 className="text-3xl font-bold text-white mb-8 text-center">Create Room</h1>

        <div className="mb-6">
          <label className="block text-slate-300 mb-2 text-sm font-medium">Your Name</label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Enter your display name"
            maxLength={20}
            className="w-full bg-slate-800 border border-slate-600 rounded-lg px-4 py-3 text-white placeholder-slate-500 focus:outline-none focus:border-indigo-500"
          />
        </div>

        <div className="mb-8">
          <label className="block text-slate-300 mb-3 text-sm font-medium">Choose Game</label>
          <div className="grid gap-3">
            {GAME_TYPES.map((game) => (
              <button
                key={game.id}
                disabled={!game.enabled}
                onClick={() => setSelectedGame(game.id)}
                className={clsx(
                  'p-4 rounded-xl border-2 text-left transition-all',
                  !game.enabled && 'opacity-40 cursor-not-allowed border-slate-700 bg-slate-800/50',
                  game.enabled && selectedGame === game.id && 'border-indigo-500 bg-indigo-500/10 text-white',
                  game.enabled && selectedGame !== game.id && 'border-slate-600 bg-slate-800 text-slate-300 hover:border-slate-500'
                )}
              >
                <div className="font-semibold">{game.name}</div>
                {!game.enabled && <div className="text-xs text-slate-500 mt-1">Coming Soon</div>}
              </button>
            ))}
          </div>
        </div>

        {error && <p className="text-red-400 text-sm mb-4 text-center">{error}</p>}

        <button
          onClick={handleCreate}
          disabled={!canCreate}
          className={clsx(
            'w-full py-3 rounded-xl font-semibold text-lg transition-colors',
            canCreate ? 'bg-indigo-600 hover:bg-indigo-500 text-white' : 'bg-slate-700 text-slate-500 cursor-not-allowed'
          )}
        >
          {loading ? 'Creating...' : 'Create Room'}
        </button>

        <button
          onClick={() => navigate('/')}
          className="w-full mt-3 py-2 text-slate-400 hover:text-white transition-colors"
        >
          Back
        </button>
      </div>
    </div>
  );
}
