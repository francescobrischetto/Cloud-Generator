using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perlinNoise
{
    //Hash array is producting a tiling pattern that we notice only when we see a large sampling of it. 
    //This particular permutation of 256 values (repeated 2 times for indexing reasons) is used in Perlin reference implementation.
    //This is not absolutely required because it does require a random array of the values [0-255] but we should ensure uniform distribution of those values.
    private static int[] permutation = {
        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
        140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
        247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
         57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
         74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
         60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
         65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
        200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
         52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
        207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
        119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
        129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
        218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
         81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
        184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
        222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
        140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
        247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
         57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
         74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
         60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
         65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
        200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
         52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
        207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
        119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
        129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
        218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
         81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
        184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
        222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
    };

    const int permutationCount = 255;

    //Perlin found that you can use just 12 gradients, however we extend it to 16 to use bit masking in the calculations
    private static Vector3[] directions = {
        new Vector3( 1f, 1f, 0f),
        new Vector3(-1f, 1f, 0f),
        new Vector3( 1f,-1f, 0f),
        new Vector3(-1f,-1f, 0f),
        new Vector3( 1f, 0f, 1f),
        new Vector3(-1f, 0f, 1f),
        new Vector3( 1f, 0f,-1f),
        new Vector3(-1f, 0f,-1f),
        new Vector3( 0f, 1f, 1f),
        new Vector3( 0f,-1f, 1f),
        new Vector3( 0f, 1f,-1f),
        new Vector3( 0f,-1f,-1f),

        new Vector3( 1f, 1f, 0f),
        new Vector3(-1f, 1f, 0f),
        new Vector3( 0f,-1f, 1f),
        new Vector3( 0f,-1f,-1f)
    };

    private const int directionCount = 15;

    private static float dotProduct(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    //This is used to don't get sharp transition. Instead of using linear distance from points, we turn it into smooth curve using the function
    // -6t^5 - 15t^4 +10t^3 which derivative is zero at both ends
    private static float smoothDistance(float t)
    {

        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    public static float PerlinNoise3D(Vector3 point, float discreteStep)
    {
        point *= 1 / discreteStep;

        int flooredPointX0 = Mathf.FloorToInt(point.x);
        int flooredPointY0 = Mathf.FloorToInt(point.y);
        int flooredPointZ0 = Mathf.FloorToInt(point.z);
        //those bitwise and operations are used to constraint the number to stay inside the permutation vector.
        flooredPointX0 &= permutationCount;
        flooredPointY0 &= permutationCount;
        flooredPointZ0 &= permutationCount;

        int ceilingPointX1 = flooredPointX0 + 1;
        int ceilingPointY1 = flooredPointY0 + 1;
        int ceilingPointZ1 = flooredPointZ0 + 1;

        int permutationX0 = permutation[flooredPointX0];
        int permutationX1 = permutation[ceilingPointX1];

        int permutationY00 = permutation[permutationX0 + flooredPointY0];
        int permutationY10 = permutation[permutationX1 + flooredPointY0];
        int permutationY01 = permutation[permutationX0 + ceilingPointY1];
        int permutationY11 = permutation[permutationX1 + ceilingPointY1];

        //NOTE: the following procedure is the same as writing this to find the 8 directions:
        //Vector 3 direction000 = directions[permutation[permutation[permutation[flooredPointX0] + flooredPointY0] + flooredPointZ0] & directionCount];
        Vector3 direction000 = directions[permutation[permutationY00 + flooredPointZ0] & directionCount];
        Vector3 direction100 = directions[permutation[permutationY10 + flooredPointZ0] & directionCount];
        Vector3 direction010 = directions[permutation[permutationY01 + flooredPointZ0] & directionCount];
        Vector3 direction110 = directions[permutation[permutationY11 + flooredPointZ0] & directionCount];
        Vector3 direction001 = directions[permutation[permutationY00 + ceilingPointZ1] & directionCount];
        Vector3 direction101 = directions[permutation[permutationY10 + ceilingPointZ1] & directionCount];
        Vector3 direction011 = directions[permutation[permutationY01 + ceilingPointZ1] & directionCount];
        Vector3 direction111 = directions[permutation[permutationY11 + ceilingPointZ1] & directionCount];


        float distanceX0 = point.x - flooredPointX0;
        float distanceY0 = point.y - flooredPointY0;
        float distanceZ0 = point.z - flooredPointZ0;

        float distanceX1 = distanceX0 - 1f;
        float distanceY1 = distanceY0 - 1f;
        float distanceZ1 = distanceZ0 - 1f;

        //Dot products of the directions of the 8 corners with the offset vector to the candidate point
        float value000 = dotProduct(direction000, new Vector3(distanceX0, distanceY0, distanceZ0));
        float value100 = dotProduct(direction100, new Vector3(distanceX1, distanceY0, distanceZ0));
        float value010 = dotProduct(direction010, new Vector3(distanceX0, distanceY1, distanceZ0));
        float value110 = dotProduct(direction110, new Vector3(distanceX1, distanceY1, distanceZ0));
        float value001 = dotProduct(direction001, new Vector3(distanceX0, distanceY0, distanceZ1));
        float value101 = dotProduct(direction101, new Vector3(distanceX1, distanceY0, distanceZ1));
        float value011 = dotProduct(direction011, new Vector3(distanceX0, distanceY1, distanceZ1));
        float value111 = dotProduct(direction111, new Vector3(distanceX1, distanceY1, distanceZ1));


        //Linear interpolation with smoothstep function
        float smoothDistanceX = smoothDistance(distanceX0);
        float smoothDistanceY = smoothDistance(distanceY0);
        float smoothDistanceZ = smoothDistance(distanceZ0);

        return Mathf.Lerp(
            Mathf.Lerp(Mathf.Lerp(value000, value100, smoothDistanceX), Mathf.Lerp(value010, value110, smoothDistanceX), smoothDistanceY),
            Mathf.Lerp(Mathf.Lerp(value001, value101, smoothDistanceX), Mathf.Lerp(value011, value111, smoothDistanceX), smoothDistanceY),
            smoothDistanceZ);
    }
    
}
