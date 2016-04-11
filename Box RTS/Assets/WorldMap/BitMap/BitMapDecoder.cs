using UnityEngine;
using System.Collections;

public class BitMapDecoder {

    public static Texture2D heightmap;

    public BitMapDecoder(Texture2D heightMap)
    {
        heightmap = heightMap;
    }

    public static int getHeightPos(int x, int z)
    {
        int pos = x + (z * 100);
        if (heightmap.GetPixels32()[pos].b <= 20){
            //Debug.Log("bitMap x: " + x.ToString() + " z: " + z.ToString() + " Heigh: " + heightmap.GetPixels32()[pos].b.ToString());
            return 5;
        }
        //Debug.Log("bitMap x: " + x.ToString() + " z: " + z.ToString() + " Heigh: " + heightmap.GetPixels32()[pos].b.ToString());
        return 0; 

    }
}
