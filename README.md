# ConfigurationSubstitutor <img src="logo.png" width="5%" />
Allows to substitute variables from configuration, this way hostnames, or passwords can be separated and automatically substituted if another configuration entry references them.
Scenarios could be that you have the password from an Azure KeyVault and the connection string defined in appsettings. The connection string can reference the password.
Another scenario is that you have multiple configuration entries for the same domain, don't duplicate that information anymore, reference it.

# Nuget
The nuget package name is `ConfigurationSubstitutor`: https://www.nuget.org/packages/ConfigurationSubstitutor/

# Usage

## ASP.NET Core
To add it to ASP.NET Core configuration simply place `.EnableSubstitutions()` last.
Make sure `.EnableSubstitutions()` is always called after all other configurations are added, else it won't behave properly!

```c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureAppConfiguration((ctx, builder) =>
		{
			// if you have any additional configuration place it before
			builder.EnableSubstitutions();
		});
```

Another example where the configuration builder is created:
```c#
var configuration = new ConfigurationBuilder()
	.AddUserSecrets(typeof(GameRepositoryTests).Assembly)
	.AddJsonFile("appsettings.json")
	.EnableSubstitutions()
	.Build();
```

Remark: you can also specify the start/end strings that define a substitutable value.
Here we define that values are enclosed within $(), that's what is used by Azure DevOps for substitutions.
```c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureAppConfiguration((ctx, builder) =>
		{
			// if you have any additional configuration place it before
			builder.EnableSubstitutions("$(", ")");
		});
```


## Examples

### Connection string and password
Example where the entry ConnectionString references the DatabasePassword.
The configuration contains these two entries:
- ConnectionString = blablabla&password={DatabasePassword}&server=localhost
- DatabasePassword = ComplicatedPassword

```c#
var substituted = configuration["ConnectionString"];
```

Easy-peasy `substituted` contains `blablabla&password=ComplicatedPassword&server=localhost`

### More substitutions
It supports any number of substitutions, for example if the configuration contains these three entries:
- Foo = {Bar1}{Bar2}{Bar1}
- Bar1 = Barista
- Bar2 = -Jean-

```c#
var substituted = configuration["Foo"];
```

Now `substituted` contains `Barista-Jean-Barista`
