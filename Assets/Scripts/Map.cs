using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{

    public bool loadMap(string level)
    {
        string[] splits = level.Split(';', '\n');
        string[] info = splits[0].Split(',');

        string[] size = info[0].Split(':');
        if (!int.TryParse(size[0], out width)) return false;

        if (size.Length > 1 && !int.TryParse(size[1], out height)) return false;
        else height = width;

        if (info[1] != "0") return false;

        if (!int.TryParse(info[2], out levelInPage)) return false;
        Debug.Log($"Nivel en la pagina: {levelInPage}");

        if (!int.TryParse(info[3], out flowsNumber)) return false;

        flows = new List<Vector2>[flowsNumber];

        for (int i = 0; i < flowsNumber; i++)
        {
            string[] flowI = splits[i + 1].Split(',');
            flows[i] = new List<Vector2>();

            for (int j = 0; j < flowI.Length; j++)
            {
                int pos;
                if (!int.TryParse(flowI[j], out pos)) return false;

                flows[i].Add(new Vector2(pos % width, pos / height));
            }
        }

        return true;
    }

    // [TODO] GETTERS

    public int width, height;
    // reservado
    public int levelInPage;
    public int flowsNumber;
    // puentes
    // celdas huecas
    // muros


    public List<Vector2>[] flows;
}
