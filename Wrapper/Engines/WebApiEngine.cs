using System;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Microsoft.Practices.Unity;
using Wrapper.Unity;

namespace Wrapper.Engines
{
    /// <summary>
    /// Class WebApiHost
    /// </summary>
    public class WebApiEngine : IApiEngine
    {
        /// <summary>
        /// The _container
        /// </summary>
        private readonly IUnityContainer _container;

        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogEngine _logger;

        /// <summary>
        /// The _enabled
        /// </summary>
        private readonly bool _enabled;

        /// <summary>
        /// The _port
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public HttpSelfHostConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>The server.</value>
        public HttpSelfHostServer Server { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiEngine"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// container
        /// or
        /// logger
        /// </exception>
        public WebApiEngine(IUnityContainer container, ILogEngine logger)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (logger == null) throw new ArgumentNullException("logger");
            _container = container;
            _logger = logger;

            _port = MinecraftSettings.Default.WebApiPort;

            if (_port != 0)
                _enabled = true;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            if (!_enabled)
                return;

            Configuration = new HttpSelfHostConfiguration(string.Format("http://locahost:{0}", _port))
                {
                    DependencyResolver = new UnityDependencyResolver(_container),
                };

            Configuration.Routes.MapHttpRoute("API Default", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            Configuration.Formatters.Remove(Configuration.Formatters.XmlFormatter);

            Server = new HttpSelfHostServer(Configuration);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (Server == null)
                Initialize();

            if (!_enabled || Server == null)
                return;

            Server.OpenAsync().Wait();
            _logger.Write("Started Web Api Server on Port: {0}", _port);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            if (!_enabled || Server == null)
                return;

            Server.CloseAsync().Wait();
            _logger.Write("Closed Web Api Server on Port: {0}", _port);
        }
    }
}
