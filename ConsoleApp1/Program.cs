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
            // XML -> GameObjectData[] {Custom Class}
            List<GameObjectData> gameObjectDataList;
            List<PathOutline> pathOutlineList;
            XMLToGameObjectData(Input.svgPath, out gameObjectDataList, out pathOutlineList);
            foreach (var data in gameObjectDataList) LineWriter.WriteLine(data.ToString());
            foreach (var outline in pathOutlineList) if (outline != null) LineWriter.WriteLine(outline.ToString());

            // GameObjectData[] -> GameObject[]
            List<GameObject> gameObjectsList = new List<GameObject>();

            ApplyGameObjectDatasToGameObjectList(gameObjectDataList, ref gameObjectsList);
            ApplyGameObjectDatasToGameObjectList(
                ConvertPathOutlinesToGameObjectDatas(pathOutlineList), ref gameObjectsList);

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

                    float[] offsetVect = GetVarFromShapeName.Offset(item.Name);
                    gameObjectsData.Add(new GameObjectData()
                    {
                        ID = GenerateID(999),
                        shape = GetVarFromShapeName.Shape(item.Name),
                        offsetX = offsetVect[0],
                        offsetY = offsetVect[1]
                    });
                    if (nodes.Item(i).Name == "path") pathOutlinesData.Add(new PathOutline());
                    else pathOutlinesData.Add(null);

                    GameObjectData obj = gameObjectsData[i];
                    PathOutline pO = pathOutlinesData[i];
                    for (int a = 0; a < attributes.Count; a++)
                    {
                        LineWriter.WriteLine(attributes.Item(a).Name + ", " + attributes.Item(a).Value);
                        float valuef = IsItemIsInArray(attributes.Item(a).Name, GameObjectData.varLabels) ? float.Parse(attributes.Item(a).Value) : 0;
                        DataAppliers.ApplyThisAttributeValue(ref obj, ref pO, attributes.Item(a).Name, attributes.Item(a).Value, valuef);
                    }
                    gameObjectsData[i] = obj;
                    pathOutlinesData[i] = pO;

                    if (gameObjectsData[i].shape == null)
                        gameObjectsData[i].shape = GetVarFromShapeName.Shape(item.Name);
                }
                LineWriter.WriteLine("NEW OBJECT!! type:{" + item.Name + "}" + i);
            }
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
        public struct GetVarFromShapeName
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
