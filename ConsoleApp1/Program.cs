using System;
using System.Collections;
using System.IO;
using PA_PrefabBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace ConsoleApp1
{
    public static class UserOptions // Just modify these variables
    {
        public static string prefabName = "Prefab Generated From SVG";
        public static PrefabType prefabType = PrefabType.Misc1;
        public static float secondsToLast = 15f;
        public static string svgPath = @"D:/Documents/CSharpProjects/AbandonedBossStretch.svg";
        public static string prefabPath = @"C:\Program Files (x86)\Steam\steamapps\common\Project Arrhythmia\beatmaps\prefabs";
    }
    class Colors {
        /* Colors corresponding with their ids. this must be auto filled by the user.
         * NOTE: Need to be careful. Not sure if svg also used hex or hsv. May need to convert later */
        public static string[] ids = new string[10]
        {
        "rgb(0,0,0)",
        "rgb(0,0,0)",
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
        public Shapes shape;
        public float positionX;
        public float positionY;
        public float sizeX;
        public float sizeY;
        public float offsetX;
        public float offsetY;
        public float rotAngle;
        public int colorNum;
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
            GameObject obj = new GameObject(ID.ToString(),"GameObject" + ID, shape);
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
            
            obj.AddEvent(EventType.col, UserOptions.secondsToLast, colorNum, null, Easing.Linear);
            return obj.Clone();
        }
        public override string ToString()
        {
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
                "shape: " + shape.ToString();

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
                objects[i].Bin = i; // This may be a problem if I > 14.
                pb.Objects.Add(objects[i]);
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
            bool reachedClipPath = false;

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

                if (!reachedClipPath && value == "@clip-path")
                    reachedClipPath = true;
                if (reachedClipPath)
                {
                    if (isTypeConvertableToPAObject(value))
                        currentType = value;

                    if (!objectStarted && tokenTypes[i].ToString() == "StartObject")
                    {
                        objectStarted = true;
                        float[] offsetVect = GetVarFromValue.Offset(currentType);
                        gameObjectsData.Add(new GameObjectData()
                        {
                            ID = GenerateID(999),
                            shape = GetVarFromValue.Shape(currentType),
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
                            if (currentType == "rect")
                            {
                                AddValToData.Square(ref obj, value, nextVal, nextValf);
                            }
                            else if (currentType == "circle")
                            {
                                AddValToData.Circle(ref obj, value, nextVal, nextValf);
                            }
                            else if (currentType == "ellipse")
                            {
                                AddValToData.Ellipse(ref obj, value, nextVal, nextValf);
                            }
                            gameObjectsData[cItem] = obj;
                        }

                    }

                    if (objectStarted && tokenTypes[i].ToString() == "EndObject")
                    {
                        // We're not resetting current type because of JSON output.
                        objectStarted = false;
                        Console.WriteLine("END OBJECT!! type:{0}, objectstarted:{1}", currentType, objectStarted);
                    }
                }
            }
            return gameObjectsData;
        }
        static GameObject[] ConvertDataListToGameObjects(ArrayList dataList)
        {
            GameObject[] gameObjects = new GameObject[dataList.Count];
            for (int i = 0; i < gameObjects.Length; i++)
            {
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
            public static Shapes Shape(string value)
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
                return Shapes.Hexagon; // idk what to return by default lol
            }
            public static float[] Offset(string value)
            {
                // Center Pivot
                if (IsItemIsInArray(value, new string[] { "circle", "ellipse" }))
                    return new float[] { 0f, 0f };

                // Top Right Pivot
                else
                    return new float[] { -0.5f, -0.5f };

            }
        }
        struct AddValToData
        {
            public static void Square(ref GameObjectData obj, string value, string nextVal, float nextValf)
            {
                switch (value)
                {
                    case "@x":
                        obj.positionX = nextValf;
                        break;

                    case "@y":
                        obj.positionY = nextValf;
                        break;

                    case "@width":
                        obj.sizeX = nextValf;
                        break;

                    case "@height":
                        obj.sizeY = nextValf;
                        break;

                    case "@fill":
                        int id;
                        if (IsItemIsInArray(nextVal, Colors.ids, out id))
                        {
                            obj.colorNum = id; // Affirmative, colors count from 0.
                        }
                        break;

                    case "@stroke":
                        // Will implement this in the future, may be same as fill
                        break;
                }
            }
            public static void Circle(ref GameObjectData obj, string value, string nextVal, float nextValf)
            {
                switch (value)
                {
                    case "@cx":
                        obj.positionX = nextValf;
                        break;

                    case "@cy":
                        obj.positionY = nextValf;
                        break;

                    case "@r":
                        obj.sizeX = obj.sizeY = nextValf;
                        break;

                    case "@fill":
                        int id;
                        if (IsItemIsInArray(nextVal, Colors.ids, out id))
                        {
                            obj.colorNum = id; // Affirmative, colors count from 0.
                        }
                        break;

                    case "@stroke":
                        // Will implement this in the future, may be same as fill
                        break;
                }
            }
            public static void Ellipse(ref GameObjectData obj, string value, string nextVal, float nextValf)
            {
                switch (value)
                {
                    case "@cx":
                        obj.positionX = nextValf;
                        break;

                    case "@cy":
                        obj.positionY = nextValf;
                        break;

                    case "@rx":
                        obj.sizeX = nextValf;
                        break;

                    case "@ry":
                        obj.sizeY = nextValf;
                        break;

                    case "@fill":
                        int id;
                        if (IsItemIsInArray(nextVal, Colors.ids, out id))
                        {
                            obj.colorNum = id; // Affirmative, colors count from 0.
                        }
                        break;

                    case "@stroke":
                        // Will implement this in the future, may be same as fill
                        break;
                }
            }
        }
    }
}
