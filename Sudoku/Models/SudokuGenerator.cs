using System;
using System.Collections.Generic;

namespace Sudoku.Models
{
    /// <summary>
    /// Generiert und validiert Sudoku-Rätsel.
    /// </summary>
    public sealed class SudokuGenerator
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Generiert ein vollständiges gültiges Sudoku-Board mittels Backtracking.
        /// </summary>
        /// <returns>Ein 9x9-Board gefüllt mit Zahlen von 1 bis 9.</returns>
        public int[,] GenerateCompleteBoard()
        {
            int[,] board = new int[9, 9];
            FillBoard(board, 0, 0);
            return board;
        }

        /// <summary>
        /// Füllt das Board rekursiv mit gültigen Werten.
        /// </summary>
        /// <param name="board">Das zu füllende Board.</param>
        /// <param name="row">Die aktuelle Zeile.</param>
        /// <param name="col">Die aktuelle Spalte.</param>
        /// <returns>True, wenn das Board erfolgreich gefüllt wurde.</returns>
        private bool FillBoard(int[,] board, int row, int col)
        {
            if (col >= 9)
            {
                row++;
                col = 0;
            }

            if (row >= 9)
            {
                return true;
            }

            List<int> numbers = GetShuffledNumbers();

            foreach (int num in numbers)
            {
                if (IsValidPlacement(board, row, col, num))
                {
                    board[row, col] = num;

                    if (FillBoard(board, row, col + 1))
                    {
                        return true;
                    }

                    board[row, col] = 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Gibt eine gemischte Liste der Zahlen 1 bis 9 zurück.
        /// </summary>
        /// <returns>Eine gemischte Liste von Zahlen.</returns>
        private List<int> GetShuffledNumbers()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int i = numbers.Count - 1; i > 0; i--)
            {
                int j = Random.Next(i + 1);
                int temp = numbers[i];
                numbers[i] = numbers[j];
                numbers[j] = temp;
            }

            return numbers;
        }

        /// <summary>
        /// Erstellt ein Rätsel durch Entfernen von Zellen aus einem vollständigen Board.
        /// </summary>
        /// <param name="difficulty">Die Anzahl der zu entfernenden Zellen.</param>
        /// <returns>Ein Rätsel-Board mit null für leere Zellen.</returns>
        public int?[,] CreatePuzzle(int difficulty)
        {
            int[,] complete = GenerateCompleteBoard();
            int?[,] puzzle = new int?[9, 9];

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    puzzle[r, c] = complete[r, c];
                }
            }

            int removed = 0;
            while (removed < difficulty)
            {
                int row = Random.Next(9);
                int col = Random.Next(9);

                if (puzzle[row, col].HasValue)
                {
                    puzzle[row, col] = null;
                    removed++;
                }
            }

            return puzzle;
        }

        /// <summary>
        /// Validiert, ob ein Zug an der angegebenen Position legal ist.
        /// </summary>
        /// <param name="board">Das aktuelle Board.</param>
        /// <param name="row">Die Zeile der Zelle.</param>
        /// <param name="col">Die Spalte der Zelle.</param>
        /// <param name="value">Der zu prüfende Wert.</param>
        /// <returns>True, wenn der Zug legal ist.</returns>
        public bool IsValidMove(int?[,] board, int row, int col, int value)
        {
            for (int c = 0; c < 9; c++)
            {
                if (c != col && board[row, c].HasValue && board[row, c].Value == value)
                {
                    return false;
                }
            }

            for (int r = 0; r < 9; r++)
            {
                if (r != row && board[r, col].HasValue && board[r, col].Value == value)
                {
                    return false;
                }
            }

            int boxRow = (row / 3) * 3;
            int boxCol = (col / 3) * 3;

            for (int r = boxRow; r < boxRow + 3; r++)
            {
                for (int c = boxCol; c < boxCol + 3; c++)
                {
                    if ((r != row || c != col) && board[r, c].HasValue && board[r, c].Value == value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Prüft, ob das Board vollständig und gültig ist.
        /// </summary>
        /// <param name="board">Das zu prüfende Board.</param>
        /// <returns>True, wenn das Board vollständig und gültig ist.</returns>
        public bool IsBoardComplete(int?[,] board)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (!board[r, c].HasValue)
                    {
                        return false;
                    }

                    int value = board[r, c].Value;

                    if (!IsValidMove(board, r, c, value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
