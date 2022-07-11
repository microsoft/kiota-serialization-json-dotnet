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
        if (parseNode.GetStringValue() is string stringValue) {
            result.StringValue = stringValue;
        }
        return result;
    }
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
        if(!string.IsNullOrEmpty(StringValue)) {
            return new Dictionary<string, Action<IParseNode>>();
        } else {
            return ParseNodeHelper.MergeDeserializersForIntersectionWrapper(ComposedType1, ComposedType2);
        }
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