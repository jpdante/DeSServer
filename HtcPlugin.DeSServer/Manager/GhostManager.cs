using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Model;

namespace HtcPlugin.DeSServer.Manager {
    public class GhostManager {

        private readonly List<Ghost> _wanderingGhosts;

        public GhostManager() {
            _wanderingGhosts = new List<Ghost>();
        }

        public Task Enable() {
            return Task.CompletedTask;
        }

        public Task Disable() {
            return Task.CompletedTask;
        }

        public Ghost[] GetWanderingGhosts(string playerId, int max, int blockId) {
            return _wanderingGhosts.Where(x => !x.PlayerId.Equals(playerId) && x.BlockId == blockId).Take(max).OrderBy(a => Guid.NewGuid()).ToArray();
        }

        public void AddWanderingGhost() {
            _wanderingGhosts.Add(new Ghost());
        }
    }
}