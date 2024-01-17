// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using System.Xml;
using Microsoft.Kiota.Abstractions.Extensions;

#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Serialization.Json
{
    /// <summary>
    /// The <see cref="IParseNode"/> implementation for the json content type
    /// </summary>
    public class JsonParseNode : IParseNode
    {
        private readonly JsonElement _jsonNode;
        /// <summary>
        /// The <see cref="JsonParseNode"/> constructor.
        /// </summary>
        /// <param name="node">The JsonElement to initialize the node with</param>
        public JsonParseNode(JsonElement node)
        {
            _jsonNode = node;
        }

        /// <summary>
        /// Get the string value from the json node
        /// </summary>
        /// <returns>A string value</returns>
        public string? GetStringValue() => _jsonNode.ValueKind == JsonValueKind.String ? _jsonNode.GetString() : null;

        /// <summary>
        /// Get the boolean value from the json node
        /// </summary>
        /// <returns>A boolean value</returns>
        public bool? GetBoolValue() => _jsonNode.ValueKind == JsonValueKind.True || _jsonNode.ValueKind == JsonValueKind.False ? _jsonNode.GetBoolean() : null;

        /// <summary>
        /// Get the byte value from the json node
        /// </summary>
        /// <returns>A byte value</returns>
        public byte? GetByteValue() => _jsonNode.ValueKind == JsonValueKind.Number ? _jsonNode.GetByte() : null;

        /// <summary>
        /// Get the sbyte value from the json node
        /// </summary>
        /// <returns>A sbyte value</returns>
        public sbyte? GetSbyteValue() => _jsonNode.ValueKind == JsonValueKind.Number ? _jsonNode.GetSByte() : null;

        /// <summary>
        /// Get the int value from the json node
        /// </summary>
        /// <returns>A int value</returns>
        public int? GetIntValue() => _jsonNode.ValueKind == JsonValueKind.Number ? _jsonNode.GetInt32() : null;

        /// <summary>
        /// Get the float value from the json node
        /// </summary>
        /// <returns>A float value</returns>
        public float? GetFloatValue() => _jsonNode.ValueKind == JsonValueKind.Number ? _jsonNode.GetSingle() : null;

        /// <summary>
        /// Get the Long value from the json node
        /// </summary>
        /// <returns>A Long value</returns>
        public long? GetLongValue() => _jsonNode.ValueKind == JsonValueKind.Number ? _jsonNode.GetInt64() : null;

        /// <summary>
        /// Get the double value from the json node
        /// </summary>
        /// <returns>A double value</returns>
        public double? GetDoubleValue() => _jsonNode.ValueKind == JsonValueKind.Number ? _jsonNode.GetDouble() : null;

        /// <summary>
        /// Get the decimal value from the json node
        /// </summary>
        /// <returns>A decimal value</returns>
        public decimal? GetDecimalValue() => _jsonNode.ValueKind == JsonValueKind.Number ? _jsonNode.GetDecimal() : null;

        /// <summary>
        /// Get the guid value from the json node
        /// </summary>
        /// <returns>A guid value</returns>
        public Guid? GetGuidValue() => _jsonNode.ValueKind == JsonValueKind.String ? _jsonNode.GetGuid() : null;

        /// <summary>
        /// Get the <see cref="DateTimeOffset"/> value from the json node
        /// </summary>
        /// <returns>A <see cref="DateTimeOffset"/> value</returns>
        public DateTimeOffset? GetDateTimeOffsetValue()
        {
            // JsonElement.GetDateTimeOffset is super strict so try to be more lenient if it fails(e.g. when we have whitespace or other variant formats).
            // ref - https://docs.microsoft.com/en-us/dotnet/standard/datetime/system-text-json-support
            if(!_jsonNode.TryGetDateTimeOffset(out var value))
                value = DateTimeOffset.Parse(_jsonNode.GetString()!);

            return value;
        }

        /// <summary>
        /// Get the <see cref="TimeSpan"/> value from the json node
        /// </summary>
        /// <returns>A <see cref="TimeSpan"/> value</returns>
        public TimeSpan? GetTimeSpanValue()
        {
            var jsonString = _jsonNode.GetString();
            if(string.IsNullOrEmpty(jsonString))
                return null;

            // Parse an ISO8601 duration.http://en.wikipedia.org/wiki/ISO_8601#Durations to a TimeSpan
            return XmlConvert.ToTimeSpan(jsonString);
        }

        /// <summary>
        /// Get the <see cref="Date"/> value from the json node
        /// </summary>
        /// <returns>A <see cref="Date"/> value</returns>
        public Date? GetDateValue()
        {
            var dateString = _jsonNode.GetString();
            if(!DateTime.TryParse(dateString,out var result))
                return null;

            return new Date(result);
        }

        /// <summary>
        /// Get the <see cref="Time"/> value from the json node
        /// </summary>
        /// <returns>A <see cref="Time"/> value</returns>
        public Time? GetTimeValue()
        {
            var dateString = _jsonNode.GetString();
            if(!DateTime.TryParse(dateString,out var result))
                return null;

            return new Time(result);
        }

        /// <summary>
        /// Get the enumeration value of type <typeparam name="T"/>from the json node
        /// </summary>
        /// <returns>An enumeration value or null</returns>
#if NET5_0_OR_GREATER
        public T? GetEnumValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : struct, Enum
#else
        public T? GetEnumValue<T>() where T : struct, Enum
#endif
        {
            var rawValue = _jsonNode.GetString();
            if(string.IsNullOrEmpty(rawValue)) return null;
            
            var type = typeof(T);
            rawValue = ToEnumRawName<T>(rawValue!);
            if(type.GetCustomAttributes<FlagsAttribute>().Any())
            {
                return (T)(object)rawValue!
                    .Split(',')
                    .Select(x => Enum.TryParse<T>(x, true, out var result) ? result : (T?)null)
                    .Where(x => !x.Equals(null))
                    .Select(x => (int)(object)x!)
                    .Sum();
            }
            else
                return Enum.TryParse<T>(rawValue, true,out var result) ? result : null;
        }

        /// <summary>
        /// Get the collection of type <typeparam name="T"/>from the json node
        /// </summary>
        /// <param name="factory">The factory to use to create the model object.</param>
        /// <returns>A collection of objects</returns>
        public IEnumerable<T> GetCollectionOfObjectValues<T>(ParsableFactory<T> factory) where T : IParsable
        {
            if (_jsonNode.ValueKind == JsonValueKind.Array) {
                var enumerator = _jsonNode.EnumerateArray();
                while(enumerator.MoveNext())
                {
                    var currentParseNode = new JsonParseNode(enumerator.Current)
                    {
                        OnAfterAssignFieldValues = OnAfterAssignFieldValues,
                        OnBeforeAssignFieldValues = OnBeforeAssignFieldValues
                    };
                    yield return currentParseNode.GetObjectValue<T>(factory);
                }
            }
        }
        /// <summary>
        /// Gets the collection of enum values of the node.
        /// </summary>
        /// <returns>The collection of enum values.</returns>
#if NET5_0_OR_GREATER
        public IEnumerable<T?> GetCollectionOfEnumValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]T>() where T : struct, Enum
#else
        public IEnumerable<T?> GetCollectionOfEnumValues<T>() where T : struct, Enum
#endif
        {
            if (_jsonNode.ValueKind == JsonValueKind.Array) {
                var enumerator = _jsonNode.EnumerateArray();
                while(enumerator.MoveNext())
                {
                    var currentParseNode = new JsonParseNode(enumerator.Current)
                    {
                        OnAfterAssignFieldValues = OnAfterAssignFieldValues,
                        OnBeforeAssignFieldValues = OnBeforeAssignFieldValues
                    };
                    yield return currentParseNode.GetEnumValue<T>();
                }
            }
        }
        /// <summary>
        /// Gets the byte array value of the node.
        /// </summary>
        /// <returns>The byte array value of the node.</returns>
        public byte[]? GetByteArrayValue() {
            var rawValue = _jsonNode.GetString();
            if(string.IsNullOrEmpty(rawValue)) return null;
            return Convert.FromBase64String(rawValue);
        }
        /// <summary>
        /// Gets the untyped value of the node
        /// </summary>
        /// <returns>The untyped value of the node.</returns>
        private UntypedNode? GetUntypedValue()
        {
            UntypedNode? untypedNode = null;
            switch(_jsonNode.ValueKind)
            {
                case JsonValueKind.Number:
                    var rawValue = _jsonNode.GetRawText();
                    untypedNode = new UntypedNumber(rawValue);
                    break;
                case JsonValueKind.String:
                    var stringValue = _jsonNode.GetString();
                    untypedNode = new UntypedString(stringValue);
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    var boolValue = _jsonNode.GetBoolean();
                    untypedNode = new UntypedBoolean(boolValue);
                    break;
                case JsonValueKind.Array:
                    var arrayValue = GetCollectionOfUntypedValues();
                    untypedNode = new UntypedArray(arrayValue);
                    break;
                case JsonValueKind.Object:
                    var objectValue = GetPropertiesOfUntypedObject();
                    untypedNode = new UntypedObject(objectValue);
                    break;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    untypedNode = new UntypedNull();
                    break;
            }
            
            return untypedNode;
        }
        private static readonly Type booleanType = typeof(bool?);
        private static readonly Type byteType = typeof(byte?);
        private static readonly Type sbyteType = typeof(sbyte?);
        private static readonly Type stringType = typeof(string);
        private static readonly Type intType = typeof(int?);
        private static readonly Type floatType = typeof(float?);
        private static readonly Type longType = typeof(long?);
        private static readonly Type doubleType = typeof(double?);
        private static readonly Type guidType = typeof(Guid?);
        private static readonly Type dateTimeOffsetType = typeof(DateTimeOffset?);
        private static readonly Type timeSpanType = typeof(TimeSpan?);
        private static readonly Type dateType = typeof(Date?);
        private static readonly Type timeType = typeof(Time?);

        /// <summary>
        /// Get the collection of primitives of type <typeparam name="T"/>from the json node
        /// </summary>
        /// <returns>A collection of objects</returns>
        public IEnumerable<T> GetCollectionOfPrimitiveValues<T>()
        {
            if (_jsonNode.ValueKind == JsonValueKind.Array) {
                var genericType = typeof(T);
                foreach(var collectionValue in _jsonNode.EnumerateArray())
                {
                    var currentParseNode = new JsonParseNode(collectionValue)
                    {
                        OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                        OnAfterAssignFieldValues = OnAfterAssignFieldValues
                    };
                    if(genericType == booleanType)
                        yield return (T)(object)currentParseNode.GetBoolValue()!;
                    else if(genericType == byteType)
                        yield return (T)(object)currentParseNode.GetByteValue()!;
                    else if(genericType == sbyteType)
                        yield return (T)(object)currentParseNode.GetSbyteValue()!;
                    else if(genericType == stringType)
                        yield return (T)(object)currentParseNode.GetStringValue()!;
                    else if(genericType == intType)
                        yield return (T)(object)currentParseNode.GetIntValue()!;
                    else if(genericType == floatType)
                        yield return (T)(object)currentParseNode.GetFloatValue()!;
                    else if(genericType == longType)
                        yield return (T)(object)currentParseNode.GetLongValue()!;
                    else if(genericType == doubleType)
                        yield return (T)(object)currentParseNode.GetDoubleValue()!;
                    else if(genericType == guidType)
                        yield return (T)(object)currentParseNode.GetGuidValue()!;
                    else if(genericType == dateTimeOffsetType)
                        yield return (T)(object)currentParseNode.GetDateTimeOffsetValue()!;
                    else if(genericType == timeSpanType)
                        yield return (T)(object)currentParseNode.GetTimeSpanValue()!;
                    else if(genericType == dateType)
                        yield return (T)(object)currentParseNode.GetDateValue()!;
                    else if(genericType == timeType)
                        yield return (T)(object)currentParseNode.GetTimeValue()!;
                    else
                        throw new InvalidOperationException($"unknown type for deserialization {genericType.FullName}");
                }
            }
        }

        /// <summary>
        /// Gets the collection of untyped values of the node.
        /// </summary>
        /// <returns>The collection of untyped values.</returns>
        private IEnumerable<UntypedNode> GetCollectionOfUntypedValues()
        {
            if (_jsonNode.ValueKind == JsonValueKind.Array)
            {
                foreach(var collectionValue in _jsonNode.EnumerateArray())
                {
                    var currentParseNode = new JsonParseNode(collectionValue)
                    {
                        OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                        OnAfterAssignFieldValues = OnAfterAssignFieldValues
                    };
                    yield return currentParseNode.GetUntypedValue()!;
                }
            }
        }

        /// <summary>
        /// Gets the collection of properties in the untyped object.
        /// </summary>
        /// <returns>The collection of properties in the untyped object.</returns>
        private IDictionary<string, UntypedNode> GetPropertiesOfUntypedObject()
        {
            var properties = new Dictionary<string, UntypedNode>();
            if(_jsonNode.ValueKind == JsonValueKind.Object)
            {
                foreach(var objectValue in _jsonNode.EnumerateObject())
                {
                    UntypedNode? untypedNode;
                    switch(objectValue.Value.ValueKind)
                    {
                        case JsonValueKind.Number:
                            var stringRawValue = objectValue.Value.GetRawText();
                            untypedNode = new UntypedNumber(stringRawValue);
                            break;
                        case JsonValueKind.String:
                            var stringValue = objectValue.Value.GetString();
                            untypedNode = new UntypedString(stringValue);
                            break;
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            var boolValue = objectValue.Value.GetBoolean();
                            untypedNode = new UntypedBoolean(boolValue);
                            break;
                        case JsonValueKind.Array:
                            var arrayValue = GetCollectionOfUntypedValues();
                            untypedNode = new UntypedArray(arrayValue);
                            break;
                        case JsonValueKind.Object:
                            var childNode = new JsonParseNode(objectValue.Value)
                            {
                                OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                                OnAfterAssignFieldValues = OnAfterAssignFieldValues
                            };
                            var objectVal = childNode.GetPropertiesOfUntypedObject();
                            untypedNode = new UntypedObject(objectVal);
                            break;
                        case JsonValueKind.Null:
                        case JsonValueKind.Undefined:
                            untypedNode = new UntypedNull();
                            break;
                        default:
                            untypedNode = new UntypedNull();
                            break;
                    }
                    properties[objectValue.Name] = untypedNode;
                }
            }
            return properties;
        }

        /// <summary>
        /// The action to perform before assigning field values.
        /// </summary>
        public Action<IParsable>? OnBeforeAssignFieldValues { get; set; }

        /// <summary>
        /// The action to perform after assigning field values.
        /// </summary>
        public Action<IParsable>? OnAfterAssignFieldValues { get; set; }

        /// <summary>
        /// Get the object of type <typeparam name="T"/>from the json node
        /// </summary>
        /// <param name="factory">The factory to use to create the model object.</param>
        /// <returns>A object of the specified type</returns>
        public T GetObjectValue<T>(ParsableFactory<T> factory) where T : IParsable
        {
            // until interface exposes GetUntypedValue()
            var genericType = typeof(T);
            if(genericType == typeof(UntypedNode))
            {
                return (T)(object)GetUntypedValue()!;
            }
            var item = factory(this);
            OnBeforeAssignFieldValues?.Invoke(item);
            AssignFieldValues(item);
            OnAfterAssignFieldValues?.Invoke(item);
            return item;
        }
        private void AssignFieldValues<T>(T item) where T : IParsable
        {
            if(_jsonNode.ValueKind != JsonValueKind.Object) return;
            IDictionary<string, object>? itemAdditionalData = null;
            if(item is IAdditionalDataHolder holder)
            {
                holder.AdditionalData ??= new Dictionary<string, object>();
                itemAdditionalData = holder.AdditionalData;
            }
            var fieldDeserializers = item.GetFieldDeserializers();  

            foreach(var fieldValue in _jsonNode.EnumerateObject())
            {
                if(fieldDeserializers.ContainsKey(fieldValue.Name))
                {
                    if(fieldValue.Value.ValueKind == JsonValueKind.Null)
                        continue;// If the property is already null just continue. As calling functions like GetDouble,GetBoolValue do not process JsonValueKind.Null.

                    var fieldDeserializer = fieldDeserializers[fieldValue.Name];
                    Debug.WriteLine($"found property {fieldValue.Name} to deserialize");
                    fieldDeserializer.Invoke(new JsonParseNode(fieldValue.Value)
                    {
                        OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                        OnAfterAssignFieldValues = OnAfterAssignFieldValues
                    });
                }
                else if (itemAdditionalData != null)
                {
                    Debug.WriteLine($"found additional property {fieldValue.Name} to deserialize");
                    IDictionaryExtensions.TryAdd(itemAdditionalData, fieldValue.Name, TryGetAnything(fieldValue.Value)!);
                }
                else
                {
                    Debug.WriteLine($"found additional property {fieldValue.Name} to deserialize but the model doesn't support additional data");
                }
            }
        }
        private static object? TryGetAnything(JsonElement element)
        {
            switch(element.ValueKind)
            {
                case JsonValueKind.Number:
                    if(element.TryGetDecimal(out var dec)) return dec;
                    else if(element.TryGetDouble(out var db)) return db;
                    else if(element.TryGetInt16(out var s)) return s;
                    else if(element.TryGetInt32(out var i)) return i;
                    else if(element.TryGetInt64(out var l)) return l;
                    else if(element.TryGetSingle(out var f)) return f;
                    else if(element.TryGetUInt16(out var us)) return us;
                    else if(element.TryGetUInt32(out var ui)) return ui;
                    else if(element.TryGetUInt64(out var ul)) return ul;
                    else throw new InvalidOperationException("unexpected additional value type during number deserialization");
                case JsonValueKind.String:
                    if(element.TryGetDateTime(out var dt)) return dt;
                    else if(element.TryGetDateTimeOffset(out var dto)) return dto;
                    else if(element.TryGetGuid(out var g)) return g;
                    else return element.GetString();
                case JsonValueKind.Array:
                case JsonValueKind.Object:
                    return element;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    throw new InvalidOperationException($"unexpected additional value type during deserialization json kind : {element.ValueKind}");
            }
        }

        /// <summary>
        /// Get the child node of the specified identifier
        /// </summary>
        /// <param name="identifier">The identifier of the child node</param>
        /// <returns>An instance of <see cref="IParseNode"/></returns>
        public IParseNode? GetChildNode(string identifier)
        {
            if(_jsonNode.ValueKind == JsonValueKind.Object && _jsonNode.TryGetProperty(identifier ?? throw new ArgumentNullException(nameof(identifier)), out var jsonElement)) 
            {
                return new JsonParseNode(jsonElement)
                {
                    OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                    OnAfterAssignFieldValues = OnAfterAssignFieldValues
                };
            }

            return default;
        }
        
#if NET5_0_OR_GREATER
        private static string ToEnumRawName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string value) where T : struct, Enum
#else
        private static string ToEnumRawName<T>(string value) where T : struct, Enum
#endif
        {
            if (typeof(T).GetMembers().FirstOrDefault(member =>
                   member.GetCustomAttribute<EnumMemberAttribute>() is { } attr &&
                   value.Equals(attr.Value, StringComparison.Ordinal))?.Name is { } strValue)
                return strValue;

            return value;
        }
    }
}
