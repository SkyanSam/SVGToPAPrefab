using System;
using System.Linq;
using System.Collections;
using PA_PrefabBuilder;
using System.Numerics;
using SVGToPrefab.Custom;

namespace SVGToPrefab
{
    class DataAppliers
    {
        public static void ApplyThisVal(ref GameObjectData obj, string value, string nextVal, float nextValf)
        {
            Shapes? shape = null;
            if (Program.IsItemIsInArray(value, new string[] { "@d" })) D(ref obj, nextVal, out shape);
            if (shape != null) obj.shape = shape.Value;

            if (Program.IsItemIsInArray(value, new string[] { "@x", "@cx" })) X(ref obj, nextValf);
            if (Program.IsItemIsInArray(value, new string[] { "@y", "@cy" })) Y(ref obj, nextValf);
            if (Program.IsItemIsInArray(value, new string[] { "@width", "@rx" })) sizeX(ref obj, nextValf, value);
            if (Program.IsItemIsInArray(value, new string[] { "@height", "@ry" })) sizeY(ref obj, nextValf, value);
            if (Program.IsItemIsInArray(value, new string[] { "@transform" })) translate(ref obj, nextVal);
            if (Program.IsItemIsInArray(value, new string[] { "@r" })) size(ref obj, nextValf);
            if (Program.IsItemIsInArray(value, new string[] { "@fill", "@stroke" })) fill(ref obj, nextVal);
            if (Program.IsItemIsInArray(value, new string[] { "@stroke" })) stroke(ref obj);
        }

        /// <summary>
        /// Applier for custom path.
        /// </summary>
        public static void D(ref GameObjectData obj, string nextVal, out Shapes? shape)
        {
            shape = null;
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

            float[] xArray = (float[])xArrayList.ToArray(typeof(float));
            float[] yArray = (float[])yArrayList.ToArray(typeof(float));


            Vector2[] points = new Vector2[xArray.Length];
            for (int i = 0; i < xArray.Length; i++)
            {
                points[i] = new Vector2(xArray[i], yArray[i]);
            }

            float SizeMultiplier = 1;

            // Analyze the number of points. We multiply by two because its split up by float, not by vector / float[2] then add two because of [M x y]
            if (points.Length == 3)
            {
                shape = Shapes.Triangle;
                SizeMultiplier = 2;
            }

            else if (points.Length == 4) shape = Shapes.Square;
            else if (points.Length == 6) shape = Shapes.Hexagon;

            // Get min and max
            float[] min = new float[] { xArray.Min(), yArray.Min() };
            float[] max = new float[] { xArray.Max(), yArray.Max() };

            Console.WriteLine("minmax");
            Console.WriteLine(min[0] + "," + min[1]);
            Console.WriteLine(max[0] + "," + max[1]);
            Console.WriteLine("minmax");
            // Get center and size
            float[] center = CustomMath.GetCenter(min, max);

            // Everything with final means its been rotated.
            Vector2[] finalPoints;
            float finalRotation;
            bool isRightTriangle;
            CustomMath.Rotations.GetRotatedShape(1f, CustomConversions.Float2ToVect(center), points, out finalPoints, out finalRotation, out isRightTriangle);

            if (isRightTriangle && points.Length == 3) obj.shapeVariant = 2;

            float[] finalXPoints = CustomConversions.VectIndexToFloatList(0, finalPoints);
            float[] finalYPoints = CustomConversions.VectIndexToFloatList(1, finalPoints);

            float[] finalMin = new float[] { finalXPoints.Min(), finalYPoints.Min() };
            float[] finalMax = new float[] { finalXPoints.Max(), finalYPoints.Max() };

            float[] finalSize = CustomMath.GetSize(finalMin, finalMax);


            Console.WriteLine(center[0]);
            Console.WriteLine(center[1]);
            Console.WriteLine(finalSize[0]);
            Console.WriteLine(finalSize[1]);

            // Apply center and size
            obj.positionX = center[0];
            obj.positionY = center[1];
            obj.sizeX = finalSize[0] * SizeMultiplier;
            obj.sizeY = finalSize[1] * SizeMultiplier;
            obj.rotAngle = finalRotation;
        }
        public static void X(ref GameObjectData obj, float nextValf)
        {
            obj.positionX = nextValf;
        }
        public static void Y(ref GameObjectData obj, float nextValf)
        {
            obj.positionY = nextValf;
        }
        public static void sizeX(ref GameObjectData obj, float nextValf, string value)
        {
            int multiplier = 1;
            if (value == "@rx") multiplier = 2; // If this is an ellipse we want twice as much
            obj.sizeX = nextValf * multiplier;
        }
        public static void sizeY(ref GameObjectData obj, float nextValf, string value)
        {
            int multiplier = 1;
            if (value == "@ry") multiplier = 2; // If this is an ellipse we want twice as much
            obj.sizeY = nextValf * multiplier;
        }
        public static void size(ref GameObjectData obj, float nextValf)
        {
            obj.sizeY = obj.sizeX = nextValf * 2;
        }
        public static void fill(ref GameObjectData obj, string nextVal)
        {
            int id;
            if (Program.IsItemIsInArray(nextVal, Input.Colors.ids, out id))
            {
                obj.colorNum = id; // Affirmative, colors count from 0.
            }
        }
        public static void stroke(ref GameObjectData obj)
        {
            obj.shapeVariant = 1;
        }
        public static void rotation(ref GameObjectData obj, float nextValf)
        {
            obj.rotAngle = nextValf;
        }
        public static void translate(ref GameObjectData obj, string nextVal)
        {
            string[] vals = nextVal.Split('(', ')', ','); // Questioning if i should split for empty spaces..
            if (vals[0] == "matrix")
            {
                float[] matrix = new float[vals.Length - 1];

                for (int i = 1; i < vals.Length - 1; i++)
                {
                    matrix[i - 1] = float.Parse(vals[i]);
                }
                float posXdelta;
                float posYdelta;
                float sizeX; // Not valid at this point I think
                float sizeY; // Not valid at this point I think
                float rotation;
                CustomConversions.GetVarsFromMatrix(matrix, out posXdelta, out posYdelta, out sizeX, out sizeY, out rotation);
                obj.offsetX = 0;
                obj.offsetY = 0;
                obj.positionX += 0.5f;
                obj.rotAngle = rotation;
                // I NEED TO MAKE THE OFFSET CENTER AND THEN PUSH EVERYTHING BY 0.5
            } else if (vals[0] == "rotate")
            {
                obj.rotAngle = float.Parse(vals[1]);
            }
        }
        struct Multi { 
            public static void PathOutline(ref GameObjectData obj, string nextVal, out GameObjectData[] objs)
            {

            }
        
        }

    }
}
