using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Minesweeper
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public int column_count, row_count, separator, bomb_count, button_dim;
        private int _currentBombCount;
        private readonly Point[] _points =
        {
            new Point(-1,-1), new Point(0,-1), new Point(1,-1),
            new Point(-1,0),                    new Point(1,0),
            new Point(-1,1), new Point(0,1), new Point(1,1)
        };
        private readonly Random _random = new Random();
        private readonly GameButton[,] _fields;
        private TextBlock _bombCountTB;
        private TextBlock _timerTB;

        private System.Timers.Timer _timer;
        private int _elapsedSeconds = 0;
        public MainWindow()
        {
            button_dim = 35;
            separator = 0;

            bomb_count = 40;
            _currentBombCount = bomb_count;

            column_count = 16;
            row_count = 16;
            _fields = new GameButton[column_count, row_count];

            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += UpdateElapsedTime;
            _timerTB = new TextBlock();
            _timerTB.Text = "Elapsed Time: 00:00";
            _timerTB.FontSize = 25;

            _bombCountTB = new TextBlock();
            InitializeComponent();
            StartNewGame();
        }

        private void StartNewGame()
        {
            canvas.Children.Clear();
            _currentBombCount = bomb_count;
            _timer.Start();
            InitializeButtons();
            InitializeBombCount();
            InitializeTimer();
            ResetBombs();
        }
        private void InitializeButtons()
        {
            for (int i = 0; i < column_count; i++)
            {
                for (int j = 0; j < row_count; j++)
                {
                    _fields[i, j] = new GameButton(button_dim, button_dim);
                    _fields[i, j].Click += HandleButtonClick;
                    _fields[i, j].MouseRightButtonUp += Field_RightClick;
                    Canvas.SetLeft(_fields[i, j], i * (button_dim + separator));
                    Canvas.SetTop(_fields[i, j], j * (button_dim + separator));
                    canvas.Children.Add(_fields[i, j]);
                }
            }
            InitializeNeighbors();
        }
        private void InitializeBombCount()
        {
            Canvas.SetLeft(_bombCountTB, 560);
            Canvas.SetTop(_bombCountTB, 100);
            _bombCountTB.Text = $"Current Bombs: {_currentBombCount}";
            _bombCountTB.FontSize = 35;
            canvas.Children.Add(_bombCountTB);
        }
        private void InitializeTimer()
        {
            _elapsedSeconds = 0;
            Canvas.SetBottom(_timerTB, 10);
            Canvas.SetRight(_timerTB, 10);
            _timerTB.Text = "Elapsed Time: 00:00";
            _timerTB.FontSize = 25;
            canvas.Children.Add(_timerTB);
        }
        private void SetBombs()
        {
            int tempBombCount = bomb_count, randX = _random.Next(column_count), randY = _random.Next(row_count);
            while (tempBombCount>0)
            {
                if (_fields[randX, randY].State == ButtonState.Free)
                {
                    _fields[randX, randY].State = ButtonState.Mine;
                    tempBombCount--;
                }
                randX = _random.Next(column_count);
                randY = _random.Next(row_count);
            }
        }
        private void ResetBombs()
        {
            foreach (GameButton field in _fields)
            {
                field.State = ButtonState.Free;
            }
            SetBombs();
            foreach (GameButton field in _fields)
            {
                field.SetNumberState();
            }
        }
        public void HandleButtonClick(object sender, RoutedEventArgs e)
        {
            GameButton senderButton = (GameButton)sender;
            if(senderButton.RightClickState != RightClickState.Nothing)
            {
                return;
            }
            senderButton.IsEnabled = false;
            UpdateButton(senderButton);
            ManageGameOver(senderButton);
        }
        private void Field_RightClick(object sender, MouseButtonEventArgs e)
        {
            _currentBombCount = bomb_count - _fields.Cast<GameButton>().Count(button => button.RightClickState == RightClickState.Flag);
            _bombCountTB.Text = $"Current Bombs: {_currentBombCount}";
        }
        private void InitializeNeighbors()
        {
            for (int i = 0; i < column_count; i++)
            {
                for (int j = 0; j < row_count; j++)
                {
                    List<GameButton> neighbors = (from p in _points
                                                  where IsOnField(new Point(p.X + i, p.Y + j))
                                                  select _fields[i + (int)p.X, j + (int)p.Y]).ToList();
                    _fields[i, j].FillNeighbors(neighbors);
                }
            }
        }
        private bool IsOnField(Point point)
        {
            return point.X >= 0 && point.X < column_count && point.Y >= 0 && point.Y < row_count;
        }
        private void UpdateButton(GameButton sender, bool doRecursion = true)
        {
            switch (sender.State)
            {
                case ButtonState.Free:
                    {
                        if (!doRecursion)
                        {
                            return;
                        }
                        foreach(GameButton button in sender.Neighbors.Where(button => button.IsEnabled))
                        {
                            HandleButtonClick(button, null);
                        }
                        break;
                    }
                case ButtonState.Mine:
                    {
                        sender.Content = "X";
                        break;
                    }
                case ButtonState.Number:
                    {
                        sender.Content = "" + sender.Neighbors.Count(x => x.State == ButtonState.Mine);
                        break;
                    }
            }
        }
        private void ManageGameOver(GameButton lastClickedButton)
        {
            if (_fields.Cast<GameButton>().Count(field => field.IsEnabled) == bomb_count && lastClickedButton.State != ButtonState.Mine)
            {
                _timer.Stop();
                MessageBoxResult result = MessageBox.Show("Du hosd gwunga! Schowieda versuchn?", "Minesweeper", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    StartNewGame();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else if(lastClickedButton.State == ButtonState.Mine)
            {
                _timer.Stop();
                foreach (GameButton field in _fields)
                {
                    UpdateButton(field, false);
                }
                MessageBoxResult result = MessageBox.Show("Du hosd voloan! Schowieda versuchn?", "Minesweeper", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    StartNewGame();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }
        private void UpdateElapsedTime(object sender, System.Timers.ElapsedEventArgs e)
        {
            _elapsedSeconds++;
            Dispatcher.Invoke(() =>
            {
                int minutes = _elapsedSeconds / 60;
                int seconds = _elapsedSeconds % 60;
                _timerTB.Text = $"Elapsed Time: {minutes:00}:{seconds:00}";
            });
        }
    }
}
