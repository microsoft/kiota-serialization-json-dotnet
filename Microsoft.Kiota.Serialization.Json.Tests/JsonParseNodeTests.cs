﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Json.Tests
{
    public class JsonParseNodeTests
    {
        private const string TestUserJson = "{\r\n" +
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

        private const string TestUntypedJson = "{\r\n" +
                                            "    \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#sites('contoso.sharepoint.com')/lists('fa631c4d-ac9f-4884-a7f5-13c659d177e3')/items('1')/fields/$entity\",\r\n" +
                                            "    \"id\": \"5\",\r\n" +
                                            "    \"title\": \"Project 101\",\r\n" +
                                            "    \"location\": {\r\n" +
                                            "        \"address\": {\r\n" +
                                            "            \"city\": \"Redmond\",\r\n" +
                                            "            \"postalCode\": \"98052\",\r\n" +
                                            "            \"state\": \"Washington\",\r\n" +
                                            "            \"street\": \"NE 36th St\"\r\n" +
                                            "        },\r\n" +
                                            "        \"coordinates\": {\r\n" +
                                            "            \"latitude\": 47.641942,\r\n" +
                                            "            \"longitude\": -122.127222\r\n" +
                                            "        },\r\n" +
                                            "        \"displayName\": \"Microsoft Building 92\",\r\n" +
                                            "        \"floorCount\": 50,\r\n" +
                                            "        \"hasReception\": true,\r\n" +
                                            "        \"contact\": null\r\n" +
                                            "    },\r\n" +
                                            "    \"keywords\": [\r\n" +
                                            "        {\r\n" +
                                            "            \"created\": \"2023-07-26T10:41:26Z\",\r\n" +
                                            "            \"label\": \"Keyword1\",\r\n" +
                                            "            \"termGuid\": \"10e9cc83-b5a4-4c8d-8dab-4ada1252dd70\",\r\n" +
                                            "            \"wssId\": 6442450942\r\n" +
                                            "        },\r\n" +
                                            "        {\r\n" +
                                            "            \"created\": \"2023-07-26T10:51:26Z\",\r\n" +
                                            "            \"label\": \"Keyword2\",\r\n" +
                                            "            \"termGuid\": \"2cae6c6a-9bb8-4a78-afff-81b88e735fef\",\r\n" +
                                            "            \"wssId\": 6442450943\r\n" +
                                            "        }\r\n" +
                                            "    ],\r\n" +
                                            "    \"detail\": null,\r\n" +
                                            "    \"extra\": {\r\n" +
                                            "        \"createdDateTime\":\"2024-01-15T00:00:00\\u002B00:00\"\r\n" +
                                            "    }\r\n" +
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
        public void GetEntityWithUntypedNodesFromJson()
        {
            // Arrange
            using var jsonDocument = JsonDocument.Parse(TestUntypedJson);
            var rootParseNode = new JsonParseNode(jsonDocument.RootElement);
            // Act
            var entity = rootParseNode.GetObjectValue(UntypedTestEntity.CreateFromDiscriminatorValue);
            // Assert
            Assert.NotNull(entity);
            Assert.Equal("5", entity.Id);
            Assert.Equal("Project 101", entity.Title);
            Assert.NotNull(entity.Location);
            Assert.IsType<UntypedObject>(entity.Location); // creates untyped object
            var location = (UntypedObject)entity.Location;
            var locationProperties = location.GetValue();
            Assert.IsType<UntypedObject>(locationProperties["address"]);
            Assert.IsType<UntypedString>(locationProperties["displayName"]); // creates untyped string
            Assert.IsType<UntypedInteger>(locationProperties["floorCount"]); // creates untyped number
            Assert.IsType<UntypedBoolean>(locationProperties["hasReception"]); // creates untyped boolean
            Assert.IsType<UntypedNull>(locationProperties["contact"]); // creates untyped null
            Assert.IsType<UntypedObject>(locationProperties["coordinates"]); // creates untyped null
            var coordinates = (UntypedObject)locationProperties["coordinates"];
            var coordinatesProperties = coordinates.GetValue();
            Assert.IsType<UntypedDecimal>(coordinatesProperties["latitude"]); // creates untyped decimal
            Assert.IsType<UntypedDecimal>(coordinatesProperties["longitude"]);
            Assert.Equal("Microsoft Building 92", ((UntypedString)locationProperties["displayName"]).GetValue());
            Assert.Equal(50, ((UntypedInteger)locationProperties["floorCount"]).GetValue());
            Assert.True(((UntypedBoolean)locationProperties["hasReception"]).GetValue());
            Assert.Null(((UntypedNull)locationProperties["contact"]).GetValue());
            Assert.NotNull(entity.Keywords);
            Assert.IsType<UntypedArray>(entity.Keywords); // creates untyped array
            Assert.Equal(2, ((UntypedArray)entity.Keywords).GetValue().Count());
            Assert.Null(entity.Detail);
            var extra = entity.AdditionalData["extra"];
            Assert.NotNull(extra);
        }
    }
}
