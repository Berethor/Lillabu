using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = LilaApp.Models.Point;
using WinPoint = System.Windows.Point;

namespace Lilabu.Views
{
    using ViewModels;

    public partial class TraceMap : UserControl
    {
        public MainViewModel MainVM;
        public TraceMapViewModel VM;

        public TraceMap()
        {
            InitializeComponent();

            InitializeScrollView();
        }
        private void TraceMap_OnLoaded(object sender, RoutedEventArgs e)
        {
            MainVM = DataContext as MainViewModel;
            VM = MainVM?.TraceMapVm;

            if (VM != null)
            {
                VM.PropertyChanged += VM_PropertyChanged;
            }
        }

        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TraceMapViewModel.Points))
            {
                var points = VM.Points;

                // Поиск минимумов и максимумов
                var maxPoint = points.Length > 1 ? points[0] : new Point(0, 0);
                var minPoint = points.Length > 1 ? points[0] : new Point(0, 0);
                foreach (var point in points)
                {
                    if (point.X < minPoint.X) minPoint.X = point.X;
                    if (point.Y < minPoint.Y) minPoint.Y = point.Y;
                    if (point.X > maxPoint.X) maxPoint.X = point.X;
                    if (point.Y > maxPoint.Y) maxPoint.Y = point.Y;
                }

                const int padding = 10;
                const int multiplier = 10;

                VM.Width = (maxPoint.X - minPoint.X) * multiplier + 2 * padding + 2;
                VM.Height = (maxPoint.Y - minPoint.Y) * multiplier + 2 * padding + 2;

                grid_MapOuter.Width = VM.Width * 1.2;
                grid_MapOuter.Height = VM.Height * 1.2;

                grid_Map.Children.Clear();

                void AddLine(Point p1, Point p2, Brush color, double thickness = 1)
                {
                    grid_Map.Children.Add(new Line
                    {
                        X1 = padding + multiplier * (p1.X - minPoint.X),
                        X2 = padding + multiplier * (p2.X - minPoint.X),
                        Y1 = padding + multiplier * (p1.Y - minPoint.Y),
                        Y2 = padding + multiplier * (p2.Y - minPoint.Y),
                        StrokeThickness = thickness,
                        Stroke = color,
                    });
                }

                void AddArc(Point p1, Point p2, int direction)
                {
                    const double radius = 3;

                    var x1 = padding + multiplier * (p1.X - minPoint.X);
                    var x2 = padding + multiplier * (p2.X - minPoint.X);
                    var y1 = padding + multiplier * (p1.Y - minPoint.Y);
                    var y2 = padding + multiplier * (p2.Y - minPoint.Y);

                    var arc = new ArcSegment()
                    {
                        Point = new WinPoint(x2, y2),
                        Size = new Size(radius * multiplier, radius * multiplier),
                        SweepDirection = direction == 1 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise,
                    };
                    var figure = new PathFigure() { StartPoint = new WinPoint(x1, y1) };
                    figure.Segments.Add(arc);
                    var geometry = new PathGeometry();
                    geometry.Figures.Add(figure);
                    var path = new Path() { Data = geometry, Stroke = Brushes.Red, StrokeThickness = 1 };
                    grid_Map.Children.Add(path);
                }

                void AddEllipse(Point p1)
                {
                    var x1 = padding + multiplier * (p1.X - minPoint.X);
                    var y1 = padding + multiplier * (p1.Y - minPoint.Y);

                    var size = 0.3 * multiplier;

                    grid_Map.Children.Add(new Ellipse()
                    {
                        Width = size,
                        Height = size,
                        Fill = Brushes.Blue,
                        Margin = new Thickness(x1 - size / 2, y1 - size / 2, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                    });
                }

                // Сетка
                for (var i = (int)minPoint.X - 1; i <= Math.Ceiling(maxPoint.X) + 1; i++)
                    AddLine(new Point(i, minPoint.Y - 1), new Point(i, maxPoint.Y + 1), Brushes.DarkGray, 0.25);
                for (var i = (int)minPoint.Y - 1; i <= Math.Ceiling(maxPoint.Y) + 1; i++)
                    AddLine(new Point(minPoint.X - 1, i), new Point(maxPoint.X + 1, i), Brushes.DarkGray, 0.25);

                // Ось OY
                AddLine(new Point(0, 0), new Point(0, 0.95), Brushes.Black);
                AddLine(new Point(0.2, 0.8), new Point(-0.0333, 1), Brushes.Black);
                AddLine(new Point(-0.2, 0.8), new Point(0.0333, 1), Brushes.Black);

                // Трасса
                for (var i = 0; i < points.Length - 1 && i < MainVM.Model.Order.Count; i++)
                {
                    if (MainVM.Model.Order[i].StartsWith("T"))
                    {
                        // Рисуем дугу
                        var direction = MainVM.Model.Topology[i].Direction;
                        AddArc(points[i], points[i + 1], direction);
                    }
                    else
                    {
                        // Рисуем линию
                        AddLine(points[i], points[i + 1], Brushes.Red);
                    }
                }

                // Точки маршрута

                foreach (var route in MainVM.Model.Points)
                {
                    AddEllipse(route);
                }
            }
        }

        #region Zoom and move scrollview

        WinPoint? lastCenterPositionOnTarget;
        WinPoint? lastMousePositionOnTarget;
        WinPoint? lastDragPoint;

        void InitializeScrollView()
        {
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;

            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;

            slider.ValueChanged += OnSliderValueChanged;
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                var posNow = e.GetPosition(scrollViewer);

                var dX = posNow.X - lastDragPoint.Value.X;
                var dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            // make sure we still can use the scrollbars
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight)
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(grid);

            if (e.Delta > 0)
            {
                slider.Value += 1;
            }
            if (e.Delta < 0)
            {
                slider.Value -= 1;
            }

            e.Handled = true;
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Hand;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new WinPoint(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                WinPoint? targetBefore = null;
                WinPoint? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new WinPoint(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        var centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, grid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(grid);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    var dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    var dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    var multiplicatorX = e.ExtentWidth / grid.Width;
                    var multiplicatorY = e.ExtentHeight / grid.Height;

                    var newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    var newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        #endregion

    }
}
