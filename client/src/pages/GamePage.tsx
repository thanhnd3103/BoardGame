import { useNavigate } from 'react-router';
import { useSignalR } from '../hooks/useSignalR';
import { useGameStore } from '../store/gameStore';
import { usePlayerStore } from '../store/playerStore';
import { useRoomStore } from '../store/roomStore';
import { ensureConnected } from '../services/signalRService';
import { CardHand } from '../components/game/CardHand';
import { PlayArea } from '../components/game/PlayArea';
import { PlayerSeat } from '../components/game/PlayerSeat';
import { useState } from 'react';

export function GamePage() {
  useSignalR();
  const navigate = useNavigate();
  const playerId = usePlayerStore((s) => s.playerId);
  const roomCode = useRoomStore((s) => s.roomCode);
  const hostPlayerId = useRoomStore((s) => s.hostPlayerId);
  const {
    hand, playArea, currentTurnPlayerId, players,
    finishOrder, isGameOver, selectedIndices,
    toggleCardSelection, clearSelection,
  } = useGameStore();
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const isMyTurn = currentTurnPlayerId === playerId;
  const myPlayer = players.find((p) => p.playerId === playerId);
  const otherPlayers = players.filter((p) => p.playerId !== playerId);
  const hasFinished = myPlayer?.hasFinished ?? false;

  async function handlePlay() {
    if (selectedIndices.size === 0) return;
    setLoading(true);
    setError('');

    try {
      const conn = await ensureConnected();
      const selectedCards = Array.from(selectedIndices).map((i) => hand[i]);
      await conn.invoke('PlayCards', selectedCards);
      clearSelection();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to play cards');
    } finally {
      setLoading(false);
    }
  }

  async function handlePass() {
    setLoading(true);
    setError('');

    try {
      const conn = await ensureConnected();
      await conn.invoke('Pass');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to pass');
    } finally {
      setLoading(false);
    }
  }

  async function handleBackToLobby() {
    useGameStore.getState().clear();
    navigate(`/room/${roomCode}`);
  }

  async function handlePlayAgain() {
    setLoading(true);
    try {
      const conn = await ensureConnected();
      await conn.invoke('StartGame');
    } catch (e) {
      console.error('Failed to restart:', e);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex flex-col min-h-screen p-4">
      <div className="flex justify-between items-center mb-4">
        <div className="text-slate-400 text-sm">Room: <span className="text-white font-mono">{roomCode}</span></div>
        <div className="text-slate-400 text-sm">
          {isGameOver ? (
            <span className="text-yellow-400 font-semibold">Game Over!</span>
          ) : isMyTurn ? (
            <span className="text-green-400 font-semibold">Your Turn</span>
          ) : (
            <span>Waiting for other player...</span>
          )}
        </div>
      </div>

      <div className="flex justify-center gap-4 mb-6">
        {otherPlayers.map((p) => (
          <PlayerSeat
            key={p.playerId}
            player={p}
            isCurrentTurn={p.playerId === currentTurnPlayerId}
            isSelf={false}
          />
        ))}
      </div>

      <div className="flex-1 flex flex-col justify-center mb-6">
        <PlayArea cards={playArea} />
      </div>

      {isGameOver && (
        <div className="bg-slate-800 rounded-xl p-6 mb-6 text-center">
          <h2 className="text-2xl font-bold text-white mb-4">Results</h2>
          <div className="space-y-2">
            {finishOrder.map((entry) => {
              const player = players.find((p) => p.playerId === entry.playerId);
              return (
                <div key={entry.playerId} className="flex justify-between px-8 py-2 bg-slate-700/50 rounded-lg">
                  <span className="text-yellow-400 font-bold">#{entry.place}</span>
                  <span className="text-white">{player?.displayName ?? 'Unknown'}</span>
                </div>
              );
            })}
          </div>
          <div className="flex gap-3 mt-4 justify-center">
            {playerId === hostPlayerId && (
              <button
                onClick={handlePlayAgain}
                disabled={loading}
                className="bg-green-600 hover:bg-green-500 text-white font-semibold py-2 px-6 rounded-xl transition-colors"
              >
                Play Again
              </button>
            )}
            <button
              onClick={handleBackToLobby}
              className="bg-slate-700 hover:bg-slate-600 text-white font-semibold py-2 px-6 rounded-xl transition-colors"
            >
              Back to Lobby
            </button>
          </div>
        </div>
      )}

      {error && <p className="text-red-400 text-sm text-center mb-2">{error}</p>}

      {!hasFinished && !isGameOver && (
        <>
          <div className="flex justify-center gap-3 mb-4">
            <button
              onClick={handlePlay}
              disabled={!isMyTurn || selectedIndices.size === 0 || loading}
              className={`py-2 px-8 rounded-xl font-semibold transition-colors ${
                isMyTurn && selectedIndices.size > 0
                  ? 'bg-indigo-600 hover:bg-indigo-500 text-white'
                  : 'bg-slate-700 text-slate-500 cursor-not-allowed'
              }`}
            >
              Play
            </button>
            <button
              onClick={handlePass}
              disabled={!isMyTurn || loading}
              className={`py-2 px-8 rounded-xl font-semibold transition-colors ${
                isMyTurn
                  ? 'bg-slate-600 hover:bg-slate-500 text-white'
                  : 'bg-slate-700 text-slate-500 cursor-not-allowed'
              }`}
            >
              Pass
            </button>
          </div>

          <CardHand
            cards={hand}
            selectedIndices={selectedIndices}
            onToggle={toggleCardSelection}
            disabled={!isMyTurn}
          />
        </>
      )}
    </div>
  );
}
