using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class Map
    {
        private int _width, _height;                            // Width and height of the map. It may not be the same if the level is not a square one.
        private int _levelInPage;                               // The level the map represents (in the context of its pack).
        private int _flowsNumber;                               // The number of flows the level has.
        
        private List<Vector2Int>[] _flows;                      // The solution to the puzzle, as an array of lists (where the first and last element of each list represent circles, and the intermediate values represent the tiles that connect them). 
        private List<Vector2Int> _gaps;                         // The positions of the tiles that are gaps in the level.
        private List<Tuple<Vector2Int, Vector2Int>> _walls;     // The walls in the map (as a list of tuples, where each element of the list represents a wall, and each value in the tuple represents one of the tiles that form such wall).

        /// <summary>
        /// Creates a map from the given line. Stores information about the flows, gaps, walls, and everything that is needed to build the board later on.
        /// </summary>
        /// <param name="level">A line from the text file, which corresponds to a specific level.</param>
        /// <returns>true if the map could be loaded; false otherwise.</returns>
        public bool loadMap(string level)
        {
            // Splits every chunk of information.
            string[] splits = level.Split(';');
            
            // Splits the first part (the header), to read each value individually.
            string[] header = splits[0].Split(',');

            // Reads the size of the board, ignoring other characters.
            string[] size = header[0].Split(':', '+');
            if (!int.TryParse(size[0], out _width)) return false;
            
            // If there was only one digit, the board is a square one. Otherwise, reads the height.
            if(size.Length == 1) _height = _width;
            else if (!int.TryParse(size[1], out _height)) return false;

            // header[1] can be ignored. It is always 0, but it gives no information about the level.
            
            // Reads the level the level represents.
            if (!int.TryParse(header[2], out _levelInPage)) return false;

            // Reads the number of flows the level has.
            if (!int.TryParse(header[3], out _flowsNumber)) return false;

            // Creates a list for every flow, each one with the tiles it contains.
            _flows = new List<Vector2Int>[_flowsNumber];
            for (int i = 0; i < _flowsNumber; i++)
            {
                string[] currentFlow = splits[i + 1].Split(',');
                _flows[i] = new List<Vector2Int>();

                for (int j = 0; j < currentFlow.Length; j++)
                {
                    int pos;
                    if (!int.TryParse(currentFlow[j], out pos)) return false;
                    _flows[i].Add(new Vector2Int(pos % _width, pos / _width));
                }
            }
            
            // Does the level have any gap? In that case, they are stored in a list.
            _gaps = new List<Vector2Int>();
            if (header.Length > 5 && header[5] != "")
            {
                string[] gaps = header[5].Split(':');
                for (int i = 0; i < gaps.Length; i++)
                {
                    int pos;
                    if (!int.TryParse(gaps[i], out pos)) return false;
                    _gaps.Add(new Vector2Int(pos % _width, pos / _width));
                }
            }
            
            // Does the level have any wall? In that case, they are stored as couples of tiles, meaning the wall is the one that connects both of them.
            _walls = new List<Tuple<Vector2Int, Vector2Int>>();
            if (header.Length > 6 && header[6] != "")
            {
                string[] walls = header[6].Split(':');
                for (int i = 0; i < walls.Length; i++)
                {
                    string[] wallsCouple = walls[i].Split('|');
                    int posA, posB;

                    if (!int.TryParse(wallsCouple[0], out posA)) return false;
                    if (!int.TryParse(wallsCouple[1], out posB)) return false;
                    _walls.Add(new Tuple<Vector2Int, Vector2Int>
                    (new Vector2Int(posA % _width, posA / _width), new Vector2Int(posB % _width, posB / _width)));
                }
            }

            return true;
        }

        // ----- GETTERS ----- //
        public int GetWidth() { return _width; }

        public int GetHeight() { return _height; }

        public int GetLevelInPage() { return _levelInPage; }

        public int GetFlowsNumber() { return _flowsNumber; }

        public List<Vector2Int>[] GetFlows() { return _flows; }

        public List<Vector2Int> GetGaps() { return _gaps; }
        
        public List<Tuple<Vector2Int, Vector2Int>> GetWalls() { return _walls; }
    }
}