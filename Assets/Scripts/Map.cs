using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class Map
    {

        public bool loadMap(string level)
        {
            string[] splits = level.Split(';');
            string[] info = splits[0].Split(',');

            string[] size = info[0].Split(':');
            if (!int.TryParse(size[0], out width)) return false;
            
            if(size.Length == 1) height = width;
            else if (!int.TryParse(size[1], out height)) return false;

            if (info[1] != "0") return false;

            if (!int.TryParse(info[2], out levelInPage)) return false;

            if (!int.TryParse(info[3], out flowsNumber)) return false;

            flows = new List<Vector2Int>[flowsNumber];
            for (int i = 0; i < flowsNumber; i++)
            {
                string[] flowI = splits[i + 1].Split(',');
                flows[i] = new List<Vector2Int>();

                for (int j = 0; j < flowI.Length; j++)
                {
                    int pos;
                    if (!int.TryParse(flowI[j], out pos)) return false;

                    flows[i].Add(new Vector2Int(pos % width, pos / height));

                }
            }
            
            // Does the level have any gap?
            if (info.Length > 5)
            {
                string[] gaps = info[4].Split(':');
                _gaps = new List<Vector2Int>();
                for (int i = 0; i < gaps.Length; i++)
                {
                    int pos;
                    if (!int.TryParse(gaps[i], out pos)) return false;

                    _gaps.Add(new Vector2Int(pos / height, pos % width));
                }
            }
            
            // Does the level have any wall?
            if (info.Length > 6)
            {
                string[] walls = info[4].Split(':');
                _walls = new List<Tuple<Vector2Int, Vector2Int>>();
                for (int i = 0; i < walls.Length; i++)
                {
                    int pos;
                    if (!int.TryParse(walls[i], out pos)) return false;

                    _gaps.Add(new Vector2Int(pos / height, pos % width));
                }
            }

            return true;
        }

        public int getWidth()
        {
            return width;
        }

        public int getHeight()
        {
            return height;
        }

        public int getLevelInPage()
        {
            return levelInPage;
        }

        public int getFlowsNumber()
        {
            return flowsNumber;
        }

        public List<Vector2Int>[] getFlows()
        {
            return flows;
        }

        public List<Vector2Int> GetGaps()
        {
            return _gaps;
        }
        public List<Tuple<Vector2Int, Vector2Int>> GetWalls()
        {
            return _walls;
        }


        private int width, height;
        private int levelInPage;
        private int flowsNumber;
        
        private List<Vector2Int>[] flows;
        private List<Vector2Int> _gaps;
        private List<Tuple<Vector2Int, Vector2Int>> _walls;

    }
}