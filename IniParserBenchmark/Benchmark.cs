using BenchmarkDotNet.Attributes;

namespace IniParserBenchmark;

[MemoryDiagnoser]
public class Benchmark
{
    [Benchmark]
    public Config? Benchmark1()
    {
        return IniParser.IniReader.Parse<Config>(@"./Test1.ini");
    }

    [Benchmark]
    public async Task<Config?> Benchmark2()
    {
        return await IniParser.IniReader.ParseAsync<Config>(@"./Test1.ini");
    }
}

public class Config
{
    public Test? A { get; set; }
    public Test? B { get; set; }
}

public class Test
{
    public bool MyBool { get; set; }
    public byte MyByte { get; set; }
    public int MyInt { get; set; }
    public long MyLong { get; set; }
    public float MyFloat { get; set; }
    public double MyDouble { get; set; }
    public string MyString { get; set; } = string.Empty;
    public List<string> MyArray { get; set; } = [];
}