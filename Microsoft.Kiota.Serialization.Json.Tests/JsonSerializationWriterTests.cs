using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Json.Tests.Converters;
using Microsoft.Kiota.Serialization.Json.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Json.Tests
{
    public class JsonSerializationWriterTests
    {
        [Fact]
        public void WritesSampleObjectValue()
        {
            // Arrange
            var testEntity = new TestEntity()
            {
                Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
                WorkDuration = TimeSpan.FromHours(1),
                StartWorkTime = new Time(8, 0, 0),
                BirthDay = new Date(2017, 9, 4),
                HeightInMetres = 1.80m,
                AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone",null}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"weightInKgs", 51.80m}, // write weigth
                    {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                    {"endDateTime", new DateTime(2023,03,14,0,0,0,DateTimeKind.Utc) }, // ensure the DateTime doesn't crash
                    {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
                }
            };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty,testEntity);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();
            
            // Assert
            var expectedString = "{" +
                                 "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                 "\"workDuration\":\"PT1H\","+    // Serializes timespans
                                 "\"birthDay\":\"2017-09-04\"," + // Serializes dates
                                 "\"heightInMetres\":1.80,"+
                                 "\"startWorkTime\":\"08:00:00\"," + //Serializes times
                                 "\"mobilePhone\":null," +
                                 "\"accountEnabled\":false," +
                                 "\"jobTitle\":\"Author\"," +
                                 "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                 "\"weightInKgs\":51.80,"+
                                 "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                 "\"endDateTime\":\"2023-03-14T00:00:00+00:00\"," +
                                 "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}" +
                                 "}";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WritesSampleObjectValueWithJsonElementAdditionalData()
        {
            var nullJsonElement = JsonDocument.Parse("null").RootElement;
            var arrayJsonElement = JsonDocument.Parse("[\"+1 412 555 0109\"]").RootElement;
            var objectJsonElement = JsonDocument.Parse("{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}").RootElement;

            // Arrange
            var testEntity = new TestEntity()
            {
                Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
                WorkDuration = TimeSpan.FromHours(1),
                StartWorkTime = new Time(8, 0, 0),
                BirthDay = new Date(2017, 9, 4),
                AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone", nullJsonElement}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"businessPhones", arrayJsonElement }, // write collection of primitives value
                    {"manager", objectJsonElement }, // write nested object value
                }
            };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "{" +
                                 "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                 "\"workDuration\":\"PT1H\"," +    // Serializes timespans
                                 "\"birthDay\":\"2017-09-04\"," + // Serializes dates
                                 "\"startWorkTime\":\"08:00:00\"," + //Serializes times
                                 "\"mobilePhone\":null," +
                                 "\"accountEnabled\":false," +
                                 "\"jobTitle\":\"Author\"," +
                                 "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                 "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                 "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}" +
                                 "}";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WritesSampleCollectionOfObjectValues()
        {
            // Arrange
            var testEntity = new TestEntity()
            {
                Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
                Numbers = TestEnum.One | TestEnum.Two,
                TestNamingEnum = TestNamingEnum.Item2SubItem1,
                AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone",null}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                    {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
                }
            };
            var entityList = new List<TestEntity>() { testEntity};
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "[{" +
                                 "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                 "\"numbers\":\"one,two\"," +
                                 "\"testNamingEnum\":\"Item2:SubItem1\"," +
                                 "\"mobilePhone\":null," +
                                 "\"accountEnabled\":false," +
                                 "\"jobTitle\":\"Author\"," +
                                 "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                 "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                 "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}" +
                                 "}]";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WritesEnumValuesAsCamelCasedIfNotEscaped()
        {
            // Arrange
            var testEntity = new TestEntity()
            {
                TestNamingEnum = TestNamingEnum.Item1,
            };
            var entityList = new List<TestEntity>() { testEntity };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "[{" +
                                 "\"testNamingEnum\":\"item1\"" + // Camel Cased
                                 "}]";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WritesEnumValuesAsDescribedIfEscaped()
        {
            // Arrange
            var testEntity = new TestEntity()
            {
                TestNamingEnum = TestNamingEnum.Item2SubItem1,
            };
            var entityList = new List<TestEntity>() { testEntity };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "[{" +
                                 "\"testNamingEnum\":\"Item2:SubItem1\"" + // Appears same as attribute
                                 "}]";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WriteGuidUsingConverter()
        {
            // Arrange
            var id = Guid.NewGuid();
            var testEntity = new ConverterTestEntity { Id = id };
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new JsonGuidConverter() }
            };
            var serializationContext = new KiotaJsonSerializationContext(serializerOptions);
            using var jsonSerializerWriter = new JsonSerializationWriter(serializationContext);

            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();
            
            // Assert
            var expectedString = $"{{\"id\":\"{id:N}\"}}";
            Assert.Equal(expectedString, serializedJsonString);
        }
        
        [Fact]
        public void WriteGuidUsingNoConverter()
        {
            // Arrange
            var id = Guid.NewGuid();
            var testEntity = new ConverterTestEntity { Id = id };
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);
            var serializationContext = new KiotaJsonSerializationContext(serializerOptions);
            using var jsonSerializerWriter = new JsonSerializationWriter(serializationContext);

            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();
            
            // Assert
            var expectedString = $"{{\"id\":\"{id:D}\"}}";
            Assert.Equal(expectedString, serializedJsonString);
        }
    }
}
