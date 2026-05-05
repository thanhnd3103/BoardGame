import type { Card as CardType } from '../../types/card';
import { Card } from './Card';

interface PlayAreaProps {
  cards: CardType[];
}

export function PlayArea({ cards }: PlayAreaProps) {
  return (
    <div className="flex justify-center items-center min-h-28 bg-slate-800/50 rounded-2xl p-4">
      {cards.length === 0 ? (
        <p className="text-slate-500">No cards played yet</p>
      ) : (
        <div className="flex gap-2">
          {cards.map((card) => (
            <Card key={`${card.rank}-${card.suit}`} card={card} small />
          ))}
        </div>
      )}
    </div>
  );
}
