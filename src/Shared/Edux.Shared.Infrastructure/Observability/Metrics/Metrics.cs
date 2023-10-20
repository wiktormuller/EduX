using System.Diagnostics.Metrics;

namespace Edux.Shared.Infrastructure.Observability.Metrics
{
    internal sealed class Metrics : IMetrics
    {
        private readonly string _name;
        private readonly Meter _meter;

        public Metrics(string name)
        {
            _name = name;
            _meter = new Meter(name);
        }

        public Counter<T> Counter<T>(string name) where T : struct
        {
            return _meter.CreateCounter<T>(Key(name));
        }

        public Histogram<T> Histogram<T>(string name) where T : struct
        {
            return _meter.CreateHistogram<T>(Key(name));
        }

        public ObservableGauge<T> ObservableGauge<T>(string name, Func<Measurement<T>> measurement) where T : struct
        {
            return _meter.CreateObservableGauge(Key(name), measurement);
        }

        private string Key(string key) => $"{_name}_{key}";
    }
}
