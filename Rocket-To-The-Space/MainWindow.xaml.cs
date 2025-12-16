using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        private uint launchCount = 0;
        private Slot[] shopSlots = new Slot[8];
        private Slot lastSelectedShopSlot;
        private Button sellButton = new Button();
        private Button addToRocketButton = new Button();
        private static readonly decimal WOOD_MIN_PRICE = 0.5m;
        private static readonly decimal WOOD_MAX_PRICE = 10m;
        private static readonly int WOOD_MIN_WEIGHT = 10;
        private static readonly int WOOD_MAX_WEIGHT = 20;
        private static readonly int WOOD_MIN_BOOSTER_THRUST_POWER = 80;
        private static readonly int WOOD_MAX_BOOSTER_THRUST_POWER = 100;
        private static readonly int WOOD_MAX_ENERGY = 1000;
        private static readonly double WOOD_CAPSULE_CONTROL_MULTIPLIER = 0.5;
        private static readonly int WOOD_ENGINE_MIN_THRUST_POWER = 50;
        private static readonly int WOOD_ENGINE_MAX_THRUST_POWER = 100;
        private static readonly int WOOD_TANK_FUEL_CAPACITY = 1000;
        private static readonly decimal PLASTIC_MIN_PRICE = 20m;
        private static readonly decimal PLASTIC_MAX_PRICE = 50m;
        private static readonly double PLASTIC_CAPSULE_CONTROL_MULTIPLIER = 0.7;
        private static readonly int PLASTIC_ENGINE_MIN_THRUST_POWER = 150;
        private static readonly int PLASTIC_ENGINE_MAX_THRUST_POWER = 200;
        private static readonly int PLASTIC_TANK_FUEL_CAPACITY = 5000;
        private static readonly double CARBON_CAPSULE_CONTROL_MULTIPLIER = 1;
        private static readonly decimal CARBON_MIN_PRICE = 100m;
        private static readonly decimal CARBON_MAX_PRICE = 1000m;
        private static readonly int CARBON_ENGINE_MIN_THRUST_POWER = 250;
        private static readonly int CARBON_ENGINE_MAX_THRUST_POWER = 300;
        private static readonly int CARBON_TANK_FUEL_CAPACITY = 15000;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            Cursor = Cursors.None;
            cursor.IsHitTestVisible = false;
            ShowMainMenu();
        }

        private void InitializeButtons()
        {
            sellButton.Content = "Vendre";
            sellButton.Width = 90;
            sellButton.Height = 40;
            Panel.SetZIndex(sellButton, 3);
            sellButton.Click += SellButtonClickHandler;
            sellButton.HorizontalAlignment = HorizontalAlignment.Left;
            sellButton.VerticalAlignment = VerticalAlignment.Top;
            sellButton.FontSize = 16;
            sellButton.IsEnabled = false;

            addToRocketButton.Content = "Ajouter";
            addToRocketButton.Width = 90;
            addToRocketButton.Height = 40;
            Panel.SetZIndex(addToRocketButton, 3);
            addToRocketButton.Click += AddToRocketButtonClickHandler;
            addToRocketButton.HorizontalAlignment = HorizontalAlignment.Left;
            addToRocketButton.VerticalAlignment = VerticalAlignment.Top;
            addToRocketButton.FontSize = 16;
            addToRocketButton.IsEnabled = false;


            ((UCGame)currentUC).gameCanvas.Children.Add(sellButton);
            Canvas.SetLeft(sellButton, 5);
            Canvas.SetTop(sellButton, 450);
            ((UCGame)currentUC).gameCanvas.Children.Add(addToRocketButton);
            Canvas.SetLeft(addToRocketButton, 5);
            Canvas.SetTop(addToRocketButton, 400);
        }

        private void AddToRocketButtonClickHandler(object sender, RoutedEventArgs e)
        {
            
        }

        private void SellButtonClickHandler(object sender, RoutedEventArgs e)
        {
            
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
                Panel.SetZIndex(texture, 3);
                Panel.SetZIndex(selectedTexture, 3);
                slots[i] = new Slot(texture, selectedTexture, x, y);
                slots[i].Draw();

            }   
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000/240);
            timer.Tick += new EventHandler(Update);
            timer.Start();
        }

        private void InitializeShop()
        {
            for (int i = 0; i < shopSlots.Length; i++)
            {
                RocketComponent shopItem = null;
                int rndType = random.Next(1, 5);
                if (rndType == 1)
                {
                    int weight = random.Next(WOOD_MIN_WEIGHT, WOOD_MAX_WEIGHT + 1);
                    decimal price = WOOD_MIN_PRICE + (decimal)(random.NextDouble()) * (WOOD_MAX_PRICE - WOOD_MIN_PRICE);
                    Image componentTexture = new Image();
                    BitmapImage img = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/RocketBoosters/wooden_booster.png", UriKind.Absolute));
                    componentTexture.Source = img;
                    int thrustPower = random.Next(WOOD_MIN_BOOSTER_THRUST_POWER, WOOD_MAX_BOOSTER_THRUST_POWER + 1);
                    shopItem = new RocketBooster("Wood Booster", weight, price, componentTexture, ((UCGame) currentUC).gameCanvas, thrustPower, WOOD_MAX_ENERGY);
                } else if (rndType == 2)
                {
                    int weight = random.Next(WOOD_MIN_WEIGHT, WOOD_MAX_WEIGHT + 1);
                    decimal price = WOOD_MIN_PRICE + (decimal)(random.NextDouble()) * (WOOD_MAX_PRICE - WOOD_MIN_PRICE);
                    Image componentTexture = new Image();
                    BitmapImage img = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/RocketCapsules/wooden_capsule.png", UriKind.Absolute));
                    componentTexture.Source = img;
                    int thrustPower = random.Next(WOOD_MIN_BOOSTER_THRUST_POWER, WOOD_MAX_BOOSTER_THRUST_POWER + 1);
                    shopItem = new RocketCapsule("Wood Capsule", weight, price, componentTexture, ((UCGame)currentUC).gameCanvas, WOOD_CAPSULE_CONTROL_MULTIPLIER);
                } else if (rndType == 3)
                {
                    int weight = random.Next(WOOD_MIN_WEIGHT, WOOD_MAX_WEIGHT + 1);
                    decimal price = WOOD_MIN_PRICE + (decimal)(random.NextDouble()) * (WOOD_MAX_PRICE - WOOD_MIN_PRICE);
                    Image componentTexture = new Image();
                    BitmapImage img = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/RocketEngine/wooden_engine.png", UriKind.Absolute));
                    componentTexture.Source = img;
                    int thrustPower = random.Next(WOOD_ENGINE_MIN_THRUST_POWER, WOOD_ENGINE_MAX_THRUST_POWER + 1);
                    shopItem = new RocketEngine("Wood Engine", weight, price, componentTexture, ((UCGame)currentUC).gameCanvas, thrustPower);
                } else if (rndType == 4)
                {
                    int weight = random.Next(WOOD_MIN_WEIGHT, WOOD_MAX_WEIGHT + 1);
                    decimal price = WOOD_MIN_PRICE + (decimal)(random.NextDouble()) * (WOOD_MAX_PRICE - WOOD_MIN_PRICE);
                    Image componentTexture = new Image();
                    BitmapImage img = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/RocketTanks/wooden_tank.png", UriKind.Absolute));
                    componentTexture.Source = img;
                    int thrustPower = random.Next(WOOD_ENGINE_MIN_THRUST_POWER, WOOD_ENGINE_MAX_THRUST_POWER + 1);
                    shopItem = new RocketTank("Wood Tank", weight, price, componentTexture, ((UCGame)currentUC).gameCanvas, WOOD_TANK_FUEL_CAPACITY);
                }
                ((UCGame)currentUC).gameCanvas.Children.Add(shopItem.Texture);
                Image texture = new Image();
                texture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/inventory_slot.png", UriKind.Absolute));
                texture.Width = 48;
                texture.Height = 48;
                Image selectedTexture = new Image();
                selectedTexture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/inventory_slot_selected.png", UriKind.Absolute));
                ((UCGame)currentUC).gameCanvas.Children.Add(texture);
                ((UCGame)currentUC).gameCanvas.Children.Add(selectedTexture);
                selectedTexture.Width = 48;
                selectedTexture.Height = 48;
                Slot slot = new Slot(texture, selectedTexture, 0, 0);
                slot.SetComponent(shopItem);
                slot.Hide();
            } 
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
            mediaPlayer.Volume = 0.5;
            mediaPlayer.Play();
        }

        private void ShowRuleScreen(object sender, RoutedEventArgs e)
        {
            UCGameRules gameRules = new UCGameRules();
            currentUC = gameRules;
            mainContentControl.Content = gameRules;
            gameRules.startButton.Click += ShowGameScreen;
            mediaPlayer.Stop();
        }

        private void ShowGameScreen(object sender, RoutedEventArgs e)
        {
            UCGame game = new UCGame();
            int i = 0;

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
            InitializeShop();
            InitializeSlots(game);
            InitializeButtons();
            game.shop.Visibility = Visibility.Collapsed;
            mainContentControl.Content = game;
            game.gameCanvas.MouseDown += GameClickHandler;
            game.launchButton.Click += LaunchRocket;
            mainWindow.KeyDown += GameKeyPressHandler;
            rocket = new Rocket();
            rocket.Init(game);
            rocket.Draw(camera);
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
            launchCount++;
            currentStage = 1;
            foreach (Slot slot in slots)
            {
                slot.Hide();
            }
            UCGame game = (UCGame)currentUC;
            game.launchButton.Visibility = Visibility.Hidden;
            addToRocketButton.Visibility = Visibility.Hidden;
            sellButton.Visibility = Visibility.Hidden;

        }

        private void GameClickHandler(object sender, MouseButtonEventArgs e)
        {
            Slot slotClicked;
            if (GetSelectedSlot(out slotClicked))
            {
                SelectSlot(slotClicked);
                if (slotClicked.GetRocketComponent() != null)
                {
                    addToRocketButton.IsEnabled = true;
                    sellButton.IsEnabled = true;
                }
                else
                {
                    addToRocketButton.IsEnabled = false;
                    sellButton.IsEnabled = false;
                }
            }
            else if (!isRocketLaunched)
            {
                lastSelectedSlot?.Deselect();
                addToRocketButton.IsEnabled = false;
                sellButton.IsEnabled = false;
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
            rocket.Update();
            rocket.Draw(camera);
            camera.Y = (rocket.Y + rocket.RocketBox.ActualHeight + 90) - ((UCGame)currentUC).gameCanvas.Height;
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
                    if (obj is Rectangle)
                    { 
                        Rectangle rec = (Rectangle)obj;
                        double left = Canvas.GetLeft(rec);
                        double top = Canvas.GetTop(rec);
                        double localDeltaX = deltaX;
                        double localDeltaY = deltaY;
                        Canvas.SetLeft(rec, left - localDeltaX);
                        Canvas.SetTop(rec, top - localDeltaY);
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
            if (currentStage == 5 || currentStage == 6)
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

        private void generateBaseInventory()
        {
            if (launchCount == 0)
            {
            }
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