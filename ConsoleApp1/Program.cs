using System;
using System.Collections;
using System.IO;
using PA_PrefabBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Linq;

namespace ConsoleApp1
{
    public static class UserOptions // Just modify these variables
    {
        public static string prefabName = "Prefab Generated From SVG";
        public static PrefabType prefabType = PrefabType.Misc1;
        public static float secondsToLast = 15f;
        public static string svgPath = @"D:/Documents/CSharpProjects/AbandonedBoss.svg";
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
                string[] rawArray = nextVal.Split('M', 'L', 'Z', ' ');
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

                // Analyze the number of points. We multiply by two because its split up by float, not by vector / float[2] then add two because of [M x y]
                if (floatArray.Length == (3 * 2)+2) shape = Shapes.Triangle;
                else if (floatArray.Length == (6 * 2)+2) shape = Shapes.Hexagon;

                Console.WriteLine(floatArray.Length);
                if (floatArray.Length == 3 * 2) Console.WriteLine("ITS A FUCKING TRIANGLE");
                // If nothing else we can assume the shape is null.

                // Create float array for x and y.
                float[] xArray = new float[floatArray.Length / 2];
                float[] yArray = new float[floatArray.Length / 2];
                for (int i = 0; i < floatArray.Length; i += 2)
                {
                    xArray[i / 2] = floatArray[i];
                }
                for (int[] i = new int[] { 1, 0 }; i[0] < floatArray.Length; i[0] += 2)
                {
                    // i[0] is index of floatArray
                    // i[1] is index of yArray
                    yArray[i[1]] = floatArray[i[0]];
                    i[1] += 1;
                }

                // Get min and max
                float[] min = new float[] { xArray.Min(), yArray.Min() };
                float[] max = new float[] { xArray.Max(), yArray.Max() };

                Console.WriteLine("minmax");
                Console.WriteLine(min[0] + "," + min[1]);
                Console.WriteLine(max[0] + "," + max[1]);
                Console.WriteLine("minmax");
                // Get center and size
                float[] center = CustomMath.GetCenter(min, max);
                float[] size = CustomMath.GetSize(min, max);
                Console.WriteLine(center[0]);
                Console.WriteLine(center[1]);
                Console.WriteLine(size[0]);
                Console.WriteLine(size[1]);

                // Apply center and size
                obj.positionX = center[0];
                obj.positionY = center[1];
                obj.sizeX = size[0];
                obj.sizeY = size[1];
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
        }
    }
}
