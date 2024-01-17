﻿
using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Json.Tests.Mocks;

public class UntypedTestEntity : IParsable, IAdditionalDataHolder
{
    /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
    public IDictionary<string, object> AdditionalData { get; set; }
    public string Id { get; set; }
    public string Title { get; set; }
    public UntypedNode Location { get; set; }
    public UntypedNode Keywords { get; set; }
    public UntypedNode Detail { get; set; }
    public UntypedTestEntity()
    {
        AdditionalData = new Dictionary<string, object>();
    }

    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        return new Dictionary<string, Action<IParseNode>>
        {
            { "id", node => Id = node.GetStringValue() },
            { "title", node => Title = node.GetStringValue() },
            { "location", node => Location = node.GetObjectValue(UntypedNode.CreateFromDiscriminator) },
            { "keywords", node => Keywords = node.GetObjectValue(UntypedNode.CreateFromDiscriminator) },
            { "detail", node => Detail = node.GetObjectValue(UntypedNode.CreateFromDiscriminator) }
        };
    }
    public void Serialize(ISerializationWriter writer)
    {
        writer.WriteStringValue("id", Id);
        writer.WriteStringValue("title", Title);
        writer.WriteObjectValue("location", Location);
        writer.WriteObjectValue("keywords", Keywords);
        writer.WriteObjectValue("detail", Detail);
        writer.WriteAdditionalData(AdditionalData);
    }
    public static UntypedTestEntity CreateFromDiscriminator(IParseNode parseNode)
    {
        var discriminatorValue = parseNode.GetChildNode("@odata.type")?.GetStringValue();
        return discriminatorValue switch
        {
            _ => new UntypedTestEntity(),
        };
    }
}