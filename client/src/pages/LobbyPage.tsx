import { useState } from 'react';
import { useNavigate } from 'react-router';
import { useSignalR } from '../hooks/useSignalR';
import { useRoomStore } from '../store/roomStore';
import { usePlayerStore } from '../store/playerStore';
import { useGameStore } from '../store/gameStore';
import { ensureConnected } from '../services/signalRService';

export function LobbyPage() {
  useSignalR();
  const navigate = useNavigate();
  const { roomCode, players, hostPlayerId, gameType } = useRoomStore();
  const playerId = usePlayerStore((s) => s.playerId);
  const gameActive = useGameStore((s) => s.gameActive);
  const [loading, setLoading] = useState(false);
  const [copied, setCopied] = useState(false);

  const isHost = playerId === hostPlayerId;
  const canStart = isHost && players.length >= 2 && !loading;

  if (gameActive) {
    navigate(`/game/${roomCode}`);
  }

  async function handleStart() {
    setLoading(true);
    try {
      const conn = await ensureConnected();
      await conn.invoke('StartGame');
      navigate(`/game/${roomCode}`);
    } catch (e) {
      console.error('Failed to start game:', e);
    } finally {
      setLoading(false);
    }
  }

  async function handleLeave() {
    try {
      const conn = await ensureConnected();
      await conn.invoke('LeaveRoom');
      useRoomStore.getState().clear();
      navigate('/');
    } catch {
      navigate('/');
    }
  }

  function handleCopy() {
    if (roomCode) {
      navigator.clipboard.writeText(roomCode);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4">
      <div className="w-full max-w-md">
        <p className="text-slate-400 text-sm mb-2 text-center">{gameType ?? 'Card Game'}</p>
        <h1 className="text-3xl font-bold text-white mb-8 text-center">Room Lobby</h1>

        <div className="bg-slate-800 rounded-xl p-6 mb-6 text-center">
          <p className="text-slate-400 text-sm mb-2">Room Code</p>
          <button
            onClick={handleCopy}
            className="text-4xl font-mono font-bold text-indigo-400 tracking-[0.3em] hover:text-indigo-300 transition-colors"
          >
            {roomCode}
          </button>
          <p className="text-slate-500 text-xs mt-2">
            {copied ? 'Copied!' : 'Click to copy'}
          </p>
        </div>

        <div className="bg-slate-800 rounded-xl p-4 mb-6">
          <p className="text-slate-400 text-sm mb-3">Players ({players.length}/4)</p>
          <div className="space-y-2">
            {players.map((p) => (
              <div
                key={p.playerId}
                className="flex items-center gap-3 bg-slate-700/50 rounded-lg px-4 py-3"
              >
                <div className="w-8 h-8 bg-indigo-600 rounded-full flex items-center justify-center text-white text-sm font-bold">
                  {p.displayName[0].toUpperCase()}
                </div>
                <span className="text-white font-medium flex-1">{p.displayName}</span>
                {p.playerId === hostPlayerId && (
                  <span className="text-yellow-400 text-xs font-semibold">HOST</span>
                )}
                {!p.isConnected && (
                  <span className="text-red-400 text-xs">Disconnected</span>
                )}
              </div>
            ))}
          </div>
        </div>

        {isHost && (
          <button
            onClick={handleStart}
            disabled={!canStart}
            className={`w-full py-3 rounded-xl font-semibold text-lg transition-colors mb-3 ${
              canStart
                ? 'bg-green-600 hover:bg-green-500 text-white'
                : 'bg-slate-700 text-slate-500 cursor-not-allowed'
            }`}
          >
            {loading ? 'Starting...' : players.length < 2 ? 'Waiting for players...' : 'Start Game'}
          </button>
        )}

        {!isHost && (
          <p className="text-slate-400 text-center mb-3">Waiting for host to start the game...</p>
        )}

        <button
          onClick={handleLeave}
          className="w-full py-2 text-slate-400 hover:text-red-400 transition-colors"
        >
          Leave Room
        </button>
      </div>
    </div>
  );
}
