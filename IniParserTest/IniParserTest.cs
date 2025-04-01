using IniParser;

namespace IniParserTest;

public class IniParserTest
{
    [Fact]
    public void Parsing_Succeeds()
    {
        var result = IniReader.Parse<Config>(@"./Test1.ini");

        Assert.NotNull(result);
        Assert.NotNull(result.A);
        Assert.True(result.A.MyBool);
        Assert.Equal(8, result.A.MyByte);
        Assert.Equal(1, result.A.MyInt);
        Assert.Equal(9000, result.A.MyLong);
        Assert.Equal(3.14, result.A.MyFloat, 0.01);
        Assert.Equal(3.1415926, result.A.MyDouble, 0.0000001);
        Assert.Equal("Hello Good Sir", result.A.MyString);
        Assert.Collection(result.A.MyArray,
            item => Assert.Equal("Hi", item),
            item => Assert.Equal("Good Morning", item),
            item => Assert.Equal("Good Bye", item));

        Assert.NotNull(result.B);
        Assert.False(result.B.MyBool);
        Assert.Equal(10, result.B.MyByte);
        Assert.Equal(12, result.B.MyInt);
        Assert.Equal(8000, result.B.MyLong);
        Assert.Equal(6.28, result.B.MyFloat, 0.01);
        Assert.Equal(6.2866545, result.B.MyDouble, 0.0000001);
        Assert.Equal("Hello Good Sir!", result.B.MyString);
        Assert.Collection(result.B.MyArray,
            item => Assert.Equal("Hi!", item),
            item => Assert.Equal("Good Morning!", item),
            item => Assert.Equal("Good Bye!", item));
    }

    [Fact]
    public void WrongLocation_Returns_Null()
    {
        var result = IniReader.Parse<Config>("");

        Assert.Null(result);
    }

    [Fact]
    public void WrongFile_Returns_Null()
    {
        var result = IniReader.Parse<Config>(@"./TestNone.ini");

        Assert.Null(result);
    }

    [Fact]
    public void MissingHeader_Returns_Null()
    {
        var result = IniReader.Parse<Config>(@"./Test2.ini");
        Assert.NotNull(result);
        Assert.Null(result.B);
    }

    [Fact]
    public void IncorrectProperty_Returns_Nothing()
    {
        var result = IniReader.Parse<Config>(@"./Test2.ini");
        Assert.NotNull(result);
        Assert.NotNull(result.A);
    }
}


class Config
{
    public Test? A { get; set; }
    public Test? B { get; set; }
}

class Test
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