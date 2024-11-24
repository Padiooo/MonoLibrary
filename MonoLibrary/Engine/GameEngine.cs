using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using MonoLibrary.Dependency;
using MonoLibrary.Engine.Services;
using MonoLibrary.Engine.Services.Updates;

using System;
using System.IO;

namespace MonoLibrary.Engine;

public class GameEngine : Game
{
    private IUpdateLoop updater;

    public IConfiguration Configuration { get; private set; }

    private IServiceScope scope;
    public new IServiceProvider Services => scope.ServiceProvider;

    private readonly GameStateHub _gameState = new();
    public IGameStateHub GameState => _gameState;

    private ILogger gameLogger;
    public ILogger GameLogger => gameLogger;

    public GameEngine()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    /// <summary>
    /// Initializes <see cref="Configuration"/> and <see cref="Services"/>.
    /// </summary>
    protected override void Initialize()
    {
        var configBuilder = GetConfigurationBuilder();
        OnConfigure(configBuilder);
        Configuration = configBuilder.Build();

        var services = GetServiceCollection();
        OnConfigureServices(services, Configuration);

        scope = services.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        }).CreateScope();

        gameLogger = scope.ServiceProvider.GetService<ILogger<GameEngine>>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here

    }

    protected override void BeginRun()
    {
        base.BeginRun();

        updater ??= Services.GetRequiredService<IUpdateLoop>();
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            updater.BeforeUpdate();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            updater.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);

            updater.AfterUpdate();
        }
        catch (Exception e)
        {
            gameLogger.LogCritical(e, "Error in update loop.");
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            base.Draw(gameTime);
        }
        catch (Exception e)
        {
            gameLogger.LogCritical(e, "Error in draw loop.");
        }
    }

    protected static IConfigurationBuilder GetConfigurationBuilder()
    {
        var environmentName = Environment.GetEnvironmentVariable("Hosting:Environment");

        return new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", true)
                        .AddJsonFile($"appsettings.{environmentName ?? string.Empty}.json", true);
    }

    protected virtual void OnConfigure(IConfigurationBuilder builder)
    {

    }

    protected IServiceCollection GetServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSettings(typeof(GameEngine).Assembly, Configuration);
        services.AddSingleton<GameEngine>(this);
        services.AddSingleton<Game>(this);
        services.Configure<IConfiguration>(Configuration);

        services.AddOptions();

        services.AddLogging(builder =>
        {
            builder.AddConfiguration(Configuration.GetSection("Logging"));
            OnConfigureLogging(builder);
        });

        services.AddSingleton<IUpdateLoop, ServiceUpdater>(provider => ActivatorUtilities.CreateInstance<ServiceUpdater>(provider));
        //services.AddSingleton<IServiceRenderer, ServiceDrawer>(provider => ActivatorUtilities.CreateInstance<ServiceDrawer>(provider));

        services.AddSingleton<IGameStateHub, GameStateHub>(_ => _gameState);

        return services;
    }

    protected virtual void OnConfigureLogging(ILoggingBuilder logBuilder)
    {

    }

    protected virtual void OnConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        _gameState.OnExit();
        scope.Dispose();
    }
}
