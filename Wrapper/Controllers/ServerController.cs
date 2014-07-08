using System;
using System.Globalization;
using System.Web.Http;

namespace Wrapper.Controllers
{
    /// <summary>
    /// Class ServerController
    /// </summary>
    public class ServerController : ApiController
    {
        /// <summary>
        /// Times this instance.
        /// </summary>
        /// <returns>System.String.</returns>
        [AcceptVerbs("GET")]
        public string Time()
        {
            return DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
    }
}
