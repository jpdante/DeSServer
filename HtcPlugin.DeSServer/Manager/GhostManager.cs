using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Model;
using HtcSharp.Core.Logging.Abstractions;

namespace HtcPlugin.DeSServer.Manager {
    public class GhostManager {

        private readonly Dictionary<string, Ghost> _wanderingGhosts;

        public GhostManager() {
            _wanderingGhosts = new Dictionary<string, Ghost>();
        }

        public Task Enable() {
            return Task.CompletedTask;
        }

        public Task Disable() {
            return Task.CompletedTask;
        }

        public Ghost[] GetWanderingGhosts(string playerId, uint blockId, int max) {
            DeleteWanderingGhosts();
            HtcPlugin.Logger.LogInfo($"[GhostManager] Get wandering ghosts");
            return _wanderingGhosts.Values.Where(x => !x.PlayerId.Equals(playerId) && x.BlockId == blockId).Take(max).OrderBy(a => Guid.NewGuid()).ToArray();
        }

        public void AddWanderingGhost(Ghost ghost) {
            HtcPlugin.Logger.LogInfo($"[GhostManager] Add {ghost.PlayerId} wandering ghost");
            if (_wanderingGhosts.ContainsKey(ghost.PlayerId)) _wanderingGhosts[ghost.PlayerId] = ghost;
            else _wanderingGhosts.Add(ghost.PlayerId, ghost);
        }

        public void DeleteWanderingGhosts() {
            foreach (string key in _wanderingGhosts.Keys.Where(key => _wanderingGhosts[key].CreationTime.AddSeconds(30) <= DateTime.Now)) {
                HtcPlugin.Logger.LogInfo($"[GhostManager] Delete {key} wandering ghost");
                _wanderingGhosts.Remove(key);
            }
        }
    }
}