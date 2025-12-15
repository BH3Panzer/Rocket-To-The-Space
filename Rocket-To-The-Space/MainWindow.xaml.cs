using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace Rocket_To_The_Space
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer? timer;
        private Slot[] slots = new Slot[8];
        public UserControl? currentUC;
        private Slot? lastSelectedSlot;
        private bool isRocketLaunched = false;
        private Rocket? rocket;
        private Camera camera = new Camera(0, 0);
        private double lastCameraX = 0;
        private double lastCameraY = 0;
        private List<Image> decoration = new List<Image>();
        private int currentStage = 0;
        private static readonly double BACKGROUND_SPEED_MULTIPLIER = 0.05;
        private static readonly double DECORATION_SPEED_MULTIPLIER = 0.2;
        private static readonly Random random = new Random();
        private Label[] stageCheckpoint = new Label[6];
        private double[] initialStageCheckpointY = new double[6];
        private List<Obstacle> obstacles = new List<Obstacle>();
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            Cursor = Cursors.None;
            cursor.IsHitTestVisible = false;
            ShowMainMenu();
        }

        private void InitializeSlots(UCGame ucGame)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int x = i * 48 + 60;
                int y = 510;
                Image texture = new Image();
                texture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/inventory_slot.png", UriKind.Absolute));
                texture.Width = 48;
                texture.Height = 48;
                Image selectedTexture = new Image();
                selectedTexture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/inventory_slot_selected.png", UriKind.Absolute));
                ucGame.gameCanvas.Children.Add(texture);
                selectedTexture.Width = 48;
                selectedTexture.Height = 48;
                selectedTexture.Visibility = Visibility.Hidden;
                ucGame.gameCanvas.Children.Add(selectedTexture);
                Canvas.SetLeft(texture, x);
                Canvas.SetTop(texture, y);
                Canvas.SetLeft(selectedTexture, x);
                Canvas.SetTop(selectedTexture, y);
                Panel.SetZIndex(texture, 3);
                Panel.SetZIndex(selectedTexture, 3);
                slots[i] = new Slot(texture, selectedTexture);

            }   
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000/240);
            timer.Tick += new EventHandler(Update);
            timer.Start();
        }

        private void Update(object? sender, EventArgs e)
        {
            UpdateCursor(sender, e);
            if (currentUC is UCGame)
            {
                UpdateGame(sender, e);
            }
        }

        private void UpdateCursor(object? sender, EventArgs e)
        {
            Point mousePos = Mouse.GetPosition(mainCanvas);
            Canvas.SetLeft(cursor, mousePos.X);
            Canvas.SetTop(cursor, mousePos.Y);
        }

        private void ShowMainMenu()
        {
            UCMainMenu mainMenu = new UCMainMenu();
            currentUC = mainMenu;
            mainContentControl.Content = mainMenu;
            mainMenu.PlayButton.Click += ShowRuleScreen;
            mediaPlayer.Open(new Uri("Assets/Music/BH3Panzer - To the Space.mp3", UriKind.Relative));
            mediaPlayer.Play();
        }

        private void ShowRuleScreen(object sender, RoutedEventArgs e)
        {
            UCGameRules gameRules = new UCGameRules();
            currentUC = gameRules;
            mainContentControl.Content = gameRules;
            gameRules.startButton.Click += ShowGameScreen;
        }

        private void ShowGameScreen(object sender, RoutedEventArgs e)
        {
            UCGame game = new UCGame();
            int i = 0;

            mediaPlayer.Stop();

            foreach (UIElement obj in game.gameCanvas.Children)
            {
                if (obj is Label)
                {
                    stageCheckpoint[i] = (Label)obj;
                    initialStageCheckpointY[i] = Canvas.GetTop(obj);
                    i++;
                }
            }
            currentUC = game;
            InitializeSlots(game);
            mainContentControl.Content = game;
            game.gameCanvas.MouseDown += GameClickHandler;
            game.launchButton.Click += LaunchRocket;
            mainWindow.KeyDown += GameKeyPressHandler;
            rocket = new Rocket();
#if DEBUG
            mainWindow.KeyDown += DebugKeyHandler;
        }

        private void DebugKeyHandler(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftAlt))
            {
                if (e.Key >= Key.D1 && e.Key <= Key.D6)
                {
                    JumpToStage(e.Key - Key.D1);
                }
            }
        }

        private void JumpToStage(int stageIndex)
        {
            currentStage = stageIndex + 1;
            camera.Y = initialStageCheckpointY[stageIndex] / BACKGROUND_SPEED_MULTIPLIER;
        }
#else
        }
#endif



        private void LaunchRocket(object sender, RoutedEventArgs e)
        {
            //TODO: Check if rocket is ready to launch
            isRocketLaunched = true;
            currentStage = 1;
            foreach (Slot slot in slots)
            {
                slot.Hide();
            }
            UCGame game = (UCGame)currentUC;
            game.launchButton.Visibility = Visibility.Hidden;

        }

        private void GameClickHandler(object sender, MouseButtonEventArgs e)
        {
            Slot slotClicked;
            if (GetSelectedSlot(out slotClicked))
            {
               SelectSlot(slotClicked);
            } else if (!isRocketLaunched)
            {
                lastSelectedSlot?.Deselect();
            }
        }

        private void GameKeyPressHandler(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D1 && e.Key <= Key.D8)
            {
                int slotIndex = e.Key - Key.D1;
                SelectSlot(slots[slotIndex]);
            }
            if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad8)
            {
                int slotIndex = e.Key - Key.NumPad1;
                SelectSlot(slots[slotIndex]);
            }
        }

        private void SelectSlot(Slot slot)
        {
            slot.Select();
            if (lastSelectedSlot != null && lastSelectedSlot != slot)
            {
                lastSelectedSlot.Deselect();
            }
            lastSelectedSlot = slot;
        }  

        private void CreateCloud(int x, double y)
        {
            BitmapImage cloud = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/cloud_decoration.png", UriKind.Absolute));
            Image cloudImg = new Image();
            cloudImg.Source = cloud;
            cloudImg.Width = 64;
            cloudImg.Height = 64;
            cloudImg.Tag = "decoration_cloud";
            if (currentUC is UCGame)
            {
                ((UCGame)currentUC).gameCanvas.Children.Add(cloudImg);
            }
            Canvas.SetLeft(cloudImg, x);
            Canvas.SetTop(cloudImg, y);
            Panel.SetZIndex(cloudImg, 1);
            decoration.Add(cloudImg);

        }

        private void CreateAsteroid(int x, double y)
        {
            BitmapImage asteroid = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/asteroid.png", UriKind.Absolute));
            Image asteroidImg = new Image();
            asteroidImg.Source = asteroid;
            asteroidImg.Width = 64;
            asteroidImg.Height = 64;
            Obstacle asteroidObstacle = new Obstacle(ObstacleType.ASTEROID, asteroidImg);
            ((UCGame)currentUC).gameCanvas.Children.Add(asteroidImg);
            Canvas.SetLeft(asteroidImg, x);
            Canvas.SetTop(asteroidImg, y);
            Panel.SetZIndex(asteroidImg, 2);
            obstacles.Add(asteroidObstacle);

        }

        private void UpdateRocket()
        {
            if (!isRocketLaunched)
            {
                return;
            }
            camera.Y -= rocket.Speed; 
        }

        private void UpdateCamera()
        {
            if (currentUC is UCGame)
            {
                double deltaX = camera.X - lastCameraX;
                double deltaY = camera.Y - lastCameraY;
                lastCameraX = camera.X;
                lastCameraY = camera.Y;
                foreach (UIElement obj in ((UCGame)currentUC).gameCanvas.Children)
                {
                    if (obj is Image)
                    {
                        Image image = (Image)obj;
                        double left = Canvas.GetLeft(image);
                        double top = Canvas.GetTop(image);
                        double localDeltaX = deltaX;
                        double localDeltaY = deltaY;
                        if (Panel.GetZIndex(obj) == 0)
                        {
                            localDeltaX *= BACKGROUND_SPEED_MULTIPLIER;
                            localDeltaY *= BACKGROUND_SPEED_MULTIPLIER;
                        }

                        if (Panel.GetZIndex(obj) == 1)
                        {
                            localDeltaX *= DECORATION_SPEED_MULTIPLIER;
                            localDeltaY *= DECORATION_SPEED_MULTIPLIER;
                        }
                        Canvas.SetLeft(image, left - localDeltaX);
                        Canvas.SetTop(image, top - localDeltaY);
                    }
                    if (obj is Label)
                    {
                        Label label = (Label)obj;
                        double left = Canvas.GetLeft(label);
                        double top = Canvas.GetTop(label);
                        Canvas.SetLeft(label, left - deltaX * BACKGROUND_SPEED_MULTIPLIER);
                        Canvas.SetTop(label, top - deltaY * BACKGROUND_SPEED_MULTIPLIER);
                    }
                }
            }
        }

        private void UpdateDecoration()
        {
            if (currentStage == 1)
            {
                int p = random.Next(0,500);
                if (p == 0)
                {
                    int x = random.Next(0, (int)mainWindow.ActualWidth);
                    double y = -20;
                    CreateCloud(x, y);
                }
                List<Image> cloudToRemove = new List<Image>();
                foreach (Image cloud in decoration)
                {
       
                    if (Canvas.GetTop(cloud) >= ((UCGame)currentUC).gameCanvas.ActualHeight)
                    {
                        cloudToRemove.Add(cloud);
                        if (currentUC is UCGame)
                        {
                            ((UCGame)currentUC).gameCanvas.Children.Remove(cloud);
                        }
                    }
                }
                foreach (Image cloud in cloudToRemove)
                {
                    decoration.Remove(cloud);
                }
            }
        }

        private void UpdateCurrentStage()
        {
            if (currentStage == 7 || currentStage == 0)
            {
                return;
            }
            if (Canvas.GetTop(stageCheckpoint[currentStage - 1]) + mainWindow.ActualHeight  >= mainWindow.ActualHeight)
            {
                currentStage++;
            }
        }

        private void UpdateObstacles()
        {
            if (currentStage == 4 || currentStage == 5)
            {
                int p = random.Next(0,100) ;
                if (p == 0)
                {
                    double y = -20;
                    int x = random.Next(0, (int)mainWindow.ActualWidth);
                    Console.WriteLine($"created asteroid at x = {x} y = {y}");
                    CreateAsteroid(x, y);
                }
            }

            foreach (Obstacle obstacle in obstacles)
            {
                if (obstacle.Type == ObstacleType.ASTEROID)
                {
                    obstacle.SetRotationAngle(obstacle.GetRotationAngle() + 1);
                }
            }
        }

        private void UpdateGame(object? sender, EventArgs e)
        {
            UpdateRocket();
            UpdateDecoration();
            UpdateCurrentStage();
            UpdateObstacles();
            UpdateCamera();
        }

        private bool GetSelectedSlot(out Slot selectedSlot)
        {
            Point mousePos = Mouse.GetPosition(mainCanvas);
            foreach (Slot slot in slots)
            {
                if (IsSlotClicked(mousePos, slot))
                {
                    selectedSlot = slot;
                    return true;
                }
            }
            selectedSlot = null;
            return false;
        }

        private bool IsSlotClicked(Point mousePos, Slot slot)
        {
            double slotX = Canvas.GetLeft(slot.Image);
            double slotY = Canvas.GetTop(slot.Image);
            if (mousePos.X >= slotX && mousePos.X <= slotX + slot.Image.Width &&
                mousePos.Y >= slotY && mousePos.Y <= slotY + slot.Image.Height)
            {
                return true;
            }
            return false;
        }

        Geometry CreateRectGeometry(FrameworkElement e)
        {
            return new RectangleGeometry(
                new Rect(0, 0, e.ActualWidth, e.ActualHeight));
        }

        bool IsColliding(FrameworkElement a, FrameworkElement b)
        {
            Geometry g1 = CreateRectGeometry(a);
            Geometry g2 = CreateRectGeometry(b);

            g1.Transform = a.TransformToVisual(null) as Transform;
            g2.Transform = b.TransformToVisual(null) as Transform;

            return g1.FillContainsWithDetail(g2) != IntersectionDetail.Empty;
        }
    }
}