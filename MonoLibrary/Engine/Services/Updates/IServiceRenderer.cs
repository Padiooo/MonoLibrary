namespace MonoLibrary.Engine.Services.Updates;

//public interface IServiceRenderer
//{
//    /// <summary>
//    /// Registers the <paramref name="rendererService"/>.
//    /// </summary>
//    /// <param name="rendererService"></param>
//    /// <returns>An <see cref="IDisposable"/> to unregister.</returns>
//    IDisposable Register(IRendererService rendererService);

//    /// <summary>
//    /// Ran after <see cref="Game.BeginDraw"/> if draw call will be done.
//    /// </summary>
//    void BeforeDraw();

//    /// <summary>
//    /// Ran before <see cref="Game.Draw(GameTime)"/>, hence before <see cref="GameObject.Draw(GameTime)"/>.
//    /// </summary>
//    void Draw(float deltaTime);

//    /// <summary>
//    /// Ran before <see cref="Game.EndDraw"/>.
//    /// </summary>
//    void AfterDraw();
//}

//public class ServiceDrawer: IServiceRenderer
//{
//    private readonly ILogger _logger;
//    private readonly List<IRendererService> _services = new();

//    private readonly Queue<IRendererService> _toRemove = new();

//    public ServiceDrawer(ILogger<IServiceRenderer> logger)
//    {
//        _logger = logger;
//    }

//    public IDisposable Register(IRendererService rendererService)
//    {
//        _services.Add(rendererService);
//        _logger.LogInformation("Registered {interface}: {type}. Total: {count}", nameof(IRendererService), rendererService.GetType().Name, _services.Count);

//        return new Subscription(_toRemove, rendererService);
//    }

//    public void BeforeDraw()
//    {
//        foreach (var service in _services)
//            service.BeforeDraw();

//        ClearQueue();
//    }

//    public void Draw(float deltaTime)
//    {
//        foreach (var service in _services)
//            service.Draw(deltaTime);

//        ClearQueue();
//    }

//    public void AfterDraw()
//    {
//        foreach (var service in _services)
//            service.AfterDraw();

//        ClearQueue();
//    }

//    private void Remove(IRendererService updatableService)
//    {
//        _services.Remove(updatableService);
//        _logger.LogInformation("Removed {interface}: {type}. Total: {count}", nameof(IRendererService), updatableService.GetType().Name, _services.Count);
//    }

//    private void ClearQueue()
//    {
//        while (_toRemove.Count > 0)
//            Remove(_toRemove.Dequeue());
//    }

//    private readonly struct Subscription : IDisposable
//    {
//        private readonly Queue<IRendererService> _toRemove;
//        private readonly IRendererService _updatableService;

//        public Subscription(Queue<IRendererService> actions, IRendererService updatableService)
//        {
//            _toRemove = actions;
//            _updatableService = updatableService;
//        }

//        public void Dispose()
//        {
//            _toRemove.Enqueue(_updatableService);
//        }
//    }
//}
