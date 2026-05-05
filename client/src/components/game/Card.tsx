import clsx from 'clsx';
import type { Card as CardType } from '../../types/card';
import { RANK_DISPLAY, SUIT_SYMBOL, isRedSuit } from '../../types/card';

interface CardProps {
  card: CardType;
  selected?: boolean;
  onClick?: () => void;
  small?: boolean;
}

export function Card({ card, selected, onClick, small }: CardProps) {
  const red = isRedSuit(card.suit);

  return (
    <div
      onClick={onClick}
      className={clsx(
        'relative bg-white rounded-lg border-2 flex flex-col items-center justify-center select-none',
        small ? 'w-12 h-16 text-sm' : 'w-16 h-22 text-base',
        selected ? 'border-indigo-500 -translate-y-4 shadow-lg shadow-indigo-500/30' : 'border-slate-300',
        onClick && 'cursor-pointer hover:-translate-y-1 transition-transform',
        red ? 'text-red-600' : 'text-slate-900'
      )}
    >
      <div className="absolute top-1 left-1.5 text-xs font-bold leading-none">
        <div>{RANK_DISPLAY[card.rank]}</div>
        <div>{SUIT_SYMBOL[card.suit]}</div>
      </div>
      <div className={clsx('font-bold', small ? 'text-lg' : 'text-2xl')}>
        {SUIT_SYMBOL[card.suit]}
      </div>
      <div className="absolute bottom-1 right-1.5 text-xs font-bold leading-none rotate-180">
        <div>{RANK_DISPLAY[card.rank]}</div>
        <div>{SUIT_SYMBOL[card.suit]}</div>
      </div>
    </div>
  );
}
