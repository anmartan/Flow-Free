using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{

    public bool loadMap(string level)
    {
        string[] splits = level.Split(';');
        string[] info = splits[0].Split(',');

        string[] size = info[0].Split(':');
        if (!int.TryParse(size[0], out width)) return false;

        if (size.Length > 1 && !int.TryParse(size[1], out height)) return false;
        height = width;

        if (info[1] != "0") return false;

        if (!int.TryParse(info[2], out levelInPage)) return false;

        if (!int.TryParse(info[3], out flowsNumber)) return false;

        flows = new List<Vector2Int>[flowsNumber];
        _flows = new List<TileInfo>[flowsNumber];
        for (int i = 0; i < flowsNumber; i++)
        {
            string[] flowI = splits[i + 1].Split(',');
            flows[i] = new List<Vector2Int>();
            _flows[i] = new List<TileInfo>();

            for (int j = 0; j < flowI.Length; j++)
            {
                int pos;
                if (!int.TryParse(flowI[j], out pos)) return false;

                flows[i].Add(new Vector2Int(pos / height, pos % width));

                TileInfo tile = new TileInfo();
                tile._position = new Vector2Int(pos / height, pos % width);

            }
        }

        return true;
    }
    public int getWidth() { return width; }
    public int getHeight() { return height; }
    public int getLevelInPage() { return levelInPage; }
    public int getFlowsNumber() { return flowsNumber; }
    public List<Vector2Int>[] getFlows() { return flows; }



    private int width, height;
    // reservado
    private int levelInPage;
    private int flowsNumber;
    // puentes
    // TODO erase
    private List<Vector2Int>[] flows;

    private List<TileInfo>[] _flows;
    struct TileInfo
    {
        public bool _isEmpty;          // If it is empty, the player won't be able to move to this position. 
        public bool[] _walls;          // North, South, East and West, in that order.
        public Vector2Int _position;   // Expressed as (row, column).
    }
}
