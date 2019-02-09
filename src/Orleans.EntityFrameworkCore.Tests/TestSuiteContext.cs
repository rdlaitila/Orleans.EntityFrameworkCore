using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace Orleans.EntityFrameworkCore.Tests
{
    public class TestSuiteContext
    {
        public ISiloHost SiloHost1 { get; set; }

        public ISiloHost SiloHost2 { get; set; }

        public ISiloHost SiloHost3 { get; set; }

        public IClusterClient SiloClient { get; set; }

        public int SiloPortStart { get; set; }

        public Action<IServiceCollection> ConfigureServicesCallback { get; set; }
    }
}