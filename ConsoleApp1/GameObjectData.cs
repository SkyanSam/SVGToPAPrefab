using System;
using System.Collections.Generic;
using System.Text;
using PA_PrefabBuilder;

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
            obj.SetPosition(Input.sizeMultiplier * positionX, Input.sizeMultiplier * -positionY);
            obj.SetScale(Input.sizeMultiplier * sizeX, Input.sizeMultiplier * sizeY);
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
        public static string[] varLabels = new string[] { "@x", "@y", "@width", "@height", "@cx", "@cy", "@r", "@rx", "@ry" };
    }
}
