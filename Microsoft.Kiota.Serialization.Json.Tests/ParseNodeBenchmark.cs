using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Kiota.Serialization.Json.Tests.Mocks;

namespace Microsoft.Kiota.Serialization.Json.Tests
{
    [SimpleJob(RuntimeMoniker.Net80, baseline: true)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [MemoryDiagnoser]
    [RPlotExporter]
    public class ParseNodeBenchmark
    {
        private JsonParseNodeFactory _jsonParseNodeFactory;
        [GlobalSetup]
        public void Setup()
        {
            _jsonParseNodeFactory = new JsonParseNodeFactory();
        }
        [Benchmark]
        public void GetRootParseNodeBenchmark()
        {
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonParseNodeTests.TestUserJson));
            var rootParseNode = _jsonParseNodeFactory.GetRootParseNode(_jsonParseNodeFactory.ValidContentType, jsonStream);
            var result = rootParseNode.GetObjectValue<TestEntity>(TestEntity.CreateFromDiscriminator);

        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
        }
    }
}
