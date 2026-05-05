import clsx from 'clsx';
import type { GamePlayerInfo } from '../../types/gameState';

interface PlayerSeatProps {
  player: GamePlayerInfo;
  isCurrentTurn: boolean;
  isSelf: boolean;
}

export function PlayerSeat({ player, isCurrentTurn, isSelf }: PlayerSeatProps) {
  return (
    <div
      className={clsx(
        'flex flex-col items-center gap-1 p-3 rounded-xl transition-all',
        isCurrentTurn && 'ring-2 ring-yellow-400 bg-yellow-400/10',
        player.hasFinished && 'opacity-50',
        isSelf && 'bg-indigo-500/10'
      )}
    >
      <div
        className={clsx(
          'w-10 h-10 rounded-full flex items-center justify-center text-white font-bold',
          player.hasFinished ? 'bg-green-600' : player.hasPassed ? 'bg-slate-600' : 'bg-indigo-600'
        )}
      >
        {player.displayName[0].toUpperCase()}
      </div>
      <span className="text-white text-sm font-medium truncate max-w-20">{player.displayName}</span>
      <span className="text-slate-400 text-xs">
        {player.hasFinished ? 'Finished' : player.hasPassed ? 'Passed' : `${player.cardCount} cards`}
      </span>
    </div>
  );
}
