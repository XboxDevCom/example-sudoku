using System;
using Sudoku.Models;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Sudoku
{
    /// <summary>
    /// Hauptseite des Sudoku-Spiels mit Gamepad-Navigation.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int BoardSize = 9;
        private const int CellSize = 56;

        private readonly SudokuGenerator _generator;
        private int?[,] _board;
        private bool[,] _isGiven;
        private Button[,] _cells;
        private int _selectedRow;
        private int _selectedCol;

        /// <summary>
        /// Initialisiert eine neue Instanz der MainPage und erstellt das Spielbrett.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            _generator = new SudokuGenerator();
            _cells = new Button[BoardSize, BoardSize];
            _selectedRow = 0;
            _selectedCol = 0;

            CreateBoard();
            CreateNumberPad();

            NewGame();
        }

        /// <summary>
        /// Erstellt das 9x9-Spielbrett programmatisch mit Gamepad-fähigen Buttons.
        /// </summary>
        private void CreateBoard()
        {
            BoardContainer.RowDefinitions.Clear();
            BoardContainer.ColumnDefinitions.Clear();

            for (int i = 0; i < BoardSize; i++)
            {
                BoardContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize) });
                BoardContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize) });
            }

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    Button cell = new Button
                    {
                        FontSize = 24,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(0),
                        Margin = GetCellMargin(row, col),
                        IsTabStop = true,
                        Tag = new Tuple<int, int>(row, col)
                    };

                    cell.Click += Cell_Click;
                    cell.KeyDown += Cell_KeyDown;

                    _cells[row, col] = cell;

                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    BoardContainer.Children.Add(cell);
                }
            }
        }

        /// <summary>
        /// Bestimmt die Randstärke einer Zelle zur optischen Trennung der 3x3-Boxen.
        /// </summary>
        /// <param name="row">Die Zeile der Zelle.</param>
        /// <param name="col">Die Spalte der Zelle.</param>
        /// <returns>Die Randstärke für die Zelle.</returns>
        private Thickness GetCellMargin(int row, int col)
        {
            double left = col % 3 == 0 ? 2 : 0.5;
            double top = row % 3 == 0 ? 2 : 0.5;
            double right = col % 3 == 2 ? 2 : 0.5;
            double bottom = row % 3 == 2 ? 2 : 0.5;

            return new Thickness(left, top, right, bottom);
        }

        /// <summary>
        /// Erstellt das Zahlenpad mit den Buttons 1 bis 9 und einer Löschen-Taste.
        /// </summary>
        private void CreateNumberPad()
        {
            Button previous = null;

            for (int i = 1; i <= 9; i++)
            {
                Button numberButton = new Button
                {
                    Content = i.ToString(),
                    FontSize = 24,
                    Width = 56,
                    Height = 56,
                    Margin = new Thickness(4, 0, 4, 0),
                    Tag = i
                };

                numberButton.Click += NumberButton_Click;

                if (previous != null)
                {
                    previous.XYFocusRight = numberButton;
                    numberButton.XYFocusLeft = previous;
                }

                NumberPad.Children.Add(numberButton);
                previous = numberButton;
            }

            Button clearButton = new Button
            {
                Content = "Löschen",
                FontSize = 18,
                Height = 56,
                Padding = new Thickness(16, 0, 16, 0),
                Margin = new Thickness(8, 0, 0, 0),
                Tag = 0
            };

            clearButton.Click += NumberButton_Click;

            if (previous != null)
            {
                previous.XYFocusRight = clearButton;
                clearButton.XYFocusLeft = previous;
            }

            NumberPad.Children.Add(clearButton);
        }

        /// <summary>
        /// Startet ein neues Spiel mit einem frisch generierten Rätsel.
        /// </summary>
        private void NewGame()
        {
            _board = _generator.CreatePuzzle(45);
            _isGiven = new bool[BoardSize, BoardSize];

            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    _isGiven[r, c] = _board[r, c].HasValue;
                }
            }

            UpdateBoardDisplay();
            SelectCell(0, 0);
            StatusText.Text = "Neues Spiel gestartet";
        }

        /// <summary>
        /// Aktualisiert die Anzeige aller Zellen auf dem Spielbrett.
        /// </summary>
        private void UpdateBoardDisplay()
        {
            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    Button cell = _cells[r, c];
                    cell.ClearValue(Button.BackgroundProperty);

                    if (_board[r, c].HasValue)
                    {
                        cell.Content = _board[r, c].Value.ToString();

                        if (_isGiven[r, c])
                        {
                            cell.FontWeight = Windows.UI.Text.FontWeights.Bold;
                            cell.Foreground = new SolidColorBrush(Colors.LightGray);
                        }
                        else
                        {
                            cell.FontWeight = Windows.UI.Text.FontWeights.Normal;
                            cell.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        cell.Content = null;
                        cell.FontWeight = Windows.UI.Text.FontWeights.Normal;
                        cell.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }
        }

        /// <summary>
        /// Wählt die Zelle an der angegebenen Position aus und hebt sie hervor.
        /// </summary>
        /// <param name="row">Die Zeile der auszuwählenden Zelle.</param>
        /// <param name="col">Die Spalte der auszuwählenden Zelle.</param>
        private void SelectCell(int row, int col)
        {
            _cells[_selectedRow, _selectedCol].ClearValue(Button.BackgroundProperty);

            _selectedRow = row;
            _selectedCol = col;

            Button selected = _cells[row, col];
            selected.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
            selected.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Behandelt das Klicken auf eine Zelle des Spielbretts.
        /// </summary>
        /// <param name="sender">Die geklickte Zelle.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Tuple<int, int> pos)
            {
                SelectCell(pos.Item1, pos.Item2);
            }
        }

        /// <summary>
        /// Behandelt die Tastatur- und Gamepad-Navigation zwischen den Zellen.
        /// </summary>
        /// <param name="sender">Die Zelle, die das Ereignis ausgelöst hat.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void Cell_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            int newRow = _selectedRow;
            int newCol = _selectedCol;

            switch (e.OriginalKey)
            {
                case Windows.System.VirtualKey.Up:
                    newRow = Math.Max(0, _selectedRow - 1);
                    break;
                case Windows.System.VirtualKey.Down:
                    newRow = Math.Min(BoardSize - 1, _selectedRow + 1);
                    break;
                case Windows.System.VirtualKey.Left:
                    newCol = Math.Max(0, _selectedCol - 1);
                    break;
                case Windows.System.VirtualKey.Right:
                    newCol = Math.Min(BoardSize - 1, _selectedCol + 1);
                    break;
                case Windows.System.VirtualKey.GamepadA:
                    HandleNumberInput(0);
                    return;
                default:
                    return;
            }

            e.Handled = true;
            SelectCell(newRow, newCol);
        }

        /// <summary>
        /// Behandelt das Klicken auf eine Zahlentaste des Zahlenpads.
        /// </summary>
        /// <param name="sender">Die geklickte Taste.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int value)
            {
                HandleNumberInput(value);
            }
        }

        /// <summary>
        /// Verarbeitet die Eingabe einer Zahl für die aktuell ausgewählte Zelle.
        /// </summary>
        /// <param name="value">Die eingegebene Zahl (0 zum Löschen).</param>
        private void HandleNumberInput(int value)
        {
            if (_isGiven[_selectedRow, _selectedCol])
            {
                StatusText.Text = "Diese Zelle kann nicht geändert werden";
                return;
            }

            if (value == 0)
            {
                _board[_selectedRow, _selectedCol] = null;
            }
            else
            {
                _board[_selectedRow, _selectedCol] = value;
            }

            UpdateBoardDisplay();
            SelectCell(_selectedRow, _selectedCol);

            if (_generator.IsBoardComplete(_board))
            {
                StatusText.Text = "Glückwunsch! Das Sudoku ist gelöst!";
            }
            else
            {
                StatusText.Text = value == 0 ? "Zelle geleert" : "Zahl eingetragen";
            }
        }

        /// <summary>
        /// Behandelt das Klicken auf den "Neues Spiel"-Button.
        /// </summary>
        /// <param name="sender">Der geklickte Button.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        /// <summary>
        /// Behandelt das Klicken auf den "Prüfen"-Button und markiert ungültige Zellen.
        /// </summary>
        /// <param name="sender">Der geklickte Button.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateBoardDisplay();

            int errors = 0;

            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    if (!_board[r, c].HasValue)
                    {
                        continue;
                    }

                    int value = _board[r, c].Value;
                    _board[r, c] = null;

                    bool valid = _generator.IsValidMove(_board, r, c, value);
                    _board[r, c] = value;

                    if (!valid)
                    {
                        _cells[r, c].Background = new SolidColorBrush(Color.FromArgb(180, 200, 40, 40));
                        errors++;
                    }
                }
            }

            SelectCell(_selectedRow, _selectedCol);

            if (errors == 0)
            {
                StatusText.Text = "Keine Fehler gefunden";
            }
            else
            {
                StatusText.Text = errors + " Fehler gefunden";
            }
        }
    }
}
