export type Suit = 'Spades' | 'Clubs' | 'Diamonds' | 'Hearts';
export type Rank =
  | 'Three' | 'Four' | 'Five' | 'Six' | 'Seven' | 'Eight' | 'Nine'
  | 'Ten' | 'Jack' | 'Queen' | 'King' | 'Ace' | 'Two';

export interface Card {
  rank: Rank;
  suit: Suit;
}

export const RANK_ORDER: Rank[] = [
  'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine',
  'Ten', 'Jack', 'Queen', 'King', 'Ace', 'Two',
];

export const SUIT_ORDER: Suit[] = ['Spades', 'Clubs', 'Diamonds', 'Hearts'];

export const RANK_DISPLAY: Record<Rank, string> = {
  Three: '3', Four: '4', Five: '5', Six: '6', Seven: '7',
  Eight: '8', Nine: '9', Ten: '10', Jack: 'J', Queen: 'Q',
  King: 'K', Ace: 'A', Two: '2',
};

export const SUIT_SYMBOL: Record<Suit, string> = {
  Spades: '♠', Clubs: '♣', Diamonds: '♦', Hearts: '♥',
};

export function cardSortValue(card: Card): number {
  return RANK_ORDER.indexOf(card.rank) * 4 + SUIT_ORDER.indexOf(card.suit);
}

export function isRedSuit(suit: Suit): boolean {
  return suit === 'Hearts' || suit === 'Diamonds';
}
