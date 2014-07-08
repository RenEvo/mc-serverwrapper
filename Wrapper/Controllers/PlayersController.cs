using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Wrapper.Managers;
using Wrapper.Models;

namespace Wrapper.Controllers
{
    /// <summary>
    /// Class PlayersController
    /// </summary>
    public class PlayersController : ApiController
    {
        /// <summary>
        /// The _player tracker
        /// </summary>
        private readonly IPlayerManager _playerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersController" /> class.
        /// </summary>
        /// <param name="playerManager">The player manager.</param>
        /// <exception cref="System.ArgumentNullException">playerManager</exception>
        public PlayersController(IPlayerManager playerManager)
        {
            if (playerManager == null) throw new ArgumentNullException("playerManager");
            _playerManager = playerManager;
        }

        /// <summary>
        /// Onlines this instance.
        /// </summary>
        /// <returns>IEnumerable{OnlinePlayer}.</returns>
        [AcceptVerbs("GET")]
        public IEnumerable<OnlinePlayer> Online()
        {
            return _playerManager.OnlineUsers.Select(x => new OnlinePlayer { Name = x.Key, Duration = x.Value });
        }

        /// <summary>
        /// Playerses this instance.
        /// </summary>
        /// <returns>IEnumerable{System.String}.</returns>
        [AcceptVerbs("GET")]
        public IEnumerable<Player> List()
        {
            var playerList = _playerManager.Whitelist.OrderBy(x => x).Select(x => new Player { Name = x }).ToArray();

            var currentTime = DateTime.Now;

            foreach (var player in playerList)
            {
                var playerStats = _playerManager.GetPlayerPlayStats(player.Name);

                player.IsOnline = _playerManager.OnlineUsers.ContainsKey(player.Name);

                player.OnlineTime = _playerManager.OnlineUsers.ContainsKey(player.Name) 
                    ? _playerManager.OnlineUsers[player.Name] 
                    : TimeSpan.Zero;
                
                player.LastSeen = player.IsOnline
                    ? currentTime.ToString(CultureInfo.InvariantCulture) 
                    : playerStats.Item3 != DateTime.MinValue 
                        ? playerStats.Item3.ToString(CultureInfo.InvariantCulture) 
                        : "";

                player.TotalHoursOnServer = playerStats.Item1;

                player.LoginCount = player.IsOnline
                    ? playerStats.Item2 + 1 // increment by 1 if they are online
                    : playerStats.Item2; 
            }
            
            return playerList.OrderByDescending(x => x.LastSeen).ThenByDescending(x => x.TotalHoursOnServer);
        }

    }
}
