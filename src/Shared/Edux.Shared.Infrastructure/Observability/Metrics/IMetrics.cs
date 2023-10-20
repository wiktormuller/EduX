using System.Diagnostics.Metrics;

namespace Edux.Shared.Infrastructure.Observability.Metrics
{
    internal interface IMetrics
    {
        Counter<T> Counter<T>(string name) where T : struct;
        Histogram<T> Histogram<T>(string name) where T : struct;
        ObservableGauge<T> ObservableGauge<T>(string name, Func<Measurement<T>> measurement) where T : struct;
    }
}
