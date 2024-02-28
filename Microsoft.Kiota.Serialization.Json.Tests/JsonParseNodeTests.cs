﻿using System;
using System.Linq;
using System.Text.Json;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Json.Tests.Converters;
using Microsoft.Kiota.Serialization.Json.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Kiota.Serialization.Json.Tests
{
    public class JsonParseNodeTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper output = output;

        internal const string TestUserJson = "{\r\n" +
                                            "    \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#users/$entity\",\r\n" +
                                            "    \"@odata.id\": \"https://graph.microsoft.com/v2/dcd219dd-bc68-4b9b-bf0b-4a33a796be35/directoryObjects/48d31887-5fad-4d73-a9f5-3c356e68a038/Microsoft.DirectoryServices.User\",\r\n" +
                                            "    \"businessPhones\": [\r\n" +
                                            "        \"+1 412 555 0109\"\r\n" +
                                            "    ],\r\n" +
                                            "    \"displayName\": \"Megan Bowen\",\r\n" +
                                            "    \"numbers\":\"one,two,thirtytwo\"," +
                                            "    \"testNamingEnum\":\"Item2:SubItem1\"," +
                                            "    \"givenName\": \"Megan\",\r\n" +
                                            "    \"accountEnabled\": true,\r\n" +
                                            "    \"createdDateTime\": \"2017 -07-29T03:07:25Z\",\r\n" +
                                            "    \"jobTitle\": \"Auditor\",\r\n" +
                                            "    \"mail\": \"MeganB@M365x214355.onmicrosoft.com\",\r\n" +
                                            "    \"mobilePhone\": null,\r\n" +
                                            "    \"officeLocation\": null,\r\n" +
                                            "    \"preferredLanguage\": \"en-US\",\r\n" +
                                            "    \"surname\": \"Bowen\",\r\n" +
                                            "    \"workDuration\": \"PT1H\",\r\n" +
                                            "    \"startWorkTime\": \"08:00:00.0000000\",\r\n" +
                                            "    \"endWorkTime\": \"17:00:00.0000000\",\r\n" +
                                            "    \"userPrincipalName\": \"MeganB@M365x214355.onmicrosoft.com\",\r\n" +
                                            "    \"birthDay\": \"2017-09-04\",\r\n" +
                                            "    \"id\": \"48d31887-5fad-4d73-a9f5-3c356e68a038\"\r\n" +
                                            "}";
        private const string TestStudentJson = "{\r\n" +
                                            "    \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#users/$entity\",\r\n" +
                                            "    \"@odata.type\": \"microsoft.graph.student\",\r\n" +
                                            "    \"@odata.id\": \"https://graph.microsoft.com/v2/dcd219dd-bc68-4b9b-bf0b-4a33a796be35/directoryObjects/48d31887-5fad-4d73-a9f5-3c356e68a038/Microsoft.DirectoryServices.User\",\r\n" +
                                            "    \"businessPhones\": [\r\n" +
                                            "        \"+1 412 555 0109\"\r\n" +
                                            "    ],\r\n" +
                                            "    \"displayName\": \"Megan Bowen\",\r\n" +
                                            "    \"numbers\":\"one,two,thirtytwo\"," +
                                            "    \"testNamingEnum\":\"Item2:SubItem1\"," +
                                            "    \"givenName\": \"Megan\",\r\n" +
                                            "    \"accountEnabled\": true,\r\n" +
                                            "    \"createdDateTime\": \"2017 -07-29T03:07:25Z\",\r\n" +
                                            "    \"jobTitle\": \"Auditor\",\r\n" +
                                            "    \"mail\": \"MeganB@M365x214355.onmicrosoft.com\",\r\n" +
                                            "    \"mobilePhone\": null,\r\n" +
                                            "    \"officeLocation\": null,\r\n" +
                                            "    \"preferredLanguage\": \"en-US\",\r\n" +
                                            "    \"surname\": \"Bowen\",\r\n" +
                                            "    \"workDuration\": \"PT1H\",\r\n" +
                                            "    \"startWorkTime\": \"08:00:00.0000000\",\r\n" +
                                            "    \"endWorkTime\": \"17:00:00.0000000\",\r\n" +
                                            "    \"userPrincipalName\": \"MeganB@M365x214355.onmicrosoft.com\",\r\n" +
                                            "    \"birthDay\": \"2017-09-04\",\r\n" +
                                            "    \"enrolmentDate\": \"2017-09-04\",\r\n" +
                                            "    \"id\": \"48d31887-5fad-4d73-a9f5-3c356e68a038\"\r\n" +
                                            "}";

        private static readonly string TestUserCollectionString = $"[{TestUserJson}]";

        [Fact]
        public void GetsEntityValueFromJson()
        {
            // Arrange
            using var jsonDocument = JsonDocument.Parse(TestUserJson);
            var jsonParseNode = new JsonParseNode(jsonDocument.RootElement);
            // Act
            var testEntity = jsonParseNode.GetObjectValue(TestEntity.CreateFromDiscriminator);
            // Assert
            Assert.NotNull(testEntity);
            Assert.Null(testEntity.OfficeLocation);
            Assert.NotEmpty(testEntity.AdditionalData);
            Assert.True(testEntity.AdditionalData.ContainsKey("jobTitle"));
            Assert.True(testEntity.AdditionalData.ContainsKey("mobilePhone"));
            Assert.Equal("Auditor", testEntity.AdditionalData["jobTitle"]);
            Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntity.Id);
            Assert.Equal(TestEnum.One | TestEnum.Two, testEntity.Numbers ); // Unknown enum value is not included
            Assert.Equal(TestNamingEnum.Item2SubItem1, testEntity.TestNamingEnum ); // correct value is chosen
            Assert.Equal(TimeSpan.FromHours(1), testEntity.WorkDuration); // Parses timespan values
            Assert.Equal(new Time(8,0,0).ToString(),testEntity.StartWorkTime.ToString());// Parses time values
            Assert.Equal(new Time(17, 0, 0).ToString(), testEntity.EndWorkTime.ToString());// Parses time values
            Assert.Equal(new Date(2017,9,4).ToString(), testEntity.BirthDay.ToString());// Parses date values
        }
        [Fact]
        public void GetsFieldFromDerivedType()
        {
            // Arrange
            using var jsonDocument = JsonDocument.Parse(TestStudentJson);
            var jsonParseNode = new JsonParseNode(jsonDocument.RootElement);
            // Act
            var testEntity = jsonParseNode.GetObjectValue(TestEntity.CreateFromDiscriminator) as DerivedTestEntity;
            // Assert
            Assert.NotNull(testEntity);
            Assert.NotNull(testEntity.EnrolmentDate);
        }

        [Fact]
        public void GetCollectionOfObjectValuesFromJson()
        {
            // Arrange
            using var jsonDocument = JsonDocument.Parse(TestUserCollectionString);
            var jsonParseNode = new JsonParseNode(jsonDocument.RootElement);
            // Act
            var testEntityCollection = jsonParseNode.GetCollectionOfObjectValues<TestEntity>(x => new TestEntity()).ToArray();
            // Assert
            Assert.NotEmpty(testEntityCollection);
            Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntityCollection[0].Id);
        }

        [Fact]
        public void GetsChildNodeAndGetCollectionOfPrimitiveValuesFromJsonParseNode()
        {
            // Arrange
            using var jsonDocument = JsonDocument.Parse(TestUserJson);
            var rootParseNode = new JsonParseNode(jsonDocument.RootElement);
            // Act to get business phones list
            var phonesListChildNode = rootParseNode.GetChildNode("businessPhones");
            var phonesList = phonesListChildNode.GetCollectionOfPrimitiveValues<string>().ToArray();
            // Assert
            Assert.NotEmpty(phonesList);
            Assert.Equal("+1 412 555 0109", phonesList[0]);
        }

        [Fact]
        public void ReturnsDefaultIfChildNodeDoesNotExist()
        {
            // Arrange
            using var jsonDocument = JsonDocument.Parse(TestUserJson);
            var rootParseNode = new JsonParseNode(jsonDocument.RootElement);
            // Try to get an imaginary node value
            var imaginaryNode = rootParseNode.GetChildNode("imaginaryNode");
            // Assert
            Assert.Null(imaginaryNode);
        }

        [Fact]
        public void ParseGuidWithConverter()
        {
            // Arrange
            var id = Guid.NewGuid();
            var json = $"{{\"id\": \"{id:N}\"}}";
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new JsonGuidConverter() }
            };
            var serializationContext = new KiotaJsonSerializationContext(serializerOptions);
            using var jsonDocument = JsonDocument.Parse(json);
            var rootParseNode = new JsonParseNode(jsonDocument.RootElement, serializationContext);
            
            // Act
            var entity = rootParseNode.GetObjectValue(_ => new ConverterTestEntity());
            
            // Assert
            Assert.Equal(id, entity.Id);
        }
        
        [Fact]
        public void ParseGuidWithoutConverter()
        {
            // Arrange
            var id = Guid.NewGuid();
            var json = $"{{\"id\": \"{id:D}\"}}";
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);
            var serializationContext = new KiotaJsonSerializationContext(serializerOptions);
            using var jsonDocument = JsonDocument.Parse(json);
            var rootParseNode = new JsonParseNode(jsonDocument.RootElement, serializationContext);
            
            // Act
            var entity = rootParseNode.GetObjectValue(_ => new ConverterTestEntity());
            
            // Assert
            Assert.Equal(id, entity.Id);
        }

        [Fact]
        public void BenchMarkParseNodes()
        {
            
            var logger = new AccumulationLogger();

            var config = ManualConfig.Create(DefaultConfig.Instance)
                .AddLogger(logger)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator);

            var summary = BenchmarkRunner.Run<ParseNodeBenchmark>(config);

            // write benchmark summary
            output.WriteLine(logger.GetLog());

            Assert.NotNull(summary);

        }

    }
}
