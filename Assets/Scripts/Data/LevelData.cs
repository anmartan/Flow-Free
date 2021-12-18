using UnityEngine;

namespace FlowFree
{
    /// <summary>
    /// The state of a level (for saving and loading it later on).
    /// Has it been previously solved? Was it a perfect solve, or could it be improved?
    /// </summary>
    public enum LevelState
    {
        UNSOLVED,
        SOLVED,
        PERFECT
    }
    
    /// <summary>
    /// Information about the levels (for saving and loading later on).
    /// </summary>
    public class LevelData
    {
        public int LevelNumber;         // The number of the level inside a level pack (normally in the range [0, 149], but it could be more.
        public int PackNumber;          // The number of the pack inside a level category.
        public int CategoryNumber;      // The number of the level category.
        public string Data;             // The information used for creating the level (the numbers in the file).
        public Color Color;             // The color that represents the level (TODO: what is this used for?).

        public int BestSolve;           // The number of movements used in the best solved. Ignored id the level has not been solved.
        public LevelState State;        // The state of the level (whether it has been solved before or not, and whether it was a perfect solve or not).
    }
}