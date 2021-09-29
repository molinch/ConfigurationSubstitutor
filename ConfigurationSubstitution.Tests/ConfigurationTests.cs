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
        public void Should_get_substituted_value_when_nested()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "{Bar1}" },
                    { "Bar1", "{Bar2}" },
                    { "Bar2", "-Jean-" }
                })
                .EnableSubstitutions();

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("-Jean-");
        }

        [Fact]
        public void Should_throw_exception_when_recursive()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "{Bar1}" },
                    { "Bar1", "{Foo}" },
                })
                .EnableSubstitutions();

            var configuration = configurationBuilder.Build();

            Func<string> func = () => configuration["Foo"];

            // Act & assert
            func.Should().Throw<RecursiveConfigVariableException>();
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
                .EnableSubstitutions();

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
                .EnableSubstitutions(exceptionOnMissingVariables: false);

            var configuration = configurationBuilder.Build();

            var value = configuration["TestKey"];
            value.Should().Be("Test value ");
        }

        [Fact] // covers https://github.com/molinch/ConfigurationSubstitutor/issues/4
        public void Should_get_substituted_value_when_multiple_matches_present()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                { "Foo", "Works with $(Var1) and $(Var2)" },
                { "Var1", "one" },
                { "Var2", "two" }
                })
                .EnableSubstitutions("$(", ")");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Works with one and two");
        }

        [Fact]
        public void Should_get_substituted_value_when_using_different_substituable_pattern()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                { "Foo", "Hello <<Var>>" },
                { "Var", "world" }
                })
                .EnableSubstitutions("<<", ">>");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Hello world");
        }

        [Fact]
        public void Should_not_get_substituted_value_when_no_match()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                { "Foo", "Hello world, nothing to see here" }
                })
                .EnableSubstitutions("$(", ")");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Hello world, nothing to see here");
        }

        [Fact]
        public void Should_not_get_substituted_value_when_not_maching_start_tag()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                { "Foo", "Hello (world)" }
                })
                .EnableSubstitutions("$(", ")");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Hello (world)");
        }

        [Fact]
        public void Should_not_get_substituted_value_when_no_end_tag()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                { "Foo", "Hello $(Var what's up ?" }
                })
                .EnableSubstitutions("$(", ")");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Hello $(Var what's up ?");
        }

        [Fact]
        public void Should_substitute_variable_when_substituted_value_is_empty()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1)" },
                    { "Var1", string.Empty }
                })
                .EnableSubstitutions("$(", ")");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be(string.Empty);
        }

        [Fact]
        public void Should_throw_exception_when_substituted_value_is_null()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    { "Foo", "$(Var1)" },
                    { "Var1", null }
                })
                .EnableSubstitutions("$(", ")");

            var configuration = configurationBuilder.Build();

            Func<string> func = () => configuration["Foo"];

            // Act & assert
            func.Should().Throw<UndefinedConfigVariableException>();
        }

        [Fact]
        public void Should_get_substituted_value_when_using_long_substituable_pattern()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "Hello %(env,Testo)%" },
                    { "Testo", "world" }
                })
                .EnableSubstitutions("%(env,", ")%");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Hello world");
        }
    }
}
