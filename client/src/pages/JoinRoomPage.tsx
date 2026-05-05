import { useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { ensureConnected } from '../services/signalRService';
import { usePlayerStore } from '../store/playerStore';
import { useRoomStore } from '../store/roomStore';

export function JoinRoomPage() {
  const navigate = useNavigate();
  const { roomCode: paramCode } = useParams();
  const [name, setName] = useState(usePlayerStore.getState().displayName);
  const [roomCode, setRoomCode] = useState(paramCode ?? '');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const canJoin = name.trim() && roomCode.trim() && !loading;

  async function handleJoin() {
    if (!canJoin) return;
    setLoading(true);
    setError('');

    try {
      const conn = await ensureConnected();
      const result = await conn.invoke('JoinRoom', roomCode.trim().toUpperCase(), name.trim()) as {
        playerId: string;
        room: { roomCode: string; gameType: string; status: string; players: []; hostPlayerId: string; maxPlayers: number };
      };

      usePlayerStore.getState().setDisplayName(name.trim());
      usePlayerStore.getState().setPlayerId(result.playerId);
      useRoomStore.getState().setRoom(result.room);

      navigate(`/room/${result.room.roomCode}`);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to join room');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4">
      <div className="w-full max-w-md">
        <h1 className="text-3xl font-bold text-white mb-8 text-center">Join Room</h1>

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
          <label className="block text-slate-300 mb-2 text-sm font-medium">Room Code</label>
          <input
            type="text"
            value={roomCode}
            onChange={(e) => setRoomCode(e.target.value.toUpperCase())}
            placeholder="Enter 6-character room code"
            maxLength={6}
            className="w-full bg-slate-800 border border-slate-600 rounded-lg px-4 py-3 text-white placeholder-slate-500 focus:outline-none focus:border-indigo-500 text-center text-2xl tracking-widest uppercase"
          />
        </div>

        {error && <p className="text-red-400 text-sm mb-4 text-center">{error}</p>}

        <button
          onClick={handleJoin}
          disabled={!canJoin}
          className={`w-full py-3 rounded-xl font-semibold text-lg transition-colors ${
            canJoin ? 'bg-indigo-600 hover:bg-indigo-500 text-white' : 'bg-slate-700 text-slate-500 cursor-not-allowed'
          }`}
        >
          {loading ? 'Joining...' : 'Join Room'}
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
