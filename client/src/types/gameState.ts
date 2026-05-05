import type { Card } from './card';

export interface GamePlayerInfo {
  playerId: string;
  displayName: string;
  cardCount: number;
  hasPassed: boolean;
  hasFinished: boolean;
}

export interface FinishEntry {
  playerId: string;
  place: number;
}

export interface GameStateDto {
  hand: Card[];
  playArea: Card[];
  currentTurnPlayerId: string;
  players: GamePlayerInfo[];
  finishOrder: FinishEntry[];
  isGameOver: boolean;
}
