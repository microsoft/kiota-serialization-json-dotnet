using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Json.Tests.Mocks;

public class UnionTypeMock : IUnionWrapper, IParsable
{
    public TestEntity ComposedType1 { get; set; }
    public SecondTestEntity ComposedType2 { get; set; }
    public string StringValue { get; set; }
    public static UnionTypeMock CreateFromDiscriminator(IParseNode parseNode) {
        var result = new UnionTypeMock();
        var discriminator = parseNode.GetChildNode("@odata.type")?.GetStringValue();
        if("#microsoft.graph.testEntity".Equals(discriminator)) {
            result.ComposedType1 = new();
            result.DeserializationHint = "ComposedType1";
        }
        else if("#microsoft.graph.secondTestEntity".Equals(discriminator)) {
            result.ComposedType2 = new();
            result.DeserializationHint = "ComposedType2";
        }
        else if (parseNode.GetStringValue() is string stringValue) {
            result.StringValue = stringValue;
            result.DeserializationHint = "kiota-deserialization-done";
        }
        return result;
    }
    public string DeserializationHint { get; set; }
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => throw new NotImplementedException();
    public void Serialize(ISerializationWriter writer) => throw new NotImplementedException();
}