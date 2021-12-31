using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace ConfigurationSubstitution.Tests
{
    public class ConfigurationTests
    {
        public static TheoryData<Func<IConfigurationBuilder>> ConfigurationBuilderTheoryData
            = new()
            {
                () => new ConfigurationBuilder(),
                () => new ConfigurationManager()
            };

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_substitution_is_in_middle(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_substitution_is_first(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_substitution_is_last(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_multiple_substitutions(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_nested(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_nested_with_fallback_default_values(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "{Bar1:default_value_1}" },
                    { "Bar1", "{Bar2:default_value_2}" },
                    { "Bar2", "-Jean-" }
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("{", "}", ":");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("-Jean-");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_recursive(Func<IConfigurationBuilder> builderGenerator)
        {
            Action act1 = () =>
            {
                var configurationBuilder = builderGenerator()
                    .AddInMemoryCollection(new Dictionary<string, string>()
                    {
                        { "Foo", "{Bar1}" },
                        { "Bar1", "{Foo}" },
                    })
                    .EnableSubstitutions();

                var configuration = configurationBuilder.Build();

                var substituted = configuration["Foo"];
            };

            // Act & assert
            act1.Should().Throw<EndlessRecursionVariableException>();

            Action act2 = () =>
            {
                var configurationBuilder = builderGenerator()
                    .AddInMemoryCollection(new Dictionary<string, string>()
                    {
                        { "Foo", "{Bar1:default_value}" },
                        { "Bar1", "{Foo}" },
                    })
                    .EnableSubstitutionsWithDelimitedFallbackDefaults("{", "}", ":");

                var configuration = configurationBuilder.Build();

                var substituted = configuration["Foo"];
            };

            // Act & assert
            act2.Should().Throw<EndlessRecursionVariableException>();

        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_different_start_end(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_non_substituted_value_as_is(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_for_non_resolved_variable(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_for_non_resolved_variable_and_mismatch_fallback_default_value_delimiter(
            Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "TestKey", "Test value {Foobar}" }
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("{", "}", ":");

            var configuration = configurationBuilder.Build();

            // Act
            Action act = () => _ = configuration["TestKey"];

            act.Should().Throw<UndefinedConfigVariableException>().WithMessage("*variable*{Foobar}*");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_ignore_non_resolved_variable(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "TestKey", "Test value {Foobar}" }
                })
                .EnableSubstitutions(exceptionOnMissingVariables: false);

            var configuration = configurationBuilder.Build();

            var value = configuration["TestKey"];
            value.Should().Be("Test value ");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_ignore_non_resolved_variable_and_mismatch_fallback_default_value_delimiter(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "TestKey", "Test value {Foobar}" }
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("{", "}", ":", false);

            var configuration = configurationBuilder.Build();

            var value = configuration["TestKey"];
            value.Should().Be("Test value ");
        }


        [Theory] // covers https://github.com/molinch/ConfigurationSubstitutor/issues/4
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_multiple_matches_present(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_using_different_substituable_pattern(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_not_get_substituted_value_when_no_match(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_not_get_substituted_value_when_not_maching_start_tag(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_not_get_substituted_value_when_no_end_tag(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_substitute_variable_when_substituted_value_is_empty(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_substituted_value_is_null(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_substituted_value_is_null_and_mismatch_fallback_default_value_delimiter(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    { "Foo", "$(Var1)" },
                    { "Var1", null }
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", ":");

            var configuration = configurationBuilder.Build();

            Func<string> func = () => configuration["Foo"];

            // Act & assert
            func.Should().Throw<UndefinedConfigVariableException>();
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_substituted_value_when_using_long_substituable_pattern(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
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

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_substitute_when_delimited_fallback_default_value_provided(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "Hello %(env:Testo)%" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("%(", ")%", ":");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Hello Testo");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_substitute_variable_when_provided_and_not_fallback_default_value(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "Hello $(Var1:Testo)" },
                    { "Var1", "world" }
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", ":");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Hello world");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_substitutableStartsWith_is_null(Func<IConfigurationBuilder> builderGenerator)
        {
            Action act1 = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutions(null, ")");

            // Act & assert
            act1.Should().Throw<ArgumentException>().WithParameterName("substitutableStartsWith");

            Action act2 = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults(null, ")", ":");

            // Act & assert
            act2.Should().Throw<ArgumentException>().WithParameterName("substitutableStartsWith");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_substitutableStartsWith_is_empty(Func<IConfigurationBuilder> builderGenerator)
        {
            Action act1 = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutions("", ")");

            // Act & assert
            act1.Should().Throw<ArgumentException>().WithParameterName("substitutableStartsWith");

            Action act2 = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("", ")", ":");

            // Act & assert
            act2.Should().Throw<ArgumentException>().WithParameterName("substitutableStartsWith");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_substitutableEndsWith_is_null(Func<IConfigurationBuilder> builderGenerator)
        {
            Action act1 = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutions("$(", null);

            // Act & assert
            act1.Should().Throw<ArgumentException>().WithParameterName("substitutableEndsWith");

            Action act2 = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", null, ":");

            // Act & assert
            act2.Should().Throw<ArgumentException>().WithParameterName("substitutableEndsWith");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_substitutableEndsWith_is_empty(Func<IConfigurationBuilder> builderGenerator)
        {
            Action act1 = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutions("$(", "");

            // Act & assert
            act1.Should().Throw<ArgumentException>().WithParameterName("substitutableEndsWith");

            Action act2 = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", "", ":");

            // Act & assert
            act2.Should().Throw<ArgumentException>().WithParameterName("substitutableEndsWith");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_substitute_when_fallback_default_value_is_empty(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", ":");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be(string.Empty);

        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_fallback_default_value_delimiter_is_null(Func<IConfigurationBuilder> builderGenerator)
        {
            Action act = () => builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", null);

            // Act & assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("fallbackDefaultValueDelimiter");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_substitute_when_fallback_default_value_delimiter_is_empty_with_substitutable_variable(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                    { "Var1:Testo", "Bar"}
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", String.Empty);

            var configuration = configurationBuilder.Build();

            var substituted = configuration["Foo"];

            substituted.Should().Be("Bar");
        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_throw_exception_when_fallback_default_value_delimiter_is_empty_without_substitutable_variable(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:Testo)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", String.Empty);

            var configuration = configurationBuilder.Build();

            Func<string> func = () => configuration["Foo"];

            // Act & assert
            // Note: This assertion is NOT for ArgumentNullException nor ArgumentNullException, because it's valid to
            //        pass an empty string delimiter constructor-wise. However, in this particular example variable
            //        'Var1:Testo' doesn't exist, thus the UndefinedConfigVariableException exception is thrown.
            func.Should().Throw<UndefinedConfigVariableException>();

        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_substitute_when_fallback_default_value_delimiter_is_a_space(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1 Testo)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Testo");

        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_substitute_first_occurance_of_fallback_default_value_after_delimiter(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "$(Var1:http://example.com)" },
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", ":");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("http://example.com");

        }

        [Theory]
        [MemberData(nameof(ConfigurationBuilderTheoryData))]
        public void Should_get_fallback_default_values_substituted_when_multiple_matches_present(Func<IConfigurationBuilder> builderGenerator)
        {
            var configurationBuilder = builderGenerator()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Foo", "Works with $(Var1), $(Var2:fbDefaulVal2) and $(Var3:fbDefaulVal3)" },
                    { "Var1", "one" },
                    { "Var3", "three" }
                })
                .EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", ":");

            var configuration = configurationBuilder.Build();

            // Act
            var substituted = configuration["Foo"];

            substituted.Should().Be("Works with one, fbDefaulVal2 and three");

        }

    }
}
