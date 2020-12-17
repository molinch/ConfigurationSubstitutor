using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace ConfigurationSubstitution.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void Should_get_substituted_value_when_substitution_is_in_middle()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "ConnectionString", "blablabla&password={DatabasePassword}&server=localhost" },
                    { "DatabasePassword", "ComplicatedPassword" }
                })
                .EnableSubstitutions();

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["ConnectionString"];

            substituted.Should().Be("blablabla&password=ComplicatedPassword&server=localhost");
        }

        [Fact]
        public void Should_get_substituted_value_when_substitution_is_first()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "ConnectionString", "{DatabasePassword}&server=localhost" },
                    { "DatabasePassword", "ComplicatedPassword" }
                })
                .EnableSubstitutions();

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["ConnectionString"];

            substituted.Should().Be("ComplicatedPassword&server=localhost");
        }

        [Fact]
        public void Should_get_substituted_value_when_substitution_is_last()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "ConnectionString", "blablabla&password={DatabasePassword}" },
                    { "DatabasePassword", "ComplicatedPassword" }
                })
                .EnableSubstitutions();

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["ConnectionString"];

            substituted.Should().Be("blablabla&password=ComplicatedPassword");
        }

        [Fact]
        public void Should_get_substituted_value_when_multiple_substitutions()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "{Bar1}{Bar2}{Bar1}" },
                    { "Bar1", "Barista" },
                    { "Bar2", "-Jean-" }
                })
                .EnableSubstitutions();

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Barista-Jean-Barista");
        }

        [Fact]
        public void Should_get_substituted_value_when_different_start_end()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "yolo $(Bar) what's up?" },
                    { "Bar", "boy" }
                })
                .EnableSubstitutions("$(", ")");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("yolo boy what's up?");
        }

        [Fact]
        public void Should_get_non_substituted_value_as_is()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Bar", "Boyz n the hood" }
                })
                .EnableSubstitutions();

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Bar"];

            substituted.Should().Be("Boyz n the hood");
        }

        [Fact]
        public void Should_throw_for_non_resolved_variable()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "TestKey", "Test value {Foobar}" }
                })
                .EnableSubstitutions(exceptionOnMissingVariables: true);

            var configuration = configurationBuilder.Build();

            // Act
            Action act = () => _ = configuration["TestKey"];

            act.Should().Throw<UndefinedConfigVariableException>().WithMessage("*variable*{Foobar}*");
        }

        [Fact]
        public void Should_ignore_non_resolved_variable()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "TestKey", "Test value {Foobar}" }
                })
                .EnableSubstitutions();

            var configuration = configurationBuilder.Build();

            var value = configuration["TestKey"];
            value.Should().Be("Test value ");
        }
    }
}
