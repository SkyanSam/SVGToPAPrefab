using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;
using PA_PrefabBuilder;

namespace SVGToPrefab.Custom
{
    static class CustomMath
    {
        /* Note: Min is the point with the lowest x and y values out of all points
         * Max is the point with the highest x and y values.
         * Min and Max are really just Vector2/Point not float[] with large array.
         */
        public static float[] GetCenter(float[] min, float[] max)
        {
            return new float[] { (max[0] + min[0]) / 2f, (max[1] + min[1]) / 2f };
        }
        public static float[] GetSize(float[] min, float[] max)
        {
            float[] center = GetCenter(min, max);
            return new float[] { (max[0] - center[0]) * 2, (max[1] - center[1]) * 2 };
        }
        public static Vector2 Normalize(this Vector2 vect)
        {
            return vect / vect.Magnitude();
        }
        public static float Magnitude(this Vector2 vect)
        {
            return MathF.Sqrt((vect.X*vect.X) + (vect.Y*vect.Y));
        }
        public struct Rotations
        {
            // https://stackoverflow.com/questions/17530169/get-angle-between-point-and-origin 
            // https://stackoverflow.com/questions/1211212/how-to-calculate-an-angle-from-three-points

            /// <summary>
            /// Gets the rotation from a center point and a vector representing direction
            /// </summary>
            /// <param name="center">Center Point</param>
            /// <param name="target">Vector Representing Direction</param>
            /// <returns></returns>
            public static float GetRotationFromVector(Vector2 center, Vector2 target)
            {
                return (float)calculateAngle (
                    center.X, center.Y, 
                    target.X, target.Y, 
                    center.X + (target - center).Magnitude(), center.Y + 0
                );
            }
            /// <summary>
            /// Returns in radians.
            /// </summary>
            public static float GetRotationFromThreePts(Vector2 P1, Vector2 P2, Vector2 P3)
            {
                return MathF.Atan2(P3.Y - P1.Y, P3.X - P1.X) -
                MathF.Atan2(P2.Y - P1.Y, P2.X - P1.X);
            }
            public static double calculateAngle(double P1X, double P1Y, double P2X, double P2Y, double P3X, double P3Y)
            {
                double numerator = P2Y * (P1X - P3X) + P1Y * (P3X - P2X) + P3Y * (P2X - P1X);
                double denominator = (P2X - P1X) * (P1X - P3X) + (P2Y - P1Y) * (P1Y - P3Y);
                double ratio = numerator / denominator;

                double angleRad = Math.Atan(ratio);
                double angleDeg = (angleRad * 180) / Math.PI;

                if (angleDeg < 0)
                {
                    angleDeg = 180 + angleDeg;
                }

                return angleDeg;
            }
            ///<summary>
            ///Rotates points anti clockwise around the origin.
            ///</summary>
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

            /// <summary>
            /// This will determine the rotation of your shape if its rotated.
            /// </summary>
            /// <param name="rotDegDiff"> A degree rotation that changes how accurate the rotation is. Default is 1.</param>
            /// <param name="center">The center of the shape.</param>
            /// <param name="points">Point Values</param>
            /// <param name="RotatedPointsOut">Points Output</param>
            /// <param name="finalRotation">Rotation Output</param>
            public static void GetRotatedShape(float rotDegDiff, Vector2 center, Vector2[] points, out Vector2[] RotatedPointsOut, out float finalRotation, out bool isRightTriangle)
            {
                // Default Values.
                finalRotation = 0;
                RotatedPointsOut = points;
                isRightTriangle = false;
                //

                var rotRadDiff = rotDegDiff * (MathF.PI / 180); // ConD:\Documents\CSharpProjects\SVGToPAPrefab\ConsoleApp1\Program.csverting deg -> rad
                for (int v = 0; v < points.Length; v++) points[v] -= center; // Subtracting the diff so we can rotate around the center.

                for (float r = 0; r < (MathF.PI * 2); r += rotRadDiff) // questioning if we should do -360 -> 360 or -360 -> 0
                {
                    Vector2[] rotatedPoints;
                    CustomMath.Rotations.AntiClockwise(r, points, out rotatedPoints);

                    for (int i = 1; i < rotatedPoints.Length; i++)
                    {
                        // We want the base line to be on the x axis.
                        var y_absDist = MathF.Abs(rotatedPoints[i].Y - rotatedPoints[i - 1].Y);
                        
                        // Comparing it to the deadzone.
                        if (y_absDist < 0.1f && rotatedPoints[i].Y > 0) // We want it to be the lowest line below everything. (closest to width)
                        {
                            // If this is a triangle
                            if (points.Length == 3)
                            {
                                Vector2 toppoint = points[0];
                                if (i == 1) toppoint = points[2];
                                if (i == 2) toppoint = points[0];

                                // We want to continue if the top point of the triangle is close to the center, or its a right triangle.

                                #region Isosoles Check
                                if (MathF.Abs(toppoint.X) < 1 / Input.sizeMultiplier)
                                {
                                    continue;
                                }
                                #endregion

                                #region Right Triangle Check

                                // Get the bottom left corner of the triangle
                                Vector2 bottomleftcorner;
                                if (rotatedPoints[i].X < rotatedPoints[i - 1].X) bottomleftcorner = rotatedPoints[i];
                                else bottomleftcorner = rotatedPoints[i - 1];

                                // X distance = bottom left corner.X and otherpoint.X
                                float xdistance1 = MathF.Abs(bottomleftcorner.X - toppoint.X);

                                // If xDist < 1 or something THEN RIGHT TRIANGLE IS A YES!
                                if (xdistance1 < 0.5f / Input.sizeMultiplier)
                                {
                                    isRightTriangle = true;
                                    continue;
                                }
                                #endregion
                            }

                            // If we got this far we can assume this is the correct rotation and points
                            RotatedPointsOut = rotatedPoints;
                            finalRotation = r * (180 / MathF.PI); // Converting rad to deg
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
}
