using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
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
        private Slot[] shopSlots = new Slot[8];
        private Slot lastSelectedShopSlot;
        private bool isShopOpened = false;
        private decimal money = 100;
        private static readonly Dictionary<string, decimal> PRICES = new Dictionary<string, decimal>()
        {
            {"WOODEN_MIN", 0.5m },
            {"WOODEN_MAX", 10m },
            {"PLASTIC_MIN", 20m },
            {"PLASTIC_MAX", 50m },
            {"CARBON_MIN", 100m },
            {"CARBON_MAX", 1000m }
        };
        private static readonly Dictionary<string, int> WEIGHTS = new Dictionary<string, int>
        {
            {"WOODEN_MIN", 10 },
            {"WOODEN_MAX", 20 }
        };
        private static readonly Dictionary<string, int> BOOSTER_THRUST_POWER = new Dictionary<string, int>
        {
            {"WOODEN_MIN", 80 },
            {"WOODEN_MAX", 100 }
        };

        private static readonly Dictionary<string, int> MAX_ENERGY = new Dictionary<string, int>
        {
            {"WOODEN", 1000 }
        };

        private static readonly Dictionary<string, double> CONTROL_MULTIPLIER = new Dictionary<string, double>()
        {
            {"WOODEN", 0.5 },
            {"PLASTIC", 0.7 },
            {"CARBON", 1 }
        };

        private static readonly Dictionary<string, int> ENGINE_THRUST_POWER = new Dictionary<string, int>
        {
            {"WOODEN_MIN", 50 },
            {"WOODEN_MAX", 100},
            {"PLASTIC_MIN", 150 },
            {"PLASTIC_MAX", 200 },
            {"CARBON_MIN", 250 },
            {"CARBON_MAX", 300}
        };

        private static readonly Dictionary<string, int> TANK_FUEL_CAPACITY = new Dictionary<string, int>()
        {
            {"WOODEN", 1000 },
            {"PLASTIC", 5000 },
            {"CARBON", 15000 }
        };

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

        private string GetTierNameEN(int tier)
        {
            switch (tier)
            {
                case 1:
                    return "Wooden";
                case 2:
                    return "plastic";
                case 3:
                    return "carbon";
            }
            return "";
        }

        private string GetTierNameFR(int tier)
        {
            switch (tier)
            {
                case 1:
                    return "bois";
                case 2:
                    return "plastique";
                case 3:
                    return "carbone";
            }
            return "";
        }

        private void resetShop(int tier1, int tier2)
        {
            UCGame game = (UCGame)currentUC;
            game.shopItemInfo.Visibility = Visibility.Collapsed;
            double slotX = (game.shop.Width - 48 * 4) / 4;
            double slotY = 50;
            for (int i = 0; i < shopSlots.Length; i++)
            {
                int tierChosen = random.Next(1, 6);
                int currentTier = -1;
                if (tierChosen == 1)
                {
                    currentTier = tier2;
                } else
                {
                    currentTier = tier1;
                }
                string frTier = GetTierNameFR(currentTier);
                string enTier = GetTierNameEN(currentTier);
                RocketComponent shopItem = null;
                int rndType = random.Next(1, 5);
                int weight = random.Next(WEIGHTS[$"{enTier.ToUpper()}_MIN"], WEIGHTS[$"{enTier.ToUpper()}_MAX"] + 1);
                decimal price = Math.Round(PRICES[$"{enTier.ToUpper()}_MIN"] + (decimal)(random.NextDouble()) * (PRICES[$"{enTier.ToUpper()}_MAX"] - PRICES[$"{enTier.ToUpper()}_MIN"]), 2);
                Image componentTexture = new Image();
                componentTexture.Width = 32;
                componentTexture.Height = 32;
                if (rndType == 1)
                {
                    BitmapImage img = new BitmapImage(new Uri($"pack://application:,,,/Assets/Img/RocketBoosters/{enTier}_booster.png", UriKind.Absolute));
                    componentTexture.Source = img;
                    componentTexture.Width = 16;
                    componentTexture.Height = 32;
                    int thrustPower = random.Next(BOOSTER_THRUST_POWER[$"{enTier.ToUpper()}_MIN"], BOOSTER_THRUST_POWER[$"{enTier.ToUpper()}_MAX"] + 1);
                    shopItem = new RocketBooster($"Boosteur en {frTier}", weight, price, componentTexture, ((UCGame) currentUC).gameCanvas, thrustPower, MAX_ENERGY[$"{enTier.ToUpper()}"]);
                } else if (rndType == 2)
                {
                    BitmapImage img = new BitmapImage(new Uri($"pack://application:,,,/Assets/Img/RocketCapsules/{enTier}_capsule.png", UriKind.Absolute));
                    componentTexture.Source = img;
                    shopItem = new RocketCapsule($"Capsule en {frTier}", weight, price, componentTexture, ((UCGame)currentUC).gameCanvas, CONTROL_MULTIPLIER[$"{enTier.ToUpper()}"]);
                } else if (rndType == 3)
                {
                    BitmapImage img = new BitmapImage(new Uri($"pack://application:,,,/Assets/Img/RocketEngine/{enTier}_engine.png", UriKind.Absolute));
                    componentTexture.Source = img;
                    int thrustPower = random.Next(ENGINE_THRUST_POWER[$"{enTier.ToUpper()}_MIN"], ENGINE_THRUST_POWER[$"{enTier.ToUpper()}_MAX"] + 1);
                    shopItem = new RocketEngine($"Moteur en {frTier}", weight, price, componentTexture, ((UCGame)currentUC).gameCanvas, thrustPower);
                } else if (rndType == 4)
                {
                    BitmapImage img = new BitmapImage(new Uri($"pack://application:,,,/Assets/Img/RocketTanks/{enTier}_tank.png", UriKind.Absolute));
                    componentTexture.Source = img;
                    shopItem = new RocketTank($"Réservoir en {frTier}", weight, price, componentTexture, ((UCGame)currentUC).gameCanvas, TANK_FUEL_CAPACITY[$"{enTier.ToUpper()}"]);
                }
                Image texture = new Image();
                texture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/inventory_slot.png", UriKind.Absolute));
                texture.Width = 48;
                texture.Height = 48;
                Image selectedTexture = new Image();
                selectedTexture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/inventory_slot_selected.png", UriKind.Absolute));
                game.shop.Children.Add(texture);
                game.shop.Children.Add(selectedTexture);
                game.shop.Children.Add(shopItem.Texture);
                texture.HorizontalAlignment = HorizontalAlignment.Left;
                texture.VerticalAlignment = VerticalAlignment.Top;
                selectedTexture.HorizontalAlignment = HorizontalAlignment.Left;
                selectedTexture.VerticalAlignment = VerticalAlignment.Top;
                shopItem.Texture.HorizontalAlignment = HorizontalAlignment.Left;
                shopItem.Texture.VerticalAlignment = VerticalAlignment.Top;
                Canvas.SetLeft(texture, slotX);
                Canvas.SetLeft(selectedTexture, slotX);
                Canvas.SetTop(texture, slotY);
                Canvas.SetLeft(shopItem.Texture, (texture.Width - shopItem.Texture.Width) / 2 + slotX);
                Canvas.SetTop(shopItem.Texture, (texture.Height - shopItem.Texture.Height) / 2 + slotY);
                Canvas.SetTop(selectedTexture, slotY);
                Canvas.SetZIndex(texture, 3);
                Canvas.SetZIndex(selectedTexture, 3);
                Canvas.SetZIndex(shopItem.Texture, 3);
                selectedTexture.Width = 48;
                selectedTexture.Height = 48;

                Slot slot = new Slot(texture, selectedTexture);
                slot.SetComponent(shopItem);
                slot.Deselect();
                slotX += 48;
                if ((i + 1) % 4 == 0)
                {
                    slotY += 48;
                    slotX = (game.shop.Width - 48 * 4) / 4;
                }
                shopSlots[i] = slot;
            }
            game.buyButton.Click += Buy;
            game.closeShopButton.Click += CloseShop;
            game.openShopButton.Click += OpenShop;
        }

        private void OpenShop(object sender, RoutedEventArgs e)
        {
            ((UCGame)currentUC).shop.Visibility = Visibility.Visible;
            isShopOpened = true;
        }

        private void Buy(object sender, RoutedEventArgs e)
        {
            foreach (Slot slot in slots)
            {
                if (slot.IsEmpty())
                {
                    money -= lastSelectedShopSlot.GetRocketComponent().Cost;
                    slot.SetComponent(lastSelectedShopSlot.GetRocketComponent().Clone());
                    Image texture = slot.GetRocketComponent().Texture;
                    ((UCGame)currentUC).gameCanvas.Children.Add(texture);
                    texture.HorizontalAlignment = HorizontalAlignment.Left;
                    texture.VerticalAlignment = VerticalAlignment.Top;
                    Canvas.SetLeft(texture, (slot.Image.Width - texture.Width) / 2 + Canvas.GetLeft(slot.Image));
                    Canvas.SetTop(texture, (slot.Image.Height - texture.Height) / 2 + Canvas.GetTop(slot.Image));
                    break;
                }
            }
        }

        private void CloseShop(object sender, RoutedEventArgs e)
        {
            ((UCGame)currentUC).shop.Visibility = Visibility.Collapsed;
            isShopOpened = false;
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
                    if ((string) ((Label)obj).Tag != "checkpoint")
                    {
                        continue;
                    }
                    stageCheckpoint[i] = (Label)obj;
                    initialStageCheckpointY[i] = Canvas.GetTop(obj);
                    i++;
                }
            }
            currentUC = game;
            resetShop(1, 1);
            InitializeSlots(game);
            game.shop.Visibility = Visibility.Collapsed;
            //game.shop.Visibility = Visibility.Collapsed;
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
            if (GetHoveredSlot(out slotClicked, slots, mainCanvas))
            {
               SelectSlot(slotClicked, ref lastSelectedSlot);
            } else if (!isRocketLaunched)
            {
                lastSelectedSlot?.Deselect();
            }
            if (GetHoveredSlot(out slotClicked, shopSlots, ((UCGame) currentUC).shop))
            {
                SelectSlot(slotClicked, ref lastSelectedShopSlot);
                ((UCGame)currentUC).buyButton.IsEnabled = true;
            } else
            {
                lastSelectedShopSlot?.Deselect();
                ((UCGame)currentUC).buyButton.IsEnabled = false;
            }
        }

        private void GameKeyPressHandler(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D1 && e.Key <= Key.D8)
            {
                int slotIndex = e.Key - Key.D1;
                SelectSlot(slots[slotIndex], ref lastSelectedSlot);
            }
            if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad8)
            {
                int slotIndex = e.Key - Key.NumPad1;
                SelectSlot(slots[slotIndex], ref lastSelectedSlot);
            }
        }

        private void SelectSlot(Slot slot, ref Slot lastSelectedSlot)
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

        private void UpdateShopHovering()
        {
            if (!isShopOpened)
            {
                return;
            }

            Slot hoveredSlot = null;
            if (GetHoveredSlot(out hoveredSlot, shopSlots, ((UCGame)currentUC).shop))
            {
                UCGame game = (UCGame)currentUC;
                game.shopItemInfo.Visibility = Visibility.Visible;
                int xOffset = 30;
                if ((Array.IndexOf(shopSlots, hoveredSlot) + 1) % 4 == 0)
                {
                    xOffset = -30;
                }
                Canvas.SetTop(game.shopItemInfo, 0);
                Canvas.SetLeft(game.shopItemInfo, mainWindow.Width - game.shopItemInfo.Width - 20);
                double currentMargin = 0;
                double marginInterval = 20;
                game.shopItemInfo.Children.Clear();
                Label name = new Label();
                Label weigth = new Label();
                Label cost = new Label();
                name.Content = hoveredSlot.GetRocketComponent().Name;
                name.Margin = new Thickness(0, currentMargin, 0, 0);
                currentMargin += marginInterval;
                weigth.Content = $"Poids : {hoveredSlot.GetRocketComponent().Weight}";
                weigth.Margin = new Thickness(0, currentMargin, 0, 0);
                currentMargin += marginInterval;
                cost.Content = $"Prix : {hoveredSlot.GetRocketComponent().Cost}";
                cost.Margin = new Thickness(0, currentMargin, 0, 0);
                currentMargin += marginInterval;
                name.Foreground = Brushes.White;
                weigth.Foreground = Brushes.White;
                cost.Foreground = Brushes.White;
                game.shopItemInfo.Children.Add(name);
                game.shopItemInfo.Children.Add(weigth);
                game.shopItemInfo.Children.Add(cost);
                if (hoveredSlot.GetRocketComponent() is RocketBooster)
                {
                    RocketBooster component = (RocketBooster)hoveredSlot.GetRocketComponent();
                    Label thrustPower = new Label();
                    Label maxEnergy = new Label();
                    thrustPower.Content = $"Puissance : {component.ThrustPower}";
                    maxEnergy.Content = $"Énergie maximum : {component.MaxEnergy}";
                    thrustPower.Foreground = Brushes.White;
                    maxEnergy.Foreground = Brushes.White;
                    thrustPower.Margin = new Thickness(0, currentMargin, 0, 0);
                    currentMargin += marginInterval;
                    maxEnergy.Margin = new Thickness(0, currentMargin, 0, 0);
                    currentMargin += marginInterval;
                    game.shopItemInfo.Children.Add(thrustPower);
                    game.shopItemInfo.Children.Add(maxEnergy);
                } else if (hoveredSlot.GetRocketComponent() is RocketCapsule)
                {
                    RocketCapsule component = (RocketCapsule)hoveredSlot.GetRocketComponent();
                    Label controlMultiplier = new Label();
                    controlMultiplier.Content = $"maniabilité : {component.ControlMultiplier}";
                    controlMultiplier.Foreground = Brushes.White;
                    game.shopItemInfo.Children.Add(controlMultiplier);
                    controlMultiplier.Margin = new Thickness(0, currentMargin, 0, 0);
                    currentMargin += marginInterval;
                } else if (hoveredSlot.GetRocketComponent() is RocketEngine)
                {
                    RocketEngine component = (RocketEngine)hoveredSlot.GetRocketComponent();
                    Label thrustPower = new Label();
                    thrustPower.Content = $"Puissance : {component.ThrustPower}";
                    thrustPower.Foreground = Brushes.White;
                    thrustPower.Margin = new Thickness(0, currentMargin, 0, 0);
                    currentMargin += marginInterval;
                    game.shopItemInfo.Children.Add(thrustPower);
                } else if (hoveredSlot.GetRocketComponent() is RocketTank)
                {
                    RocketTank component = (RocketTank)hoveredSlot.GetRocketComponent();
                    Label fuelCapacity = new Label();
                    fuelCapacity.Content = $"Capacité : {component.FuelCapacity}";
                    fuelCapacity.Foreground = Brushes.White;
                    fuelCapacity.Margin = new Thickness(0, currentMargin, 0, 0);
                    currentMargin += marginInterval;
                    game.shopItemInfo.Children.Add(fuelCapacity);
                }
            } else
            {
                ((UCGame)currentUC).shopItemInfo.Visibility = Visibility.Hidden;
            }

        }

        private void UpdateMoney()
        {
            ((UCGame)currentUC).moneyText.Content = money.ToString();
        }

        private void UpdateGame(object? sender, EventArgs e)
        {
            UpdateRocket();
            UpdateDecoration();
            UpdateCurrentStage();
            UpdateObstacles();
            UpdateCamera();
            UpdateShopHovering();
            UpdateMoney();
        }

        private bool GetHoveredSlot(out Slot selectedSlot, Slot[] slots, Canvas canvas)
        {
            Point mousePos = Mouse.GetPosition(canvas);
            foreach (Slot slot in slots)
            {
                if (IsSlotHovered(mousePos, slot))
                {
                    selectedSlot = slot;
                    return true;
                }
            }
            selectedSlot = null;
            return false;
        }

        private bool IsSlotHovered(Point mousePos, Slot slot)
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