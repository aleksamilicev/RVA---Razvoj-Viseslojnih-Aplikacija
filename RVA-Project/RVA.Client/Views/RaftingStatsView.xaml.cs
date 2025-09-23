using RVA.Client.ViewModels;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RVA.Client.Views
{
    public partial class RaftingStatsView : UserControl
    {
        private Polyline _plannedLine, _boardingLine, _paddlingLine, _restingLine, _finishedLine;

        public RaftingStatsView()
        {
            InitializeComponent();

            var vm = new RaftingStatsViewModel();
            DataContext = vm;

            // Kreiraj linije za crtanje
            _plannedLine = CreateLine(Brushes.Blue);
            _boardingLine = CreateLine(Brushes.Orange);
            _paddlingLine = CreateLine(Brushes.Green);
            _restingLine = CreateLine(Brushes.Purple);
            _finishedLine = CreateLine(Brushes.Red);

            ChartCanvas.Children.Add(_plannedLine);
            ChartCanvas.Children.Add(_boardingLine);
            ChartCanvas.Children.Add(_paddlingLine);
            ChartCanvas.Children.Add(_restingLine);
            ChartCanvas.Children.Add(_finishedLine);

            // Kada se podaci u ViewModel-u promene, osveži Canvas
            vm.DataUpdated += (s, e) => Redraw(vm);
        }

        private Polyline CreateLine(Brush color)
        {
            return new Polyline
            {
                Stroke = color,
                StrokeThickness = 2
            };
        }

        private void Redraw(RaftingStatsViewModel vm)
        {
            ChartCanvas.Children.Clear();
            ChartCanvas.Children.Add(_plannedLine);
            ChartCanvas.Children.Add(_boardingLine);
            ChartCanvas.Children.Add(_paddlingLine);
            ChartCanvas.Children.Add(_restingLine);
            ChartCanvas.Children.Add(_finishedLine);

            double width = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : ChartCanvas.Width;
            double height = ChartCanvas.ActualHeight > 0 ? ChartCanvas.ActualHeight : ChartCanvas.Height;

            if (width <= 0 || height <= 0) return;

            // pretvori podatke u koordinate
            DrawSeries(_plannedLine, vm.PlannedPoints, width, height);
            DrawSeries(_boardingLine, vm.BoardingPoints, width, height);
            DrawSeries(_paddlingLine, vm.PaddlingPoints, width, height);
            DrawSeries(_restingLine, vm.RestingPoints, width, height);
            DrawSeries(_finishedLine, vm.FinishedPoints, width, height);
        }

        private void DrawSeries(Polyline line,
            System.Collections.ObjectModel.ObservableCollection<System.Windows.Point> data,
            double width, double height)
        {
            line.Points.Clear();
            if (data.Count == 0) return;

            double maxX = data[data.Count - 1].X; // zamena za data[^1].X
            double maxY = System.Math.Max(1, data.Max(p => p.Y));

            foreach (var p in data)
            {
                double x = (p.X / maxX) * width;
                double y = height - (p.Y / maxY) * height;
                line.Points.Add(new System.Windows.Point(x, y));
            }
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is RaftingStatsViewModel viewModel)
            {
                viewModel.Cleanup();
            }
        }
    }
}
