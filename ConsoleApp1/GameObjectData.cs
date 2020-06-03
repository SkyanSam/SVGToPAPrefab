﻿using System;
using System.Collections.Generic;
using System.Text;
using PA_PrefabBuilder;
using System.Numerics;
using SVGToPrefab.Custom;

namespace SVGToPrefab
{
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
            deepObj.AddEvent(EventType.col, Input.secondsToLast, colorNum, null, Easing.Linear);
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
        public static string[] varLabels = new string[] { "@x", "@y", "@width", "@height", "@cx", "@cy", "@r", "@rx", "@ry", "@x1", "@x2", "@y1", "@y2" };
    }
    class PathOutline
    {
        public Vector2[] points = new Vector2[2] { new Vector2(), new Vector2() };
        public int colorNum = 0;
        public float outlineSize = 1;
        public GameObjectData[] ToObjs()
        {
            GameObjectData[] objs = new GameObjectData[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 nextpoint;
                if (i + 1 >= points.Length) nextpoint = points[0];
                else nextpoint = points[i + 1];
                Vector2 target = nextpoint - points[i];
                target.Y *= -1;

                objs[i] = new GameObjectData() {
                    ID = Program.GenerateID(1000),
                    colorNum = this.colorNum,
                    positionX = points[i].X,
                    positionY = points[i].Y,
                    shape = Shapes.Square,
                    sizeY = outlineSize * Input.sizeMultiplier, // Size of outline
                    sizeX = target.Magnitude(), // Length of this part of the path

                    rotAngle = Custom.CustomMath.Rotations.GetRotationFromVector(
                        new Vector2(points[i].X, -points[i].Y),
                        new Vector2(nextpoint.X, -nextpoint.Y)
                    ),
                    offsetX = 0.5f,
                    offsetY = 0,
                };
            }
            return objs;
        }
        public override string ToString()
        {
            string returnTxt = base.ToString() + " { \n";
            GameObjectData[] objs = ToObjs();
            foreach (GameObjectData data in objs)
                returnTxt += data.ToString() + "\n";
            returnTxt += "\n } ";
            System.Diagnostics.Debug.WriteLine(returnTxt);
            return returnTxt;
        }
    }
}
