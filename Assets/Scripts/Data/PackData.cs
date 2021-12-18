using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    /// <summary>
    /// Information about the levels packs (for saving and loading later on).
    /// </summary>
    public struct PackData
    {
        public List<LevelData> LevelsData;      // The levels included in this pack (as information).
        public string PackName;                 // The name of the pack.
    }
}
