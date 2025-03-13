using Reflex.Benchmark.NestedModel;
using Reflex.Benchmark.Utilities;
using Reflex.Core;

namespace Reflex.Benchmark
{
    internal class NestedBenchmarkReflex : MonoProfiler
    {
        private Container _container;

        private void Start()
        {
            _container = new ContainerBuilder()
                .AddTransient(typeof(A), new[] { typeof(IA) })
                .AddTransient(typeof(B), new[] { typeof(IB) })
                .AddTransient(typeof(C), new[] { typeof(IC) })
                .AddTransient(typeof(D), new[] { typeof(ID) })
                .AddTransient(typeof(E), new[] { typeof(IE) })
                .Build();
        }

        protected override void Sample()
        {
            _container.Resolve<IA>();
        }
    }
}