using System.IO;
using System.Text;
using Microsoft.Kiota.Serialization.Json.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Json.Tests;

public class UnionWrapperParseTests {
    private readonly JsonParseNodeFactory _parseNodeFactory = new();
    private const string contentType = "application/json";
    [Fact]
    public void ParsesUnionTypeComplexProperty1()
    {
        // Given
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("{\"displayName\":\"McGill\",\"officeLocation\":\"Montreal\", \"id\": \"opaque\"}"));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.NotNull(result.ComposedType2);
        Assert.Null(result.StringValue);
        Assert.Equal("ComposedType1;ComposedType2;", result.DeserializationHint);
        Assert.Equal("opaque", result.ComposedType1.Id);
        Assert.Equal("McGill", result.ComposedType2.DisplayName);
    }
    [Fact]
    public void ParsesUnionTypeComplexProperty2()
    {
        // Given
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("{\"displayName\":\"McGill\",\"officeLocation\":\"Montreal\", \"id\": 10}"));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.NotNull(result.ComposedType2);
        Assert.Null(result.StringValue);
        Assert.Equal("ComposedType1;ComposedType2;", result.DeserializationHint);
        Assert.Null(result.ComposedType1.Id);
        Assert.Null(result.ComposedType2.Id); // it's expected to be null since we have conflicting properties here and the parser will only try one to avoid having to brute its way through
        Assert.Equal("McGill", result.ComposedType2.DisplayName);
    }
    [Fact]
    public void ParsesUnionTypeStringValue()
    {
        // Given
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("\"officeLocation\""));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType2);
        Assert.NotNull(result.ComposedType1);
        Assert.Equal("officeLocation", result.StringValue);
        Assert.DoesNotContain("ComposedType1", result.DeserializationHint);
        Assert.DoesNotContain("ComposedType2", result.DeserializationHint);
        Assert.StartsWith("kiota-deserialization-done", result.DeserializationHint);
    }
}