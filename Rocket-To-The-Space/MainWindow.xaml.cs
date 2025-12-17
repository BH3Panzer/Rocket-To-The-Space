using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private uint launchCount = 20;
        private Slot[] shopSlots = new Slot[8];
        private Slot lastSelectedShopSlot;
        private bool isShopOpened = false;
        private decimal money = 2000;
        private bool isShopInialized = false;
        private bool isGameFinished = false;
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
            {"WOODEN_MAX", 20 },
            {"PLASTIC_MIN", 8 },
            {"PLASTIC_MAX", 15 },
            {"CARBON_MIN", 5 },
            {"CARBON_MAX", 10 }
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
        private Button sellButton = new Button();
        private Button addToRocketButton = new Button();
        private Button disassembleButton = new Button();

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

            disassembleButton.Content = "Démonter";
            disassembleButton.Width = 90;
            disassembleButton.Height = 40;
            Panel.SetZIndex(disassembleButton, 3);
            disassembleButton.Click += DisassembleRocket;
            disassembleButton.HorizontalAlignment = HorizontalAlignment.Left;
            disassembleButton.VerticalAlignment = VerticalAlignment.Top;
            disassembleButton.FontSize = 16;



            ((UCGame)currentUC).gameCanvas.Children.Add(sellButton);
            Canvas.SetLeft(sellButton, 5);
            Canvas.SetTop(sellButton, 450);
            ((UCGame)currentUC).gameCanvas.Children.Add(addToRocketButton);
            Canvas.SetLeft(addToRocketButton, 5);
            Canvas.SetTop(addToRocketButton, 400);
            ((UCGame)currentUC).gameCanvas.Children.Add(disassembleButton);
            Canvas.SetLeft(disassembleButton, 5);
            Canvas.SetTop(disassembleButton, 350);
        }

        private void DisassembleRocket(object sender, RoutedEventArgs e)
        {
            bool did = false;
            int length = rocket.GetComponents().Count;
            for (int i = length - 1; i >= 0; i--)
            {
                RocketComponent component = rocket.Components[i];
                did = false;
                foreach (Slot slot in slots)
                {
                    if (slot.IsEmpty() && !did)
                    {
                        rocket.RemoveComponent(((UCGame)currentUC).gameCanvas, component);
                        //rocket.UpdateComponents();
                        //rocket.Draw(camera);
                        slot.SetComponent(component);
                        component.IsAttachedToRocket = false;
                        Canvas.SetZIndex(slot.GetRocketComponent().Texture, 4);
                        ((UCGame)currentUC).gameCanvas.Children.Add(component.Texture);
                        did = true;
                    }
                    slot.Draw();
                    if (did)
                    {
                        break;
                    }
                }
                if (!did)
                {
                    rocket.RemoveComponent(((UCGame)currentUC).gameCanvas, component);
                }
            }
        }

        private void AddToRocketButtonClickHandler(object sender, RoutedEventArgs e)
        {
            RocketComponent compo = lastSelectedSlot.GetRocketComponent();
            if (rocket.AddComponent(compo))
            {
                lastSelectedSlot.RemoveComponent();
                addToRocketButton.IsEnabled = false;
                sellButton.IsEnabled = false;
                rocket.AddComponentToCanvas(compo, ((UCGame)currentUC).gameCanvas);
                rocket.Draw(camera);
            }
            

        }

        private void SellButtonClickHandler(object sender, RoutedEventArgs e)
        {
            money += lastSelectedSlot.GetRocketComponent().Cost;
            lastSelectedSlot.RemoveComponent();
            addToRocketButton.IsEnabled = false;
            sellButton.IsEnabled = false;
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
            GenerateBaseInventory();
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
            game.shopItemInfo.Visibility = Visibility.Hidden;
            double slotX = (game.shop.Width - 48 * 4) / 4;
            double slotY = 50;
            foreach (Slot slot in shopSlots)
            {
                if (slot != null)
                {
                    game.shop.Children.Remove(slot.Image);
                }
            }
            shopSlots = new Slot[8];
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
                int rndType = random.Next(2, 5);
                int weight = random.Next(WEIGHTS[$"{enTier.ToUpper()}_MIN"], WEIGHTS[$"{enTier.ToUpper()}_MAX"] + 1);
                decimal price = Math.Round(PRICES[$"{enTier.ToUpper()}_MIN"] + (decimal)(random.NextDouble()) * (PRICES[$"{enTier.ToUpper()}_MAX"] - PRICES[$"{enTier.ToUpper()}_MIN"]), 2);
                Image componentTexture = new Image();
                componentTexture.Width = 32;
                componentTexture.Height = 32;
                if (rndType == 2)
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
                Canvas.SetLeft(shopItem.Texture, (texture.Width - shopItem.Texture.Width) / 2 + slotX);
                Canvas.SetTop(shopItem.Texture, (texture.Height - shopItem.Texture.Height) / 2 + slotY);
                Canvas.SetZIndex(texture, 3);
                Canvas.SetZIndex(selectedTexture, 3);
                Canvas.SetZIndex(shopItem.Texture, 3);
                selectedTexture.Width = 48;
                selectedTexture.Height = 48;
                Slot slot = new Slot(texture, selectedTexture, slotX, slotY);
                slot.SetComponent(shopItem);
                slot.Deselect();
                slotX += 48;
                if ((i + 1) % 4 == 0)
                {
                    slotY += 48;
                    slotX = (game.shop.Width - 48 * 4) / 4;
                }
                shopSlots[i] = slot;
                shopSlots[i].Draw();
            }
            if (!isShopInialized)
            {
                game.buyButton.Click += Buy;
                isShopInialized = true;
            }
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
                    if (money - lastSelectedShopSlot.GetRocketComponent().Cost < 0)
                    {
                        return;
                    }
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
            UpdateShopTier();
            InitializeSlots(game);
            InitializeButtons();
            game.shop.Visibility = Visibility.Collapsed;
            //game.shop.Visibility = Visibility.Collapsed;
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
            rocket.Y = initialStageCheckpointY[stageIndex] / BACKGROUND_SPEED_MULTIPLIER;
            camera.Y = (rocket.Y + rocket.RocketBox.ActualHeight + 90) - ((UCGame)currentUC).gameCanvas.Height;
        }
#else
        }
#endif



        private void LaunchRocket(object sender, RoutedEventArgs e)
        {
            if (!rocket.IsReadyForLaunch(isRocketLaunched))
            {
                return;
            }
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
            disassembleButton.Visibility = Visibility.Hidden;
            game.openShopButton.Visibility = Visibility.Hidden;
            timer.Tick += rocket.DrainFuel;

        }

        private void ResetBackgroundAndLaunchpad()
        {
            if (currentUC is UCGame)
            {
                UCGame game = (UCGame)currentUC;
                foreach (UIElement obj in game.gameCanvas.Children)
                {
                    if (obj is Image && ((Image)obj).Name == "launchpad")
                    {
                        Canvas.SetLeft(obj, 0);
                        Canvas.SetTop(obj, 43);
                    }
                    if (obj is Image && ((Image)obj).Name == "bg")
                    {
                        Canvas.SetLeft((Image)obj, 0);
                        Canvas.SetTop((Image)obj, -64965);
                    }
                    camera.Y = 0;
                    camera.X = 0;
                }
            }
            UpdateShopTier();
        }

        private void UpdateShopTier()
        {
            if (launchCount <= 1)
            {
                resetShop(1, 1);
            } else if (launchCount >= 2 && launchCount <= 4)
            {
                resetShop(1, 2);
            } else if (launchCount >= 5 && launchCount <= 7)
            {
                resetShop(2, 2);
            } else if (launchCount >= 8 && launchCount <= 11)
            {
                resetShop(2, 3);
            } else if (launchCount >= 12)
            {
                resetShop(3, 3);
            }
        }

        private void GoBackToGround()
        {
            isRocketLaunched = false;
            foreach (Slot slot in slots)
            {
                slot.Show();
            }
            UCGame game = (UCGame)currentUC;
            game.launchButton.Visibility = Visibility.Visible;
            addToRocketButton.Visibility = Visibility.Visible;
            sellButton.Visibility = Visibility.Visible;
            disassembleButton.Visibility = Visibility.Visible;
            game.openShopButton.Visibility = Visibility.Visible;
            timer.Tick -= rocket.DrainFuel;
            rocket.ResetPosition(game);
            camera.Y = 0;
            lastCameraX = 0;
            lastCameraY = 0;
            currentStage = 0;
        }

        private void GameClickHandler(object sender, MouseButtonEventArgs e)
        {
            if (isRocketLaunched)
            {
                return;
            }
            Slot slotClicked;
            if (GetHoveredSlot(out slotClicked, slots, mainCanvas))
            {
                SelectSlot(slotClicked, ref lastSelectedSlot);
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
            if (isRocketLaunched)
            {
                return;
            }
            if (e.Key >= Key.D1 && e.Key <= Key.D8)
            {
                int slotIndex = e.Key - Key.D1;
                SelectSlot(slots[slotIndex], ref lastSelectedSlot);
                if (slots[slotIndex].GetRocketComponent() != null)
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
            if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad8)
            {
                int slotIndex = e.Key - Key.NumPad1;
                SelectSlot(slots[slotIndex], ref lastSelectedSlot);
                if (slots[slotIndex].GetRocketComponent() != null)
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

        private void UpdateRocket()
        {
            if (!isRocketLaunched)
            {
                return;
            }
            if (rocket.Update(ref money))
            {
                GoBackToGround();
                ResetBackgroundAndLaunchpad();
            }
            rocket.Draw(camera);
            camera.Y = (rocket.Y + rocket.RocketBox.ActualHeight + 90) - ((UCGame)currentUC).gameCanvas.Height;
            ((UCGame)currentUC).fuelText.Content = rocket.GetTotalFuel().ToString();
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
                    if (obj is Image && (obj.Uid == "bg" || ((Image)obj).Tag == "decoration_cloud" || ((Image)obj).Name == "launchpad"))
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
                    if (obj is Label && ((Label)obj).Tag == "checkpoint")
                    {
                        double left = Canvas.GetLeft(obj);
                        double top = Canvas.GetTop(obj);
                        double localDeltaX = deltaX;
                        double localDeltaY = deltaY;
                        localDeltaX *= BACKGROUND_SPEED_MULTIPLIER;
                        localDeltaY *= BACKGROUND_SPEED_MULTIPLIER;
                        Label label = (Label)obj;
                        Canvas.SetLeft(label, left - localDeltaX);
                        Canvas.SetTop(label, top - localDeltaY);
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
            if (currentStage == 6 || currentStage == 0)
            {
                if (currentStage == 6)
                {
                    isGameFinished = true;
                    MessageBox.Show("Félicitations ! Vous avez atteint l'espace !");
                }
                return;
            }
            if (Canvas.GetTop(stageCheckpoint[currentStage - 1]) + mainWindow.ActualHeight  >= mainWindow.ActualHeight)
            {
                currentStage++;
            }
        }

        private void SetInfo(Slot slot)
        {
            UCGame game = (UCGame)currentUC;
            game.shopItemInfo.Visibility = Visibility.Visible;
            Canvas.SetZIndex(game.shopItemInfo, 4);
            Canvas.SetTop(game.shopItemInfo, 0);
            Canvas.SetLeft(game.shopItemInfo, mainWindow.Width - game.shopItemInfo.Width - 20);
            double currentMargin = 0;
            double marginInterval = 20;
            game.shopItemInfo.Children.Clear();
            Label name = new Label();
            Label weigth = new Label();
            Label cost = new Label();
            name.Content = slot.GetRocketComponent().Name;
            name.Margin = new Thickness(0, currentMargin, 0, 0);
            currentMargin += marginInterval;
            weigth.Content = $"Poids : {slot.GetRocketComponent().Weight}";
            weigth.Margin = new Thickness(0, currentMargin, 0, 0);
            currentMargin += marginInterval;
            cost.Content = $"Prix : {slot.GetRocketComponent().Cost}";
            cost.Margin = new Thickness(0, currentMargin, 0, 0);
            currentMargin += marginInterval;
            name.Foreground = Brushes.White;
            weigth.Foreground = Brushes.White;
            cost.Foreground = Brushes.White;
            game.shopItemInfo.Children.Add(name);
            game.shopItemInfo.Children.Add(weigth);
            game.shopItemInfo.Children.Add(cost);
            if (slot.GetRocketComponent() is RocketBooster)
            {
                RocketBooster component = (RocketBooster)slot.GetRocketComponent();
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
            }
            else if (slot.GetRocketComponent() is RocketCapsule)
            {
                RocketCapsule component = (RocketCapsule)slot.GetRocketComponent();
                Label controlMultiplier = new Label();
                controlMultiplier.Content = $"maniabilité : {component.ControlMultiplier}";
                controlMultiplier.Foreground = Brushes.White;
                game.shopItemInfo.Children.Add(controlMultiplier);
                controlMultiplier.Margin = new Thickness(0, currentMargin, 0, 0);
                currentMargin += marginInterval;
            }
            else if (slot.GetRocketComponent() is RocketEngine)
            {
                RocketEngine component = (RocketEngine)slot.GetRocketComponent();
                Label thrustPower = new Label();
                thrustPower.Content = $"Puissance : {component.ThrustPower}";
                thrustPower.Foreground = Brushes.White;
                thrustPower.Margin = new Thickness(0, currentMargin, 0, 0);
                currentMargin += marginInterval;
                game.shopItemInfo.Children.Add(thrustPower);
            }
            else if (slot.GetRocketComponent() is RocketTank)
            {
                RocketTank component = (RocketTank)slot.GetRocketComponent();
                Label fuelCapacity = new Label();
                Label fuelLevelInfo = new Label();
                fuelCapacity.Content = $"Capacité : {component.FuelCapacity}";
                fuelCapacity.Foreground = Brushes.White;
                fuelCapacity.Margin = new Thickness(0, currentMargin, 0, 0);
                currentMargin += marginInterval;
                fuelLevelInfo.Content = (slot.GetRocketComponent() as RocketTank).FuelAmount > 0 ? "" : "carburant vide";
                game.shopItemInfo.Children.Add(fuelLevelInfo);
                fuelLevelInfo.Foreground = Brushes.White;
                fuelLevelInfo.Margin = new Thickness(0, currentMargin, 0, 0);
                currentMargin += marginInterval;
                game.shopItemInfo.Children.Add(fuelCapacity);
            }
            foreach (UIElement children in game.shopItemInfo.Children)
            {
                children.Visibility = Visibility.Visible;
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
                SetInfo(hoveredSlot);
            } 
            else
            {
                ((UCGame)currentUC).shopItemInfo.Visibility = Visibility.Hidden;
            }

        }

        private void UpdateInvetorySlotHovering()
        {
            Slot hoveredSlot = null;
            if (GetHoveredSlot(out hoveredSlot, slots, ((UCGame)currentUC).gameCanvas))
            {
                if (!hoveredSlot.IsEmpty())
                {

                    SetInfo(hoveredSlot);
                }
            }
            else if (!isShopOpened)
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
            if (isGameFinished)
            {
                return;
            }
            UpdateRocket();
            UpdateDecoration();
            UpdateCurrentStage();
            UpdateCamera();
            UpdateShopHovering();
            UpdateMoney();
            UpdateInvetorySlotHovering();
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

        private void GenerateBaseInventory()
        {
            if (launchCount == 0)
            {
                Image texture = new Image();
                texture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/RocketCapsules/wooden_capsule.png", UriKind.Absolute));
                texture.Width = 32;
                texture.Height = 32;
                Canvas.SetZIndex(texture, 3);
                slots[0].SetComponent(new RocketCapsule("Capsule en bois", WEIGHTS["WOODEN_MAX"], PRICES["WOODEN_MIN"], Utils.GetCopy(texture), ((UCGame)currentUC).gameCanvas, CONTROL_MULTIPLIER["WOODEN"]));
                ((UCGame)currentUC).gameCanvas.Children.Add(slots[0].GetRocketComponent().Texture);
                slots[0].Draw();
                texture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/RocketTanks/wooden_tank.png", UriKind.Absolute));
                slots[1].SetComponent(new RocketTank("Reservoir en bois", WEIGHTS["WOODEN_MAX"], PRICES["WOODEN_MIN"], Utils.GetCopy(texture), ((UCGame)currentUC).gameCanvas, TANK_FUEL_CAPACITY["WOODEN"]));
                ((UCGame)currentUC).gameCanvas.Children.Add(slots[1].GetRocketComponent().Texture);
                slots[1].Draw();
                texture.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Img/RocketEngine/wooden_engine.png", UriKind.Absolute));
                slots[2].SetComponent(new RocketEngine("Moteur en bois", WEIGHTS["WOODEN_MAX"], PRICES["WOODEN_MIN"], Utils.GetCopy(texture), ((UCGame)currentUC).gameCanvas, ENGINE_THRUST_POWER["WOODEN_MAX"]));
                ((UCGame)currentUC).gameCanvas.Children.Add(slots[2].GetRocketComponent().Texture);
                slots[2].Draw();
            }
        }
    }
}