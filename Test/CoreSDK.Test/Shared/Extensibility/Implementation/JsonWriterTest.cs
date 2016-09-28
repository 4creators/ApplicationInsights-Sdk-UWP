﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Microsoft.ApplicationInsights.DataContracts;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
	using Assert = Xunit.Assert;
    using AssertEx = Xunit.AssertEx;

    [TestClass]
    public class JsonWriterTest
    {
        [TestMethod]
        public void ClassIsInternalAndNotMeantToBeAccessedByCustomers()
        {
            Assert.False(typeof(JsonWriter).GetTypeInfo().IsPublic);
        }

        [TestMethod]
        public void WriteStartArrayWritesOpeningSquareBracket()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteStartArray();
                Assert.Equal("[", stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WriteStartObjectWritesOpeningCurlyBrace()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteStartObject();
                Assert.Equal("{", stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WriteEndArrayWritesClosingSquareBracket()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteEndArray();
                Assert.Equal("]", stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WriteEndObjectWritesClosingCurlyBrace()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteEndObject();
                Assert.Equal("}", stringWriter.ToString());
            }        
        }

        [TestMethod]
        public void WriteRawValueWritesValueWithoutEscapingValue()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteRawValue(@"Test\Name");
                Assert.Equal(@"Test\Name", stringWriter.ToString());
            }
        }

        #region WriteProperty(string, int?)

        [TestMethod]
        public void WritePropertyIntWritesIntValueWithoutQuotationMark()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                const string Name = "name";
                const int Value = 42;
                new JsonWriter(stringWriter).WriteProperty(Name, Value);
                Assert.Equal("\"" + Name + "\":" + Value, stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyIntDoesNothingIfValueIsNullBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", (int?)null);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        #endregion

        #region WriteProperty(string, double?)

        [TestMethod]
        public void WritePropertyDoubleWritesDoubleValueWithoutQuotationMark()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                const string Name = "name";
                const double Value = 42.3;
                new JsonWriter(stringWriter).WriteProperty(Name, Value);
                Assert.Equal("\"" + Name + "\":" + Value.ToString(CultureInfo.InvariantCulture), stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyDoubleDoesNothingIfValueIsNullBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", (double?)null);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        #endregion

        #region WriteProperty(string, TimeSpan?)

        [TestMethod]
        public void WritePropertyTimeSpanWritesTimeSpanValueWithQuotationMark()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                const string Name = "name";
                TimeSpan value = TimeSpan.FromSeconds(123);
                new JsonWriter(stringWriter).WriteProperty(Name, value);
                Assert.Equal("\"" + Name + "\":\"" + value + "\"", stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyTimeSpanDoesNothingIfValueIsNullBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", (TimeSpan?)null);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        #endregion

        #region WriteProperty(string, string)

        [TestMethod]
        public void WritePropertyStringWritesValueInDoubleQuotes()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                const string Name = "name";
                const string Value = "value";
                new JsonWriter(stringWriter).WriteProperty(Name, Value);
                Assert.Equal("\"" + Name + "\":\"" + Value + "\"", stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyStringThrowsArgumentNullExceptionForNameInputAsNull()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var writer = new JsonWriter(stringWriter);
                Assert.Throws<ArgumentNullException>(() => writer.WriteProperty(null, "value"));
            }
        }

        [TestMethod]
        public void WritePropertyStringDoesNothingIfValueIsNullBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", (string)null);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyStringDoesNothingIfValueIsEmptyBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", string.Empty);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        #endregion

        #region WriteProperty(string, bool?)

        [TestMethod]
        public void WritePropertyBooleanWritesValueWithoutQuotationMarks()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                const string Name = "name";
                const bool Value = true;
                new JsonWriter(stringWriter).WriteProperty(Name, Value);
                string expectedValue = Value.ToString().ToLowerInvariant();
                Assert.Equal("\"" + Name + "\":" + expectedValue, stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyBooleanDoesNothingIfValueIsNullBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", (bool?)null);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyBooleanWritesFalseBecauseItIsExplicitlySet()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                const string Name = "name";
                const bool Value = false;
                new JsonWriter(stringWriter).WriteProperty(Name, Value);
                string expectedValue = Value.ToString().ToLowerInvariant();
                Assert.Equal("\"" + Name + "\":" + expectedValue, stringWriter.ToString());
            }
        }

        #endregion

        #region WriteProperty(string, DateTimeOffset?)

        [TestMethod]
        public void WritePropertyDateTimeOffsetWritesValueInQuotationMarksAndRoundTripDateTimeFormat()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                const string Name = "name";
                DateTimeOffset value = DateTimeOffset.UtcNow;
                new JsonWriter(stringWriter).WriteProperty(Name, value);
                string expectedValue = value.ToString("o", CultureInfo.InvariantCulture);
                Assert.Equal("\"" + Name + "\":\"" + expectedValue + "\"", stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyDateTimeOffsetDoesNothingIfValueIsNullBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", (DateTimeOffset?)null);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        #endregion

        #region WriteProperty(string, IDictionary<string, double>)

        [TestMethod]
        public void WritePropertyIDictionaryDoubleWritesPropertyNameFollowedByValuesInCurlyBraces()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var writer = new JsonWriter(stringWriter);
                writer.WriteProperty("name", new Dictionary<string, double> { { "key1", 1 } });
                AssertEx.StartsWith("\"name\":{", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
                AssertEx.EndsWith("}", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WritePropertyIDictionaryDoubleWritesValuesWithoutDoubleQuotes()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var writer = new JsonWriter(stringWriter);
                writer.WriteProperty("name", new Dictionary<string, double> { { "key1", 1 } });
                Assert.Contains("\"key1\":1", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WritePropertyIDictionaryDoubleDoesNothingWhenDictionaryIsNullBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", (IDictionary<string, double>)null);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyIDictionaryDoubleDoesNothingWhenDictionaryIsEmptyBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", new Dictionary<string, double>());
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        #endregion

        #region WriteProperty(string, IDictionary<string, object>)

        [TestMethod]
        public void WritePropertyIDictionaryStringStringWritesPropertyNameFollowedByValuesInCurlyBraces()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var writer = new JsonWriter(stringWriter);
                writer.WriteProperty("name", new Dictionary<string, string> { { "key1", "1" } });
                AssertEx.StartsWith("\"name\":{", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
                AssertEx.EndsWith("}", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WritePropertyIDictionaryStringStringWritesValuesWithoutDoubleQuotes()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var writer = new JsonWriter(stringWriter);
                writer.WriteProperty("name", new Dictionary<string, string> { { "key1", "1" } });
                Assert.Contains("\"key1\":\"1\"", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WritePropertyIDictionaryStringStringDoesNothingWhenDictionaryIsNullBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", (IDictionary<string, string>)null);
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyIDictionaryStringStringDoesNothingWhenDictionaryIsEmptyBecauseItAssumesPropertyIsOptional()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new JsonWriter(stringWriter).WriteProperty("name", new Dictionary<string, string>());
                Assert.Equal(string.Empty, stringWriter.ToString());
            }
        }

        #endregion

        #region WritePropertyName

        [TestMethod]
        public void WritePropertyNameWritesPropertyNameEnclosedInDoubleQuotationMarksFollowedByColon()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                new TestableJsonWriter(stringWriter).WritePropertyName("TestProperty");
                Assert.Equal("\"TestProperty\":", stringWriter.ToString());
            }
        }

        [TestMethod]
        public void WritePropertyNamePrependsPropertyNameWithComaWhenCurrentObjectAlreadyHasProperties()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WritePropertyName("Property1");
                jsonWriter.WritePropertyName("Property2");
                Assert.Contains(",\"Property2\"", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WritePropertyNameDoesNotPrependPropertyNameWithComaWhenNewObjectWasStarted()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WritePropertyName("Property1");
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("Property2");
                Assert.Contains("{\"Property2\"", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WritePropertyNameThrowsArgumentExceptionWhenPropertyNameIsEmptyToPreventOurOwnErrors()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new JsonWriter(stringWriter);
                Assert.Throws<ArgumentException>(() => jsonWriter.WritePropertyName(string.Empty));
            }
        }

        #endregion

        #region WriteString

        [TestMethod]
        public void WriteStringEscapesQuotationMark()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WriteString("Test\"Value");
                Assert.Contains("Test\\\"Value", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WriteStringEscapesBackslash()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WriteString("Test\\Value");
                Assert.Contains("Test\\\\Value", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WriteStringEscapesBackspace()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WriteString("Test\bValue");
                Assert.Contains("Test\\bValue", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }            
        }

        [TestMethod]
        public void WriteStringEscapesFormFeed()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WriteString("Test\fValue");
                Assert.Contains("Test\\fValue", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }            
        }

        [TestMethod]
        public void WriteStringEscapesNewline()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WriteProperty("name", "Test\nValue");
                Assert.Contains("Test\\nValue", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WriteStringEscapesCarriageReturn()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WriteProperty("name", "Test\rValue");
                Assert.Contains("Test\\rValue", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        [TestMethod]
        public void WriteStringEscapesHorizontalTab()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new TestableJsonWriter(stringWriter);
                jsonWriter.WriteProperty("name", "Test\tValue");
                Assert.Contains("Test\\tValue", stringWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        #endregion

        private class TestableJsonWriter : JsonWriter
        {
            public TestableJsonWriter(TextWriter textWriter)
                : base(textWriter)
            {
            }

            public new void WriteString(string value)
            {
                base.WriteString(value);
            }
        }
    }
}
