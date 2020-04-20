using System;
using System.Collections;
using System.IO;
using PA_PrefabBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Linq;
using System.Numerics;

namespace SVGToPrefab
{
    
    class Program
    {
        static void Main(string[] args)
        {
            // XML -> JSON
            string jStr = XMLToJSON(Input.svgPath);

            // JSON -> GameObjectData[] {Custom Class}
            ArrayList gameObjectDataList = JSONToGameObjectData(jStr);
            foreach (var data in gameObjectDataList) Console.WriteLine(((GameObjectData)data).ToString());

            // GameObjectData[] -> GameObject[]
            GameObject[] objects = ConvertDataListToGameObjects(gameObjectDataList);

            // GameObject[] -> Prefab!
            PrefabBuilder pb = new PrefabBuilder(Input.prefabName, Input.prefabType, 0);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].Bin = i; // This may be a problem if I > 14.
                    pb.Objects.Add(objects[i]);
                }
            }

            // Prefab -> Project Arrythmia!!
            pb.Export(Input.prefabPath);

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
                        DataAppliers.ApplyThisVal(ref obj, value, nextVal, nextValf);
                        gameObjectsData[cItem] = obj;
                    }

                }

                if (objectStarted && tokenTypes[i].ToString() == "EndObject")
                {
                    var thisObj = (GameObjectData)gameObjectsData[cItem];
                    if (thisObj.shape == null)
                    {
                        thisObj.shape = GetVarFromValue.Shape(currentType);
                        float[] offsetVect = GetVarFromValue.Offset(currentType);
                        thisObj.offsetX = offsetVect[0];
                        thisObj.offsetY = offsetVect[1];
                        Console.WriteLine("OFFEST IS BEING CALLED");
                    }
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
        public static bool IsItemIsInArray(string item, string[] array, out int itemNum)
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
        public static bool IsItemIsInArray(string item, string[] array)
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
        
        
    }
}
