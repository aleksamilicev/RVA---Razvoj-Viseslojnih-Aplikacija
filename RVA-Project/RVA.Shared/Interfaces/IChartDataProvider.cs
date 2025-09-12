using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    /// Interfejs za podatke potrebne za LiveCharts2
    public interface IChartDataProvider
    {
        // State distribution data
        Dictionary<RaftingState, int> GetStateDistribution();

        // Real-time data
        event EventHandler<ChartDataChangedEventArgs> DataChanged;

        // Historical data
        IEnumerable<ChartDataPoint> GetHistoricalData(DateTime from, DateTime to);

        // Current data
        ChartDataPoint GetCurrentData();
    }

    /// Podatak za chart
    public class ChartDataPoint
    {
        public DateTime Timestamp { get; set; }
        public RaftingState State { get; set; }
        public int Count { get; set; }
        public double AverageIntensity { get; set; }
        public double AverageSpeed { get; set; }
    }

    /// <summary>
    /// Event argumenti za promene chart podataka
    /// </summary>
    public class ChartDataChangedEventArgs : EventArgs
    {
        public Dictionary<RaftingState, int> NewDistribution { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        public ChartDataChangedEventArgs(Dictionary<RaftingState, int> distribution)
        {
            NewDistribution = distribution;
        }
    }
}
