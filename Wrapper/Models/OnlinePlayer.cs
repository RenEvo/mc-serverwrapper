using System;

namespace Wrapper.Models
{
    /// <summary>
    /// Class OnlinePlayer
    /// </summary>
    public class OnlinePlayer
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>The duration.</value>
        public TimeSpan Duration { get; set; }
    }
}
