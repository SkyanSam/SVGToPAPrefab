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
        // External Objects are used when an object is made up of multiple gameObjectData's that dont fit with PathOutline or PathArea.
        // Example: Matrixes
        static List<GameObjectData> externalGameObjectDatas;
        public static void AddExternalGameObjectData(GameObjectData data) {
            externalGameObjectDatas.Add(data); }
        //

        static void Main(string[] args)
        {

        }
        
        public static void Process()
        {
            // XML -> GameObjectData[] {Custom Class}
            List<GameObjectData> gameObjectDataList;
            List<PathOutline> pathOutlineList;
            XMLToGameObjectData(Input.svgPath, out gameObjectDataList, out pathOutlineList);

            foreach (var data in gameObjectDataList) LineWriter.WriteLine(data.ToString());
            foreach (var pathOutline in pathOutlineList) if (pathOutline != null) pathOutline.AddToList(ref gameObjectDataList);
            
            for (int i = 0; i < gameObjectDataList.Count; i++) {
                if (gameObjectDataList[i].isShapeUnknown) {
                    gameObjectDataList.RemoveAt(i);
                    i--;
            }}
            foreach (var data in externalGameObjectDatas) gameObjectDataList.Add(data); // External GameObjects
            // GameObjectData[] -> GameObject[]
            GameObject[] objects = gameObjectDataList.ToArray<GameObject>();

            // GameObject[] -> Prefab!
            PrefabBuilder pb = new PrefabBuilder(Input.prefabName, Input.prefabType, 0);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].Bin = i;
                    objects[i].Depth = 30 - i > -1 ? 30 - i : 0;
                    pb.Objects.Add(objects[i]);
                }
            }
            // Prefab -> Project Arrythmia!!
            pb.Export(Input.prefabPath);

        }
        static void XMLToGameObjectData(string _path, out List<GameObjectData> gameObjectsData, out List<PathOutline> pathOutlinesData)
        {
            gameObjectsData = new List<GameObjectData>();
            pathOutlinesData = new List<PathOutline>();

            string xml = File.ReadAllText(_path);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var nodes = doc.LastChild.ChildNodes;

            for (int i = 0; i < nodes.Count; i++)
                if (nodes.Item(i).Name == "g") nodes = nodes.Item(i).ChildNodes;

            for (int i = 0; i < nodes.Count; i++)
            {
                var item = nodes.Item(i);
                if (isTypeConvertableToPAObject(item.Name))
                {
                    var attributes = nodes.Item(i).Attributes;
                    bool isShapeNull;
                    gameObjectsData.Add(new GameObjectData()
                    {
                        ID = GenerateID(999).ToString(),
                        Shape = GetVarFromShapeName.Shape(item.Name, out isShapeNull),
                        offset = GetVarFromShapeName.Offset(item.Name),
                        isShapeUnknown = isShapeNull
                    });
                    if (nodes.Item(i).Name == "path") pathOutlinesData.Add(new PathOutline());
                    else pathOutlinesData.Add(null);

                    GameObjectData obj = gameObjectsData[gameObjectsData.Count - 1];
                    PathOutline pO = pathOutlinesData[pathOutlinesData.Count - 1];
                    for (int a = 0; a < attributes.Count; a++)
                    {
                        LineWriter.WriteLine(attributes.Item(a).Name + ", " + attributes.Item(a).Value);
                        float valuef = GameObjectData.IsFloatAttribute(attributes.Item(a).Value) ? float.Parse(attributes.Item(a).Value) : 0;
                        DataAppliers.ApplyThisAttributeValue(ref obj, ref pO, attributes.Item(a).Name, attributes.Item(a).Value, valuef);
                    }
                    gameObjectsData[i] = obj;
                    pathOutlinesData[i] = pO;
                }
                LineWriter.WriteLine("NEW OBJECT!! type:{" + item.Name + "}" + i);
            }
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
        public struct GetVarFromShapeName
        {
            public static Shapes Shape(string value, out bool isNull)
            {
                isNull = false;
                switch (value) {
                    case "rect": return Shapes.Square;
                    case "circle": case "ellipse": return Shapes.Circle;
                    default: isNull = true; return Shapes.Arrow;
                }
            }
            public static Vector2 Offset(string value)
            {
                switch (value) {
                    case "circle":
                    case "ellipse":
                    case "path":
                        return Vector2.Zero; // Center Pivot
                    default:
                        return new Vector2(0.5f, -0.5f); // Top Right Pivot
                }
            }
        }
        
        
    }
}
