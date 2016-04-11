using UnityEngine;
using System.Collections;

public class MapTile{
    private int x;
    private int z;
    private int y;

    private bool walkable { get; set; }
    private bool placeable { get; set; }

    public MapTile(int xCord, int zCord)
    {
        this.x = xCord;
        this.z = zCord;
        walkable = true;
        placeable = true;
        y = 0;
        
    }

    public int getY()
    {
        return this.y;
    }

    public void setY(int yCord)
    {
        this.y = yCord;
    }
}
