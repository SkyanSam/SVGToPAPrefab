using System;
using System.Collections;
using System.IO;
using PA_PrefabBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

[assembly: InternalsVisibleTo("UIWindows")]
[assembly: InternalsVisibleTo("UIWPF")]

namespace SVGToPrefab
{
    static class Ext
    {
        public static T[] GetArrayType<T>(this ArrayList list)
        {
            T[] array = new T[list.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (T)list[i];
            }
            return array;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {

        }
        public static void Process()
        {

            // XML -> JSON
            string jStr = XMLToJSON(Input.svgPath);

            // JSON -> GameObjectData[] {Custom Class}
            List<GameObjectData> gameObjectDataList;
            List<PathOutline> pathOutlineList;
            JSONToGameObjectData(jStr, out gameObjectDataList, out pathOutlineList);
            foreach (var data in gameObjectDataList) LineWriter.WriteLine(((GameObjectData)data).ToString());
            foreach (var outline in pathOutlineList) if (((PathOutline)outline) != null) LineWriter.WriteLine(((PathOutline)outline).ToString());
            // GameObjectData[] -> GameObject[]
            List<GameObject> gameObjectsList = new List<GameObject>();

            ApplyGameObjectDatasToGameObjectList(gameObjectDataList, ref gameObjectsList);
            ApplyGameObjectDatasToGameObjectList (
                ConvertPathOutlinesToGameObjectDatas(pathOutlineList), ref gameObjectsList );

            GameObject[] objects = gameObjectsList.ToArray();

            // GameObject[] -> Prefab!
            PrefabBuilder pb = new PrefabBuilder(Input.prefabName, Input.prefabType, 0);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].Bin = i; // This may be a problem if I > 14.
                    // (objects[i].renderDistance) need to add render distance.
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
        static void JSONToGameObjectData(string json, out List<GameObjectData> gameObjectsData, out List<PathOutline> pathOutlinesData)
        {
            gameObjectsData = new List<GameObjectData>();
            pathOutlinesData = new List<PathOutline>();
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
                    pathOutlinesData.Add(null);
                    
                    LineWriter.WriteLine("NEW OBJECT!! type:{" + currentType + "}, objectstarted:{ " + objectStarted + "}");

                    cItem += 1;
                    if (currentType == "line") pathOutlinesData[cItem] = new PathOutline();

                    LineWriter.WriteLine("CITEM: " + cItem);
                }


                if (currentType != "" && objectStarted)
                {
                    // Initializing Object Data
                    string nextVal = (string)values[i + 1];
                    if (nextVal != null)
                    {
                        GameObjectData obj = gameObjectsData[cItem];
                        PathOutline pO = pathOutlinesData[cItem];
                        float nextValf = 0.0f;
                        int j;

                        // This is to see if the item is any of the following cases to see if the nextVal is a float or not.
                        if (IsItemIsInArray(value, GameObjectData.varLabels, out j)) nextValf = float.Parse(nextVal) * Input.sizeMultiplier;

                        // Different Objects
                        DataAppliers.ApplyThisVal(ref obj, ref pO, value, nextVal, nextValf);
                        gameObjectsData[cItem] = obj;
                        pathOutlinesData[cItem] = pO;
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
                        LineWriter.WriteLine("OFFEST IS BEING CALLED");
                    }
                    gameObjectsData[cItem] = thisObj;


                    // We're not resetting current type because of JSON output.
                    objectStarted = false;
                    LineWriter.WriteLine("END OBJECT!! type:{" + currentType + "}, objectstarted:{ " + objectStarted + "}");
                }
            }

            // Converting Path Outline to GameObjects.

            /*
            for (int i = 0; i < pathOutlinesData.Count; i++)
            {
                if (pathOutlinesData[i] as PathOutline != null)
                {
                    GameObjectData[] pathobjs = (pathOutlinesData[i] as PathOutline).ToObjs();
                    for (int g = 0; g < pathobjs.Length; i++) gameObjectsData.Add(pathobjs[i]);
                }
            }
            */
        }
        public static void ApplyGameObjectDatasToGameObjectList(List<GameObjectData> GameObjectDataList, ref List<GameObject> GameObjectList)
        {
            for (int i = 0; i < GameObjectDataList.Count; i++)
                if (GameObjectDataList[i].shape != null) // We don't want to add a shape we can't even determine!
                    GameObjectList.Add(GameObjectDataList[i].ToObj());
        }
        public static List<GameObjectData> ConvertPathOutlinesToGameObjectDatas(List<PathOutline> pathOutlines)
        {
            // Clears any null objects
            for (int i = 0; i < pathOutlines.Count; i++)
                if (pathOutlines[i] == null)
                    pathOutlines.RemoveAt(i);

            List<GameObjectData> gameObjectDatas = new List<GameObjectData>();
            for (int i = 0; i < pathOutlines.Count; i++)
            {
                if (pathOutlines[i] != null)
                {
                    GameObjectData[] tempPathOutlineObjs = pathOutlines[i].ToObjs();
                    foreach (GameObjectData data in tempPathOutlineObjs)
                        gameObjectDatas.Add(data);
                }
            }
            return gameObjectDatas;

        }
        public static int GenerateID(int maxNum)
        {
            Random random = new Random();
            return random.Next(maxNum);
        }
        public static bool isTypeConvertableToPAObject(string Value)
        {
            return
                Value == "rect" ||
                Value == "circle" ||
                Value == "polygon" || 
                Value == "path" ||
                Value == "ellipse" ||
                Value == "line"
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
