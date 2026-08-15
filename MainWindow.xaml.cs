using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using System.IO;
using System.Text.Json;
using LuckyDangle.UI.About;
using LuckyDangle.UI.Support;

using LuckyDangle.Dangles;

namespace LuckyDangle;

public partial class MainWindow : Window
{
    // =====================================================
    // WHOLE WINDOW MOVEMENT
    // =====================================================

    private bool movingWindow;
    private IDangle currentDangle;

    private Point windowDragStart;

    private double windowStartLeft;
    private double windowStartTop;


    // =====================================================
    // CHARM MOVEMENT
    // =====================================================

    private bool draggingCharm;

    private Point charmDragStart;

    private double currentX;
    private double currentY;

    private double velocityX;
    private double velocityY;


    // =====================================================
    // PHYSICS
    // =====================================================

    private readonly DispatcherTimer physicsTimer;


    private static readonly string PositionFile =
    System.IO.Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData),
        "LuckyDangle",
        "window-position.json");

    private class WindowPosition
    {
        public double Left { get; set; }
        public double Top { get; set; }

        public string? DangleId { get; set; }
    }


    // =====================================================
    // FIXED COORDINATES INSIDE THE WINDOW
    // =====================================================

    private const double AnchorX = 160;
    private const double AnchorY = 8;

    private const double RestCharmX = 115;
    private const double RestCharmY = 82;


    public MainWindow()
    {
        InitializeComponent();
        currentDangle = DangleCatalog.GetAll()[0];

        physicsTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        physicsTimer.Tick += PhysicsTimer_Tick;
    }


    // =====================================================
    // INITIAL POSITION
    // =====================================================

    private void Window_Loaded(
    object sender,
    RoutedEventArgs e)
    {
        LoadWindowPosition();

        BuildCharm(currentDangle);

        SetCharmPosition(0, 0);
    }


    // =====================================================
    // MOVE WHOLE DANGLER
    // =====================================================

    private void MoveHandle_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        movingWindow = true;

        windowDragStart = e.GetPosition(this);

        windowStartLeft = Left;
        windowStartTop = Top;

        MoveHandle.CaptureMouse();

        e.Handled = true;
    }


    private void MoveHandle_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!movingWindow)
            return;

        Point current = e.GetPosition(this);

        double dx = current.X - windowDragStart.X;
        double dy = current.Y - windowDragStart.Y;

        Left = windowStartLeft + dx;
        Top = windowStartTop + dy;

        e.Handled = true;
    }


    private void MoveHandle_MouseLeftButtonUp(
    object sender,
    MouseButtonEventArgs e)
    {
        if (!movingWindow)
            return;

        movingWindow = false;

        MoveHandle.ReleaseMouseCapture();

        SaveWindowPosition();

        e.Handled = true;
    }


    // =====================================================
    // START PULLING CHARM
    // =====================================================

    private void Charm_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        draggingCharm = true;

        physicsTimer.Stop();

        charmDragStart = e.GetPosition(this);

        CharmCanvas.CaptureMouse();

        e.Handled = true;
    }


    // =====================================================
    // PULL CHARM
    // =====================================================

    private void Charm_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!draggingCharm)
            return;

        Point current = e.GetPosition(this);

        double dx = current.X - charmDragStart.X;
        double dy = current.Y - charmDragStart.Y;

        // Maximum pull distance.
        currentX = Math.Clamp(dx, -90, 90);
        currentY = Math.Clamp(dy, -80, 100);

        SetCharmPosition(currentX, currentY);

        e.Handled = true;
    }


    // =====================================================
    // RELEASE CHARM
    // =====================================================

    private void Charm_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        if (!draggingCharm)
            return;

        draggingCharm = false;

        CharmCanvas.ReleaseMouseCapture();

        // Give the spring some momentum.
        velocityX = currentX * 0.045;
        velocityY = currentY * 0.045;

        physicsTimer.Start();

        e.Handled = true;
    }


    // =====================================================
    // UPDATE CHARM + STRING
    // =====================================================

    private void SetCharmPosition(
        double x,
        double y)
    {
        // ---------------------------------------------
        // Move charm
        // ---------------------------------------------

        double charmRestX =
    AnchorX - currentDangle.HangPointX;

        Canvas.SetLeft(
            CharmCanvas,
            charmRestX + x);

        Canvas.SetTop(
            CharmCanvas,
            RestCharmY + y);


        // ---------------------------------------------
        // Move bead
        // ---------------------------------------------

        Canvas.SetLeft(
            DangleBead,
            AnchorX - 8 + x);

        Canvas.SetTop(
            DangleBead,
            74 + y);


        // ---------------------------------------------
        // Stretch / angle string
        // ---------------------------------------------

        DangleString.X1 = AnchorX;
        DangleString.Y1 = AnchorY;

        DangleString.X2 = AnchorX + x;
        DangleString.Y2 = 82 + y;
    }


    // =====================================================
    // SPRING PHYSICS
    // =====================================================

    private void PhysicsTimer_Tick(
        object? sender,
        EventArgs e)
    {
        /*
         * Spring system.
         *
         * Increase springStrength:
         *   stronger / faster return
         *
         * Increase damping:
         *   less bouncing
         */

        const double springStrength = 0.115;
        const double damping = 0.82;


        // ---------------------------------------------
        // Horizontal spring
        // ---------------------------------------------

        velocityX += -currentX * springStrength;

        velocityX *= damping;

        currentX += velocityX;


        // ---------------------------------------------
        // Vertical spring
        // ---------------------------------------------

        velocityY += -currentY * springStrength;

        velocityY *= damping;

        currentY += velocityY;


        SetCharmPosition(
            currentX,
            currentY);


        // ---------------------------------------------
        // Stop when settled
        // ---------------------------------------------

        if (
            Math.Abs(currentX) < 0.08 &&
            Math.Abs(currentY) < 0.08 &&
            Math.Abs(velocityX) < 0.08 &&
            Math.Abs(velocityY) < 0.08)
        {
            currentX = 0;
            currentY = 0;

            velocityX = 0;
            velocityY = 0;

            SetCharmPosition(0, 0);

            physicsTimer.Stop();
        }
    }


    // =====================================================
    // RIGHT CLICK
    // =====================================================

    private void Charm_MouseRightButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        Menu.IsOpen = true;

        e.Handled = true;
    }


    // =====================================================
    // MANUAL SWING
    // =====================================================

    private void Swing_Click(
        object sender,
        RoutedEventArgs e)
    {
        currentX = 30;
        currentY = 5;

        velocityX = 2.2;
        velocityY = 0;

        physicsTimer.Start();
    }


    // =====================================================
    // RANDOM DANGLER
    // =====================================================

    private void RandomDangle_Click(
    object sender,
    RoutedEventArgs e)
    {
        var dangles =
            DangleFactory.GetAvailableDangles();

        if (dangles.Count <= 1)
            return;

        var available =
            dangles
                .Where(d => d != currentDangle)
                .ToList();

        var randomDangle =
            available[
                Random.Shared.Next(
                    available.Count)];

        currentDangle =
            randomDangle;

        SaveWindowPosition();

        BuildCharm(currentDangle);

        currentX = 0;
        currentY = 0;
        velocityX = 0;
        velocityY = 0;

        SetCharmPosition(0, 0);

        Swing_Click(sender, e);
    }

    // =====================================================
    // CHANGE DANGLER
    // =====================================================

    private void ChangeDangle_Click(
    object sender,
    RoutedEventArgs e)
    {
        var picker =
            new CharmPickerWindow(currentDangle)
            {
                Owner = this,
                WindowStartupLocation =
                    WindowStartupLocation.CenterScreen
            };

        if (picker.ShowDialog() == true)
        {
            currentDangle =
                picker.SelectedDangle;

            SaveWindowPosition();

            BuildCharm(currentDangle);

            currentX = 0;
            currentY = 0;
            velocityX = 0;
            velocityY = 0;

            SetCharmPosition(0, 0);
        }
    }


    private void AboutLuckyDangle_Click(
    object sender,
    RoutedEventArgs e)
    {
        var aboutWindow =
            new AboutWindow
            {
                Owner = this
            };

        aboutWindow.ShowDialog();
    }

    private void SupportLuckyCharm_Click(
    object sender,
    RoutedEventArgs e)
    {
        var supportWindow =
            new SupportWindow
            {
                Owner = this,
                WindowStartupLocation =
                    WindowStartupLocation.CenterScreen
            };

        supportWindow.ShowDialog();
    }

    // =====================================================
    // EXIT
    // =====================================================

    private void Exit_Click(
        object sender,
        RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }



    private void BuildCharm(IDangle dangle)
    {
        dangle.Render(CharmCanvas);
    }

    // =====================================================
    // WINDOW POSITION PERSISTENCE
    // =====================================================

    private void SaveWindowPosition()
    {
        try
        {
            string folder = System.IO.Path.GetDirectoryName(PositionFile)!;

            Directory.CreateDirectory(folder);

            var position =
    new WindowPosition
    {
        Left = Left,
        Top = Top,
        DangleId = currentDangle.Id
    };

            File.WriteAllText(
                PositionFile,
                JsonSerializer.Serialize(position));
        }
        catch
        {
            // Position saving should never crash the app.
        }
    }


    private void LoadWindowPosition()
    {
        try
        {
            if (File.Exists(PositionFile))
            {
                var json =
                    File.ReadAllText(
                        PositionFile);

                var position =
                    JsonSerializer.Deserialize<WindowPosition>(
                        json);

                if (position != null)
                {
                    Left = position.Left;
                    Top = position.Top;

                    if (!string.IsNullOrWhiteSpace(position.DangleId))
                    {
                        var savedDangle =
                            DangleCatalog.GetAll()
                                .FirstOrDefault(
                                    d => d.Id == position.DangleId);

                        if (savedDangle != null)
                            currentDangle = savedDangle;
                    }

                    KeepWindowOnScreen();

                    return;
                }
            }
        }
        catch
        {
            // Ignore corrupted/missing position data.
        }


        // -------------------------------------------------
        // First launch → default top-right
        // -------------------------------------------------

        var workArea =
            SystemParameters.WorkArea;

        Left =
            workArea.Right
            - Width
            - 15;

        Top =
            workArea.Top
            + 5;
    }


    private void KeepWindowOnScreen()
    {
        double minLeft =
            SystemParameters.VirtualScreenLeft;

        double minTop =
            SystemParameters.VirtualScreenTop;

        double maxLeft =
            SystemParameters.VirtualScreenLeft
            + SystemParameters.VirtualScreenWidth
            - Width;

        double maxTop =
            SystemParameters.VirtualScreenTop
            + SystemParameters.VirtualScreenHeight
            - Height;

        Left =
            Math.Clamp(
                Left,
                minLeft,
                maxLeft);

        Top =
            Math.Clamp(
                Top,
                minTop,
                maxTop);
    }

    // =====================================================
    // WINDOW CLOSING
    // =====================================================

    private void Window_Closing(
        object? sender,
        System.ComponentModel.CancelEventArgs e)
    {
        physicsTimer.Stop();

        SaveWindowPosition();
    }
}