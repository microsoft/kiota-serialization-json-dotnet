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
        }
        else if("#microsoft.graph.secondTestEntity".Equals(discriminator)) {
            result.ComposedType2 = new();
        }
        else if (parseNode.GetStringValue() is string stringValue) {
            result.StringValue = stringValue;
        }
        return result;
    }
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
        if (ComposedType1 != null)
            return ComposedType1.GetFieldDeserializers();
        else if (ComposedType2 != null)
            return ComposedType2.GetFieldDeserializers();
        else
            return new Dictionary<string, Action<IParseNode>>();
    }
    public void Serialize(ISerializationWriter writer) {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        if (ComposedType1 != null) {
            writer.WriteObjectValue(null, ComposedType1);
        }
        else if (ComposedType2 != null) {
            writer.WriteObjectValue(null, ComposedType2);
        }
        else if (!string.IsNullOrEmpty(StringValue)) {
            writer.WriteStringValue(null, StringValue);
        }
    }
}