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
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("{\"@odata.type\":\"#microsoft.graph.testEntity\",\"officeLocation\":\"Montreal\", \"id\": \"opaque\"}"));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.StringValue);
        Assert.StartsWith("ComposedType1", result.DeserializationHint);
        Assert.DoesNotContain("ComposedType2", result.DeserializationHint);
        Assert.DoesNotContain("kiota-deserialization-done", result.DeserializationHint);
        Assert.Equal("opaque", result.ComposedType1.Id);
    }
    [Fact]
    public void ParsesUnionTypeComplexProperty2()
    {
        // Given
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("{\"@odata.type\":\"#microsoft.graph.secondTestEntity\",\"officeLocation\":\"Montreal\", \"id\": 10}"));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Null(result.StringValue);
        Assert.StartsWith("ComposedType2", result.DeserializationHint);
        Assert.DoesNotContain("ComposedType1", result.DeserializationHint);
        Assert.DoesNotContain("kiota-deserialization-done", result.DeserializationHint);
        Assert.Equal(10, result.ComposedType2.Id);
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
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Equal("officeLocation", result.StringValue);
        Assert.DoesNotContain("ComposedType1", result.DeserializationHint);
        Assert.DoesNotContain("ComposedType2", result.DeserializationHint);
        Assert.StartsWith("kiota-deserialization-done", result.DeserializationHint);
    }
}