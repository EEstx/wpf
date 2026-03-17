using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TicTacToeMVVM
{
    // 1. Главная логика игры (ViewModel)
    public class MainViewModel : INotifyPropertyChanged
    {
        // Список из 9 клеток игрового поля
        public ObservableCollection<CellVM> Board { get; set; }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _isXTurn = true;
        private bool _gameOver = false;

        public ICommand CellClickCommand { get; }
        public ICommand RestartCommand { get; }

        public MainViewModel()
        {
            // Инициализируем 9 пустых клеток
            Board = new ObservableCollection<CellVM>();
            for (int i = 0; i < 9; i++) Board.Add(new CellVM());

            CellClickCommand = new RelayCommand(ExecuteCellClick, CanExecuteCellClick);
            RestartCommand = new RelayCommand(ExecuteRestart);

            UpdateStatus();
        }

        private bool CanExecuteCellClick(object parameter)
        {
            // Кнопку можно нажать, если игра не окончена и клетка пустая
            if (_gameOver) return false;
            if (parameter is CellVM cell) return string.IsNullOrEmpty(cell.Content);
            return false;
        }

        private void ExecuteCellClick(object parameter)
        {
            if (parameter is CellVM cell && string.IsNullOrEmpty(cell.Content) && !_gameOver)
            {
                cell.Content = _isXTurn ? "X" : "O";
                _isXTurn = !_isXTurn;
                CheckWin();
            }
        }

        private void ExecuteRestart(object obj)
        {
            foreach (var cell in Board) cell.Content = "";
            _isXTurn = true;
            _gameOver = false;
            UpdateStatus();
        }

        private void CheckWin()
        {
            // Индексы выигрышных комбинаций
            int[][] winLines = new int[][]
            {
                new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, // Горизонтали
                new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, // Вертикали
                new[] {0, 4, 8}, new[] {2, 4, 6}                   // Диагонали
            };

            foreach (var line in winLines)
            {
                var a = Board[line[0]].Content;
                var b = Board[line[1]].Content;
                var c = Board[line[2]].Content;

                if (!string.IsNullOrEmpty(a) && a == b && b == c)
                {
                    StatusMessage = $"Победитель: {a}!";
                    _gameOver = true;
                    return;
                }
            }

            if (Board.All(c => !string.IsNullOrEmpty(c.Content)))
            {
                StatusMessage = "Ничья!";
                _gameOver = true;
            }
            else
            {
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            if (!_gameOver) StatusMessage = $"Ход игрока: {(_isXTurn ? "X" : "O")}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // 2. Модель отдельной клетки (View-Model клетки)
    public class CellVM : INotifyPropertyChanged
    {
        private string _content = "";
        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // 3. Класс для обработки нажатий кнопок (Команды MVVM)
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }
}