using System;
using System.Collections;
using System.IO;
using PA_PrefabBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Linq;
using System.Numerics;

namespace ConsoleApp1
{
    public static class UserOptions // Just modify these variables
    {
        public static string prefabName = "Prefab Generated From SVG";
        public static PrefabType prefabType = PrefabType.Misc1;
        public static float secondsToLast = 15f;
        public static string svgPath = @"D:/Documents/CSharpProjects/RotatedHell.svg";
        public static string prefabPath = @"C:\Program Files (x86)\Steam\steamapps\common\Project Arrhythmia\beatmaps\prefabs";
    }
    class Colors {
        /* Colors corresponding with their ids. this must be auto filled by the user.
         * NOTE: Need to be careful. Not sure if svg also used hex or hsv. May need to convert later */
        public static string[] ids = new string[10]
        {
        "rgb(255,153,0)",
        "rgb(0,51,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        "rgb(0,0,0)",
        };
    }
    class GameObjectData
    {
        public int ID;
        public int shapeVariant; // Fill is 0, stroke is 1
        public Shapes? shape;
        public float positionX;
        public float positionY;
        public float sizeX;
        public float sizeY;
        public float offsetX;
        public float offsetY;
        public float rotAngle;
        public int colorNum = 0;
        public GameObjectData()
        {

        }
        public void SetOffsetTo(float x, float y)
        {
            offsetX = x;
            offsetY = y;
        }
        public GameObject ToObj()
        {
            string objname = "GmObj" + ID;
            if (shape != null) objname += " " + shape.Value;

            GameObject obj = new GameObject(ID.ToString(), objname, shape.Value);
            obj.ShapeVariant = shapeVariant;
            // Because pivot is top left corner
            obj.OffsetX = offsetX;
            obj.OffsetY = offsetY;
            //
            obj.SetPosition(positionX, -positionY);
            obj.SetScale(sizeX, sizeY);
            obj.SetRotation(rotAngle);
            obj.SetColor(colorNum);

            // Animation / Making sure the object lasts
            // 
            //Event e = new Event(EventType.col, UserOptions.secondsToLast);
            GameObject deepObj = obj.Clone();
            deepObj.AddEvent(EventType.col, UserOptions.secondsToLast, colorNum, null, Easing.Linear);
            return deepObj;
        }
        public override string ToString()
        {
            string printShape;
            if (shape == null) printShape = null;
            else printShape = shape.Value.ToString();

            base.ToString();
            return
                base.ToString() +
                "id: " + ID + ", " +
                "posX: " + positionX + ", " +
                "posY: " + positionY + ", " +
                "sizeX: " + sizeX + ", " +
                "sizeY: " + sizeY + ", " +
                "offsetX: " + offsetX + ", " +
                "offsetY: " + offsetY + ", " +
                "ang: " + rotAngle + ", " +
                "color: " + colorNum + ", " +
                "shape: " + printShape + ", " +
                "variant: " + shapeVariant;

             ;
        }
        public static string[] varLabels = new string[] { "@x", "@y", "@width", "@height", "@cx", "@cy", "@r", "@rx", "@ry" };
    }
    class Program
    {
        
        static void Main(string[] args)
        {
            // XML -> JSON
            string jStr = XMLToJSON(UserOptions.svgPath);

            // JSON -> GameObjectData[] {Custom Class}
            ArrayList gameObjectDataList = JSONToGameObjectData(jStr);
            foreach (var data in gameObjectDataList) Console.WriteLine(((GameObjectData)data).ToString());

            // GameObjectData[] -> GameObject[]
            GameObject[] objects = ConvertDataListToGameObjects(gameObjectDataList);

            // GameObject[] -> Prefab!
            PrefabBuilder pb = new PrefabBuilder(UserOptions.prefabName, UserOptions.prefabType, 0);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].Bin = i; // This may be a problem if I > 14.
                    pb.Objects.Add(objects[i]);
                }
            }

            // Prefab -> Project Arrythmia!!
            pb.Export(UserOptions.prefabPath);

        }
        static string XMLToJSON(string _path)
        {
            string xml = File.ReadAllText(_path);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string jsonTxt = JsonConvert.SerializeXmlNode(doc);
            return jsonTxt;
            
        }
        static ArrayList JSONToGameObjectData(string json)
        {
            ArrayList gameObjectsData = new ArrayList();
            int cItem = -1; // current Item
            string currentType = "";
            bool objectStarted = false;

            ArrayList tokenTypes = new ArrayList(); //TokenType
            ArrayList valueTypes = new ArrayList(); //Type?
            ArrayList values = new ArrayList(); //Object?

            // Creating List
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                while (reader.Read())
                {
                    tokenTypes.Add(reader.TokenType);
                    valueTypes.Add(reader.ValueType);
                    values.Add(reader.Value);
                    Console.WriteLine("{0} - {1} - {2}", reader.TokenType, reader.ValueType, reader.Value);
                } 
            }

            // Analyzing List
            for (int i = 0; i < values.Count; i++)
            {

                var value = (string)values[i];
                if (isTypeConvertableToPAObject(value))
                    currentType = value;

                if (!objectStarted && tokenTypes[i].ToString() == "StartObject")
                {
                    objectStarted = true;
                    float[] offsetVect = GetVarFromValue.Offset(currentType);
                    gameObjectsData.Add(new GameObjectData()
                    {
                        ID = GenerateID(999),
                        shape = null,
                        offsetX = offsetVect[0],
                        offsetY = offsetVect[1]
                    });
                    Console.WriteLine("NEW OBJECT!! value:{0}, objstarted: {1}", currentType, objectStarted);

                    cItem += 1;
                    Console.WriteLine("CITEM: " + cItem);
                }


                if (currentType != "" && objectStarted)
                {
                    // Initializing Object Data
                    string nextVal = (string)values[i + 1];
                    if (nextVal != null)
                    {
                        GameObjectData obj = (GameObjectData)gameObjectsData[cItem];
                        float nextValf = 0.0f;
                        int j;

                        // This is to see if the item is any of the following cases to see if the nextVal is a float or not.
                        if (IsItemIsInArray(value, GameObjectData.varLabels, out j)) nextValf = float.Parse(nextVal);

                        // Different Objects
                        AddValToData.General(ref obj, value, nextVal, nextValf);
                        gameObjectsData[cItem] = obj;
                    }

                }

                if (objectStarted && tokenTypes[i].ToString() == "EndObject")
                {
                    var thisObj = (GameObjectData)gameObjectsData[cItem];
                    if (thisObj.shape == null) thisObj.shape = GetVarFromValue.Shape(currentType);
                        
                    float[] offsetVect = GetVarFromValue.Offset(currentType);
                    thisObj.offsetX = offsetVect[0];
                    thisObj.offsetY = offsetVect[1];

                    gameObjectsData[cItem] = thisObj;


                    // We're not resetting current type because of JSON output.
                    objectStarted = false;
                    Console.WriteLine("END OBJECT!! type:{0}, objectstarted:{1}", currentType, objectStarted);
                }
            }
            return gameObjectsData;
        }
        static GameObject[] ConvertDataListToGameObjects(ArrayList dataList)
        {
            GameObject[] gameObjects = new GameObject[dataList.Count];
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (((GameObjectData)dataList[i]).shape != null) // We don't want to add a shape we can't even determine!
                    gameObjects[i] = ((GameObjectData)dataList[i]).ToObj();
            }
            return gameObjects;
        }
        static int GenerateID(int maxNum)
        {
            Random random = new Random();
            return random.Next(maxNum);
        }
        static bool isTypeConvertableToPAObject(string Value)
        {
            return
                Value == "rect" ||
                Value == "circle" ||
                Value == "polygon" || 
                Value == "path" ||
                Value == "ellipse"
            ;
        }
        static bool IsItemIsInArray(string item, string[] array, out int itemNum)
        {
            itemNum = -1;
            // Defualt export string
            for (int i = 0; i < array.Length; i++)
            {
                if (item == array[i])
                {
                    itemNum = i;
                    return true;
                }
            }
            return false;
        }
        static bool IsItemIsInArray(string item, string[] array)
        {
            for (int i = 0; i < array.Length; i++) if (item == array[i]) return true;
            return false;
        }
        public struct GetVarFromValue
        {
            public static Shapes? Shape(string value)
            {
                switch (value)
                {
                    case "rect":
                        return Shapes.Square;
                    case "circle":
                        return Shapes.Circle;
                    case "ellipse":
                        return Shapes.Circle;
                }
                return null;
            }
            public static float[] Offset(string value)
            {
                // Center Pivot
                if (IsItemIsInArray(value, new string[] { "circle", "ellipse", "path" }))
                    return new float[] { 0f, 0f };

                // Top Right Pivot
                else
                    return new float[] { 0.5f, -0.5f };

            }
        }
        struct AddValToData
        {
            public static void General(ref GameObjectData obj, string value, string nextVal, float nextValf)
            {
                Shapes? shape = null;
                if (IsItemIsInArray(value, new string[] { "@d" })) AddVarToData.D(ref obj, nextVal, out shape);
                if (shape != null) obj.shape = shape.Value;

                if (IsItemIsInArray(value, new string[] { "@x", "@cx" })) AddVarToData.X(ref obj, nextValf);
                if (IsItemIsInArray(value, new string[] { "@y", "@cy" })) AddVarToData.Y(ref obj, nextValf);
                if (IsItemIsInArray(value, new string[] { "@width", "@rx" })) AddVarToData.sizeX(ref obj, nextValf);
                if (IsItemIsInArray(value, new string[] { "@height", "@ry" })) AddVarToData.sizeY(ref obj, nextValf);
                if (IsItemIsInArray(value, new string[] { "@r" })) AddVarToData.size(ref obj, nextValf);
                if (IsItemIsInArray(value, new string[] { "@fill", "@stroke" })) AddVarToData.fill(ref obj, nextVal);
                if (IsItemIsInArray(value, new string[] { "@stroke" })) AddVarToData.stroke(ref obj);


            }
        }
        struct AddVarToData
        {
            // Custom Path.
            public static void D(ref GameObjectData obj, string nextVal, out Shapes? shape)
            {
                shape = null;
                // Splitting the values
                string[] rawArray = nextVal.Split('M', 'L', 'Z', ' ', 'z', 'l', 'm');
                ArrayList arraylist = new ArrayList();

                // We dont want to add empty stuff
                for (int i = 0; i < rawArray.Length; i++)
                    if (rawArray[i] != "") arraylist.Add(rawArray[i]);

                foreach (var item in arraylist) {
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
                }
                for (int i = 1; i < floatArray.Length; i += 2)
                {
                    yArrayList.Add(floatArray[i]);
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
                for(int i = 0; i < xArrayList.Count; i++)
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

                // Analyze the number of points. We multiply by two because its split up by float, not by vector / float[2] then add two because of [M x y]
                if (points.Length == 3) shape = Shapes.Triangle;
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
                GetRotatedShape(1f, CustomConversions.Float2ToVect(center), points, out finalPoints, out finalRotation);

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
                obj.sizeX = finalSize[0];
                obj.sizeY = finalSize[1];
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
            public static void sizeX(ref GameObjectData obj, float nextValf)
            {
                obj.sizeX = nextValf;
            }
            public static void sizeY(ref GameObjectData obj, float nextValf)
            {
                obj.sizeY = nextValf;
            }
            public static void size(ref GameObjectData obj, float nextValf)
            {
                obj.sizeY = obj.sizeX = nextValf*2;
            }
            public static void fill(ref GameObjectData obj, string nextVal)
            {
                int id;
                if (IsItemIsInArray(nextVal, Colors.ids, out id))
                {
                    obj.colorNum = id; // Affirmative, colors count from 0.
                }
            }
            public static void stroke(ref GameObjectData obj)
            {
                obj.shapeVariant = 1;
            }
        }
        struct CustomMath
        {
            public static float[] GetCenter(float[] min, float[] max)
            {
                return new float[] { (max[0] + min[0])/2f , (max[1] + min[1])/2f};
            }
            public static float[] GetSize(float[] min, float[] max)
            {
                float[] center = GetCenter(min, max);
                return new float[] { (max[0] - center[0])*2, (max[1] - center[1])*2};
            }
            public struct RotatePtsAroundOrigin {
                public static void AntiClockwise(float rotRad, Vector2[] points, out Vector2[] rotatedPoints)
                {
                    // Note that this is radians.
                    rotatedPoints = new Vector2[points.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        rotatedPoints[i].X = (points[i].X * MathF.Cos(rotRad)) - (points[i].Y * MathF.Sin(rotRad));
                        rotatedPoints[i].Y = (points[i].X * MathF.Sin(rotRad)) + (points[i].Y * MathF.Cos(rotRad));
                    }
                } 
            }
            
        }
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
        }
        public static void GetRotatedShape(float rotDegDiff, Vector2 center, Vector2[] points, out Vector2[] RotatedPointsOut, out float finalRotation)
        {
            // Default Values.
            finalRotation = 0;
            RotatedPointsOut = points;
            //

            var rotRadDiff = rotDegDiff * (MathF.PI / 180); // Converting deg -> rad
            for (int v = 0; v < points.Length; v++) points[v] -= center; // Subtracting the diff so we can rotate around the center.

            for (float r = -(MathF.PI * 2); r < MathF.PI * 2; r += rotRadDiff) // questioning if we should do -360 -> 360 or -360 -> 0
            {
                Vector2[] rotatedPoints;
                CustomMath.RotatePtsAroundOrigin.AntiClockwise(r, points, out rotatedPoints);

                for (int i = 1; i < rotatedPoints.Length; i++)
                {
                    // We want the base line to be on the x axis.
                    var absDist = MathF.Abs(rotatedPoints[i].Y - rotatedPoints[i - 1].Y);
                    // Comparing it to the deadzone.
                    if (absDist < 0.5f && rotatedPoints[i].Y > 0) // We want it to be the lowest line below everything. (closest to width)
                    { 
                        // If this is a triangle
                        if (points.Length == 3)
                        {
                            Vector2 otherpoint = points[0];
                            if (i == 1) otherpoint = points[2];
                            if (i == 2) otherpoint = points[0];
                            if (MathF.Abs(otherpoint.X) > 1)
                            {
                                // We dont want to continue if the top point of the triangle isnt close to the center.
                                // I'll consider a function to analyze the other part of the triangle to see if its a right triangle later.
                                continue;
                            }
                        }

                        // If we got this far we can assume this is the correct rotation and points
                        RotatedPointsOut = rotatedPoints;
                        finalRotation = -r / (MathF.PI / 180);
                    }

                }
            }

            // Questioning if recalculating the center makes results more accurate, if not consider removing.
            float[] xArray = CustomConversions.VectIndexToFloatList(0, RotatedPointsOut);
            float[] yArray = CustomConversions.VectIndexToFloatList(1, RotatedPointsOut);
            float[] finalCenter = CustomMath.GetCenter(
                new float[] { xArray.Min(), yArray.Min() },
                new float[] { xArray.Max(), yArray.Max() }
            );
            //

            for (int v = 0; v < points.Length; v++) points[v] += CustomConversions.Float2ToVect(finalCenter); // Adding back the diff.
        }
    }
}
