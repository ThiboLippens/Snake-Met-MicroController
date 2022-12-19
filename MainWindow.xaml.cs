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
using System.IO.Ports;
using System.IO;
using System.Collections.Immutable;
using System.Windows.Threading;
using System.Diagnostics;
using System.Timers;

namespace Project_6_12_2022
{

    public partial class MainWindow : Window
    {
        SerialPort _serialPort;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OverlayStart.Visibility = Visibility.Visible;
            OverlayPC.Visibility = Visibility.Hidden;
            OverLayPCText.Visibility = Visibility.Hidden;
            OverlayMicro.Visibility = Visibility.Hidden;
            OverLayMicroText.Visibility = Visibility.Hidden;
        }


        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food },
        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 },
        };

        private readonly int rows = 15, cols = 15;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;
        private bool gameKey;
        private Stopwatch _stopwatch;
        private Timer _timer;
        const string _startTimerDisplay = "00:00";

        public MainWindow()
        {
            InitializeComponent();
            TimerText.Text = _startTimerDisplay;
            _stopwatch = new Stopwatch();
            _timer = new Timer(interval:1000);
            _timer.Elapsed += OnTimerElapse;
            cbxComPorts.Items.Add("None");
            foreach (string port in SerialPort.GetPortNames())
                cbxComPorts.Items.Add(port);
            _serialPort = new SerialPort();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
        }
        private void OnTimerElapse(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => TimerText.Text = _stopwatch.Elapsed.ToString(format: @"mm\:ss"));
        }
        private void StartTimer()
        {
            _stopwatch.Start();
            _timer.Start();
        }
        private void StopTimer()
        {
            _stopwatch.Stop();
            _timer.Stop();
        }
        private void ResetTimer()
        {
            _timer.Stop();
            _stopwatch.Reset();
            TimerText.Text = _startTimerDisplay;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((_serialPort != null) && (_serialPort.IsOpen))
            {
                _serialPort.Write(new byte[] { 0 }, 0, 1);

                // Gooi alle info van de _serialPort in de vuilbak.
                _serialPort.Dispose();
            }
        }

        private void cbxComPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                if (cbxComPorts.SelectedItem.ToString() != "None")
                {
                    _serialPort.PortName = cbxComPorts.SelectedItem.ToString();
                    _serialPort.Open();
                    gameKey = true;
                    OverlayStart.Visibility = Visibility.Hidden;
                    OverlayPC.Visibility = Visibility.Hidden;
                    OverLayPCText.Visibility = Visibility.Hidden;
                    OverlayMicro.Visibility = Visibility.Visible;
                    OverLayMicroText.Visibility = Visibility.Visible;
                    _serialPort.DataReceived += _serialPort_DataReceived;
                }
                else
                {
                    gameKey = false;
                    OverlayStart.Visibility = Visibility.Hidden;
                    OverlayPC.Visibility = Visibility.Visible;
                    OverLayPCText.Visibility = Visibility.Visible;
                }  
            }
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            OverlayPC.Visibility = Visibility.Hidden;
            OverlayMicro.Visibility = Visibility.Hidden;
            StartTimer();
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            if (gameKey == false)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        gameState.ChangeDirection(Direction.Left);
                        break;
                    case Key.Right:
                        gameState.ChangeDirection(Direction.Right);
                        break;
                    case Key.Up:
                        gameState.ChangeDirection(Direction.Up);
                        break;
                    case Key.Down:
                        gameState.ChangeDirection(Direction.Down);
                        break;
                }
            }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Lees alle tekst in, tot je een 'nieuwe lijn symbool' binnenkrijgt.
            // New line = '\n' = ASCII-waarde 10 = ALT 10. 
            string receivedText = _serialPort.ReadLine().Trim('\r');

            // Geef de ontvangen data door aan een method die op de UI thread loopt.
            // Doe dat via een Action delegate... Delegates en Events zullen 
            // in detail behandeld worden in het vak OOP.
            Dispatcher.Invoke(new Action<string>(SwitchPress), receivedText);
        }

        private async void SwitchPress(String text)
        {
            if (gameKey == true)
            {
                //Ontvangen van MicroController Switches
                switch (text)
                {
                    case "DOWN":
                        if(gameRunning == false)
                        {
                            gameRunning = true;
                            await RunGame();
                            gameRunning = false;
                        }else
                        gameState.ChangeDirection(Direction.Down);
                        break;
                    case "UP":
                        if (gameRunning == false)
                        {
                            gameRunning = true;
                            await RunGame();
                            gameRunning = false;
                        }
                        else
                            gameState.ChangeDirection(Direction.Up);
                        break;
                    case "LEFT":
                        if (gameRunning == false)
                        {
                            gameRunning = true;
                            await RunGame();
                            gameRunning = false;
                        }
                        else
                            gameState.ChangeDirection(Direction.Left);
                        break;
                    case "RIGHT":
                        if (gameRunning == false)
                        {
                            gameRunning = true;
                            await RunGame();
                            gameRunning = false;
                        }
                        else
                            gameState.ChangeDirection(Direction.Right);
                        break;
                }
                //OverlayMicro.Visibility = Visibility.Hidden;
                if (!gameRunning)
                {
                    gameRunning = true;
                    await RunGame();
                    gameRunning = false;
                }
            }
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (OverlayPC.Visibility == Visibility.Visible)
            {
                e.Handled = true;
                OverlayMicro.Visibility = Visibility.Hidden;
            }
            if (OverlayMicro.Visibility == Visibility.Visible)
            {
                e.Handled = true;
                OverlayPC.Visibility = Visibility.Hidden;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(100); //Voor als je het spel sneller of trager wilt
                gameState.Move();
                Draw();
            }
        }
        

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;

            for(int r = 0; r < rows; r++)
            {
                for(int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }
            return images;
        }
        private string HighScore()
        {
            int Score = gameState.Score;
            string FilePath = "C:\\Users\\thibo\\Source\\Repos\\ProjectICT\\Afbeeldingen\\HighScore.txt";
            
                StreamReader Sr = new StreamReader(FilePath);
                int HighScore = Convert.ToInt32(Sr.ReadToEnd());
                Sr.Close();
                if(Score > HighScore)
                {
                    StreamWriter Sw = new StreamWriter(FilePath);
                    Sw.Write(Score.ToString());
                    Sw.Close();
                    return Score.ToString();
                }else
                {
                    return Convert.ToString(HighScore);
                }
            
        }
        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            HighScoreText.Text = $"HighScore: {HighScore()}";
            ScoreText.Text = $"SCORE {gameState.Score}";
        }

        private void DrawGrid()
        {
            for(int r = 0; r < rows; r++)
            {
                for(int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            SnakePosition headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<SnakePosition> positions = new List<SnakePosition>(gameState.SnakePositions());

            for(int i = 0; i < positions.Count; i++)
            {
                SnakePosition pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }

        private async Task ShowCountDown()
        {
            ResetTimer();
            for (int i = 3; i >= 1; i--)
            {
                if (cbxComPorts.SelectedItem.ToString() != "None")
                {
                    OverLayMicroText.Text = i.ToString();
                    OverLayPCText.Visibility = Visibility.Hidden;

                }
                else
                {
                    OverLayPCText.Text = i.ToString();
                    OverLayMicroText.Visibility = Visibility.Hidden;
                }
                //OverLayPCText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            StopTimer();
            await DrawDeadSnake();
            await Task.Delay(1000);
            if (cbxComPorts.SelectedItem.ToString() != "None")
            {
                OverlayStart.Visibility = Visibility.Hidden;
                OverlayMicro.Visibility = Visibility.Visible;
                OverLayMicroText.Text = "PRESS ANY SWITCH TO START";
                OverLayMicroText.Visibility = Visibility.Visible;
            }
            else
            {
                OverlayStart.Visibility = Visibility.Hidden;
                OverlayPC.Visibility = Visibility.Visible;
                OverLayPCText.Text = "PRESS ANY KEY TO START";
                OverLayPCText.Visibility = Visibility.Visible;
            }
        }
    }
}
