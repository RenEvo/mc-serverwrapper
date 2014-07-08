using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.Practices.Unity;

namespace Wrapper.Unity
{
    /// <summary>
    /// Class ContainerDependencyResolver
    /// </summary>
    public class UnityDependencyResolver : IDependencyResolver
    {
        /// <summary>
        /// The _container
        /// </summary>
        private readonly IUnityContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public UnityDependencyResolver(IUnityContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Retrieves a service from the scope.
        /// </summary>
        /// <param name="serviceType">The service to be retrieved.</param>
        /// <returns>The retrieved service.</returns>
        public object GetService(Type serviceType)
        {
            if (typeof(ApiController).IsAssignableFrom(serviceType))
                return _container.Resolve(serviceType);
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves a collection of services from the scope.
        /// </summary>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        /// <returns>The retrieved collection of services.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.ResolveAll(serviceType);
        }

        /// <summary>
        /// Starts a resolution scope.
        /// </summary>
        /// <returns>The dependency scope.</returns>
        public IDependencyScope BeginScope()
        {
            return this;
        }
    }
}
