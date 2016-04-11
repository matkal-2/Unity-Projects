using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldMap : MonoBehaviour {

    public int mapSizeX, mapSizeZ;
    public Texture2D heightmap;
    private Dictionary<string, MapTile> tilesDictionary;
    public Terrain terrain;
    private TerrainData terrainData;

    void Awake() {
        Debug.Log("Awake WorldMap");
        MapTile tempMapTile;
        BitMapDecoder bmd = new BitMapDecoder(heightmap);
        terrainData = terrain.terrainData;
        int heighMapWidth = terrainData.heightmapWidth;
        int heighMapHeight = terrainData.heightmapHeight;
        float[,] tempFloat = terrainData.GetHeights(0, 0, heighMapWidth, heighMapHeight);
        tilesDictionary = new Dictionary<string, MapTile>();

        
        for (int z = 0; z < mapSizeZ; z++ ) {
            for (int x = 0; x < mapSizeX; x++) {
                tempMapTile = new MapTile(x,z);
                int height = BitMapDecoder.getHeightPos(x, z);
                tempMapTile.setY(height);
                //tempFloat[x*2+1, z*2+1] = height / 5;
                tempFloat[x * 2, z*2 * 2] = (float)height / 10;
                //tempFloat[x*2, z * 2+1] = height / 5;
                //tempFloat[x * 2 + 1, z * 2] = height / 5;
                
                tilesDictionary.Add(("x" + x.ToString() + "z" + z.ToString()), tempMapTile);
            }
        }
        terrain.terrainData.SetHeights(0, 0, tempFloat);
    }

    public int getTileHeight(int x, int z)
    {

        return tilesDictionary[("x"+x.ToString()+"z"+z.ToString())].getY();
    }
}
