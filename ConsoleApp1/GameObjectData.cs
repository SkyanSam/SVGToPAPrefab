using System;
using System.Collections.Generic;
using System.Text;
using PA_PrefabBuilder;
using System.Numerics;
using SVGToPrefab.Custom;

namespace SVGToPrefab
{
    class GameObjectData : GameObject
    {
        /* If the case is lowercase then it is a GameObjectData function/variable
         * Else it is a GameObject function/variable */
        public bool isShapeUnknown = true; // Use this instead of setting shape to arrow
        public bool isAreaFilled = true;
        public bool isPerimeterOutlined = true;
        public bool isRightTriangle = false;
        private Vector2 _position;
        public Vector2 position {
            get { return _position; }
            set {
                _position = value;
                SetPosition(value.X, -value.Y); 
            }
        }
        public float positionX {
            get { return position.X; }
            set { position = new Vector2(value, position.Y); }
        }
        public float positionY {
            get { return position.Y; }
            set { position = new Vector2(position.X, value); }
        }
        private Vector2 _size;
        public Vector2 size {
            get { return _size; }
            set {
                _size = value;
                SetScale(size.X, size.Y);
            }
        }
        public float sizeX {
            get { return size.X; }
            set { size = new Vector2(value, size.Y); }
        }
        public float sizeY {
            get { return size.Y; }
            set { size = new Vector2(size.X, value); }
        }
        public Vector2 offset {
            get { return new Vector2(OffsetX, OffsetY); }
            set {
                OffsetX = offset.X;
                OffsetY = offset.Y;
            }
        }
        private float _rotAngle;
        public float rotAngle {
            get { return _rotAngle; }
            set {
                _rotAngle = value;
                SetRotation(value); 
            }
        }
        private int _colorNum;
        public int colorNum {
            get { return _colorNum; }
            set {
                _colorNum = value;
                SetColor(value); 
            }
        }
        // ShapeVariant (fill is 0, stroke is 1) implemented in GameObject class
        // Shape implemented in GameObject class
        public GameObjectData() : base("","",Shapes.Square) {
            ID = Program.GenerateID(1000).ToString();
            Name = "Obj " + ID + " " + Shape;
            AddEvent(EventType.col, Input.secondsToLast, colorNum, null, Easing.Linear);
        }
        public void Conclude()
        {
            Name = "Obj " + ID + " " + Shape;
        }
        public override string ToString()
        {
            return (
                base.ToString() + "{{" +
                "id: " + ID + ", " +
                "posX: " + position.X + ", " +
                "posY: " + position.Y + ", " +
                "sizeX: " + size.X + ", " +
                "sizeY: " + size.Y + ", " +
                "offsetX: " + offset.X + ", " +
                "offsetY: " + offset.Y + ", " +
                "ang: " + rotAngle + ", " +
                "color: " + colorNum + ", " +
                "shape: " + Shape.ToString() + ", " +
                "variant: " + ShapeVariant +
                "isShapeUnknown: " + isShapeUnknown + ", " +
                "isAreaFilled: " + isAreaFilled + ", " +
                "isPerimeterOutlined: " + isPerimeterOutlined + ", " +
                "isRightTriangle: " + isRightTriangle + ", "
                + "}}"
            );
        }
        public static bool IsFloatAttribute(string attribute)
        {
            switch(attribute)
            {
                case "x": case "y": case "width": case "height": case "cx": case "cy": case "r": case "rx": case "ry": case "x1": case "x2": case "y1": case "y2": return true;
                default: return false;
            }
        }
    }
    /// <summary>
    /// This is used to represent the perimeters of multishapes/polygons, such as the <path></path> name.
    /// </summary>
    class PathOutline
    {
        public Vector2[] points = new Vector2[2] { new Vector2(), new Vector2() };
        public int colorNum = 0;
        public float outlineSize { get; set; } = 1;
        public void AddToList(ref List<GameObjectData> list)
        {
            LineWriter.WriteLine("START PATH OUTLINE {{{{");
            GameObjectData obj;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 nextpoint = i + 1 >= points.Length ? points[0] : points[i + 1];
                Vector2 target = nextpoint - points[i];
                target.Y *= -1;
                obj = new GameObjectData();
                obj.ID = Program.GenerateID(1000).ToString();
                obj.colorNum = this.colorNum;
                obj.position = new Vector2 (
                    points[i].X + (target.Normalize().X * outlineSize * Input.sizeMultiplier),
                    points[i].Y + (target.Normalize().Y * outlineSize * Input.sizeMultiplier)
                );
                obj.size = new Vector2 (
                    target.Magnitude(), // Length of this part of the path
                    outlineSize * Input.sizeMultiplier // Size of outline
                );
                obj.Shape = Shapes.Square;
                obj.rotAngle = Custom.CustomMath.Rotations.GetRotationFromVector (
                    new Vector2(points[i].X, -points[i].Y),
                    new Vector2(nextpoint.X, -nextpoint.Y)
                );
                obj.offset = new Vector2(0.5f, 0);
                list.Add(obj);
                LineWriter.WriteLine(obj.ToString());
            };
            LineWriter.WriteLine("}}}}");
        }
    }
    /// <summary>
    /// This is used to represent the areas of multishapes/polygons, such as the <path></path> name.
    /// </summary>
    class PathArea {
    }
}
