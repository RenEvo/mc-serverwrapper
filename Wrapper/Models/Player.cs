using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrapper.Models
{
    /// <summary>
    /// Class Player
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is online.
        /// </summary>
        /// <value><c>true</c> if this instance is online; otherwise, <c>false</c>.</value>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Gets or sets the online time.
        /// </summary>
        /// <value>The online time.</value>
        public TimeSpan OnlineTime { get; set; }

        /// <summary>
        /// Gets or sets the total hours on server.
        /// </summary>
        /// <value>The total hours on server.</value>
        public double TotalHoursOnServer { get; set; }

        /// <summary>
        /// Gets or sets the login count.
        /// </summary>
        /// <value>The login count.</value>
        public int LoginCount { get; set; }

        /// <summary>
        /// Gets or sets the last seen.
        /// </summary>
        /// <value>The last seen.</value>
        public string LastSeen { get; set; }
    }
}
