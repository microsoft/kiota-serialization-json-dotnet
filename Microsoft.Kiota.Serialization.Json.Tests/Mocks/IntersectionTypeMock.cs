using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Json.Tests.Mocks;

public class IntersectionTypeMock : IIntersectionWrapper, IParsable
{
    public TestEntity ComposedType1 { get; set; }
    public SecondTestEntity ComposedType2 { get; set; }
    public string StringValue { get; set; }
    public static IntersectionTypeMock CreateFromDiscriminator(IParseNode parseNode) {
        var result = new IntersectionTypeMock();
        result.ComposedType1 = new();
        result.ComposedType2 = new();
        result.DeserializationHint = $"{nameof(ComposedType1)};{nameof(ComposedType2)};";
        if (parseNode.GetStringValue() is string stringValue) {
            result.StringValue = stringValue;
            result.DeserializationHint = "kiota-deserialization-done";
        }
        return result;
    }
    public string DeserializationHint { get; set; }
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
        return ParseNodeHelper.MergeDeserializersForIntersectionWrapper(ComposedType1, ComposedType2);
    }
    public void Serialize(ISerializationWriter writer) {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        if (!string.IsNullOrEmpty(StringValue)) {
            writer.WriteStringValue(null, StringValue);
        } else {
            writer.WriteObjectValue(null, ComposedType1, ComposedType2);
        }
    }
}