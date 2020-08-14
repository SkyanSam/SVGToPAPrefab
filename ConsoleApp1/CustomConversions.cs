using System;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections;

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
        /// <summary>
        /// Specifically for converting the @D attribute to points on the path
        /// </summary>
        /// <param name="nextVal"></param>
        /// <param name="xArray"></param>
        /// <param name="yArray"></param>
        /// <param name="points"></param>
        public static void DPathToPoints(string nextVal, out float[] xArray, out float[] yArray, out Vector2[] points)
        {
            // Splitting the values
            string[] rawArray = nextVal.Split('M', 'L', 'Z', ' ', 'z', 'l', 'm');
            ArrayList arraylist = new ArrayList();

            // We dont want to add empty stuff
            for (int i = 0; i < rawArray.Length; i++)
                if (rawArray[i] != "") arraylist.Add(rawArray[i]);

            foreach (var item in arraylist)
            {
                Console.WriteLine("/" + item + "/");
            }

            // We want to convert the string values to float
            float[] floatArray = new float[arraylist.Count];
            for (int i = 0; i < arraylist.Count; i++)
                floatArray[i] = float.Parse((string)arraylist[i]);

            Console.WriteLine(floatArray.Length);
            // If nothing else we can assume the shape is null.

            // Create float array for x and y.
            ArrayList xArrayList = new ArrayList();
            ArrayList yArrayList = new ArrayList();

            // Length of x&y array list = floatArray.Length / 2
            for (int i = 0; i < floatArray.Length; i += 2)
            {
                xArrayList.Add(floatArray[i]);
                yArrayList.Add(floatArray[i + 1]);
            }

            // Remove points if there is the same 2 points
            Vector2 startingPoint = new Vector2((float)xArrayList[0], (float)yArrayList[0]);
            for (int i = 1; i < xArrayList.Count; i++)
            {
                if (startingPoint.X == (float)xArrayList[i] && startingPoint.Y == (float)yArrayList[i])
                {
                    xArrayList.RemoveAt(i);
                    yArrayList.RemoveAt(i);
                }
            }

            // DEBUG
            Console.WriteLine("DEBUG LIST ///");
            for (int i = 0; i < xArrayList.Count; i++)
            {
                Console.WriteLine((float)xArrayList[i] + "  ,  " + (float)yArrayList[i]);
            }
            Console.WriteLine("END DEBUG ///");
            //

            xArray = (float[])xArrayList.ToArray(typeof(float));
            yArray = (float[])yArrayList.ToArray(typeof(float));


            points = new Vector2[xArray.Length];
            for (int i = 0; i < xArray.Length; i++)
            {
                points[i] = new Vector2(xArray[i], yArray[i]);
            }
        }
        
    }
}
