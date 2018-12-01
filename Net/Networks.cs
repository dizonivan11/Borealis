using System.Collections.Generic;

namespace Borealis.Net {
    public class Networks : Dictionary<Network, Dictionary<string, dynamic>> {

        // TODO: Add many helper methods
        public void AddNew(Network newNetwork) {
            Add(newNetwork, new Dictionary<string, dynamic>());
        }
    }
}
