using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Othello.Contract;

namespace Othello.AI.TengfeiMa
{
    public class TengfeiMaAI : IOthelloAI
    {
        public string Name => "Tengfei Ma AI";

        
        private const int HowFarILook = 4;

        
        private static readonly int[,] MyPositionScore = new int[,]
        {
            { 100, -20, 10, 5, 5, 10, -20, 100 },
            { -20, -50, -2, -2, -2, -2, -50, -20 },
            { 10,  -2,  1,  1,  1,  1,  -2,  10 },
            { 5,   -2,  1,  0,  0,  1,  -2,   5 },
            { 5,   -2,  1,  0,  0,  1,  -2,   5 },
            { 10,  -2,  1,  1,  1,  1,  -2,  10 },
            { -20, -50, -2, -2, -2, -2, -50, -20 },
            { 100, -20, 10, 5, 5, 10, -20, 100 }
        };

        public async Task<Move?> GetMoveAsync(BoardState board, DiscColor yourColor, CancellationToken ct)
        {
            
            List<Move> possibleMoves = GetValidMoves(board, yourColor);
            if (possibleMoves.Count == 0) return null; 
            if (possibleMoves.Count == 1) return possibleMoves[0]; 

            Move? bestMove = null;
            int bestScore = -999999; 

            
            foreach (Move move in possibleMoves)
            {
                BoardState futureBoard = TryMove(board, move, yourColor);
               
                int moveScore = LookAhead(futureBoard, HowFarILook, -999999, 999999, OpponentOf(yourColor));
                moveScore = -moveScore; 

                if (moveScore > bestScore)
                {
                    bestScore = moveScore;
                    bestMove = move;
                }

                if (ct.IsCancellationRequested) break;
                await Task.Delay(1, ct); 
            }

            return bestMove;
        }

        
        private int LookAhead(BoardState board, int depth, int alpha, int beta, DiscColor player)
        {
            if (depth == 0 || GameIsOver(board))
            {
                return EvaluateBoard(board, player);
            }

            List<Move> moves = GetValidMoves(board, player);

            if (moves.Count == 0)
            {
                DiscColor next = OpponentOf(player);
                if (GetValidMoves(board, next).Count == 0) 
                {
                    return EvaluateBoard(board, player);
                }
                
                return -LookAhead(board, depth - 1, -beta, -alpha, next);
            }

            int bestVal = -999999;
            foreach (Move move in moves)
            {
                BoardState newBoard = TryMove(board, move, player);
                int val = -LookAhead(newBoard, depth - 1, -beta, -alpha, OpponentOf(player));

                bestVal = Math.Max(bestVal, val);
                alpha = Math.Max(alpha, val);
                if (alpha >= beta) break; 
            }
            return bestVal;
        }

        
        private int EvaluateBoard(BoardState board, DiscColor me)
        {
            DiscColor enemy = OpponentOf(me);
            int total = 0;

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (board.Grid[r, c] == me)
                    {
                        total += MyPositionScore[r, c];
                        
                        if ((r == 0 || r == 7) && (c == 0 || c == 7))
                            total += 200;
                    }
                    else if (board.Grid[r, c] == enemy)
                    {
                        total -= MyPositionScore[r, c];
                        if ((r == 0 || r == 7) && (c == 0 || c == 7))
                            total -= 200;
                    }
                }
            }

            
            int myMoves = GetValidMoves(board, me).Count;
            int enemyMoves = GetValidMoves(board, enemy).Count;
            total += (myMoves - enemyMoves) * 5;

            return total;
        }

        
        private BoardState TryMove(BoardState old, Move move, DiscColor color)
        {
            DiscColor[,] newGrid = new DiscColor[8, 8];
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    newGrid[r, c] = old.Grid[r, c];

            newGrid[move.Row, move.Column] = color;

            int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };
            DiscColor enemy = OpponentOf(color);

            for (int i = 0; i < 8; i++)
            {
                int r = move.Row + dr[i];
                int c = move.Column + dc[i];
                
                List<int> flipRows = new List<int>();
                List<int> flipCols = new List<int>();

                while (r >= 0 && r < 8 && c >= 0 && c < 8 && newGrid[r, c] == enemy)
                {
                    flipRows.Add(r);
                    flipCols.Add(c);
                    r += dr[i];
                    c += dc[i];
                }

                if (r >= 0 && r < 8 && c >= 0 && c < 8 && newGrid[r, c] == color)
                {
                    for (int j = 0; j < flipRows.Count; j++)
                    {
                        newGrid[flipRows[j], flipCols[j]] = color;
                    }
                }
            }

            var newBoard = new BoardState { Grid = newGrid };  
            return newBoard;
        }

        private bool GameIsOver(BoardState board)
        {
            int total = 0;
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (board.Grid[r, c] != DiscColor.None) total++;

            if (total == 64) return true;

            bool blackHasMove = GetValidMoves(board, DiscColor.Black).Count > 0;
            bool whiteHasMove = GetValidMoves(board, DiscColor.White).Count > 0;
            return !blackHasMove && !whiteHasMove;
        }

        private List<Move> GetValidMoves(BoardState board, DiscColor color)
        {
            List<Move> moves = new List<Move>();
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (IsLegal(board, new Move(r, c), color))
                        moves.Add(new Move(r, c));
            return moves;
        }

        private bool IsLegal(BoardState board, Move move, DiscColor color)
        {
            if (board.Grid[move.Row, move.Column] != DiscColor.None) return false;

            int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };
            DiscColor enemy = OpponentOf(color);

            for (int i = 0; i < 8; i++)
            {
                int r = move.Row + dr[i];
                int c = move.Column + dc[i];
                int cnt = 0;

                while (r >= 0 && r < 8 && c >= 0 && c < 8 && board.Grid[r, c] == enemy)
                {
                    r += dr[i];
                    c += dc[i];
                    cnt++;
                }

                if (r >= 0 && r < 8 && c >= 0 && c < 8 && board.Grid[r, c] == color && cnt > 0)
                    return true;
            }
            return false;
        }

        private DiscColor OpponentOf(DiscColor color)
        {
            return color == DiscColor.Black ? DiscColor.White : DiscColor.Black;
        }
    }
}