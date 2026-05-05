import type { Card as CardType } from '../../types/card';
import { cardSortValue } from '../../types/card';
import { Card } from './Card';

interface CardHandProps {
  cards: CardType[];
  selectedIndices: Set<number>;
  onToggle: (index: number) => void;
  disabled: boolean;
}

export function CardHand({ cards, selectedIndices, onToggle, disabled }: CardHandProps) {
  const sorted = cards
    .map((card, originalIndex) => ({ card, originalIndex, sortVal: cardSortValue(card) }))
    .sort((a, b) => a.sortVal - b.sortVal);

  return (
    <div className="flex justify-center items-end flex-wrap gap-1">
      {sorted.map(({ card, originalIndex }) => (
        <Card
          key={`${card.rank}-${card.suit}`}
          card={card}
          selected={selectedIndices.has(originalIndex)}
          onClick={disabled ? undefined : () => onToggle(originalIndex)}
        />
      ))}
    </div>
  );
}
