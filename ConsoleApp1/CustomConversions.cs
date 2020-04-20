using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;

namespace SVGToPrefab.Custom
{
    struct CustomConversions
    {
        public static float[] VectToFloat2(Vector2 vector)
        {
            return new float[] { vector.X, vector.Y };
        }
        public static Vector2 Float2ToVect(float[] float2)
        {
            return new Vector2(float2[0], float2[1]);
        }
        public static float[] VectIndexToFloatList(int index, Vector2[] vectors)
        {
            float[] rFloatArray = new float[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
                if (index == 1) rFloatArray[i] = vectors[i].Y;
                else rFloatArray[i] = vectors[i].X;

            return rFloatArray;
        }
        public static void GetVarsFromMatrix(float[] matrix, out float posXdelta, out float posYdelta, out float sizeX, out float sizeY, out float rot)
        {
            posXdelta = matrix[4];
            posYdelta = matrix[5];
            sizeX = matrix[0];
            sizeY = matrix[3];
            //var skewX = matrix[1];
            var skewY = matrix[2];
            rot = MathF.Asin(skewY) * 180 / MathF.PI;
        }
        
    }
}
