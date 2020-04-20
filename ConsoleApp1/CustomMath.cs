using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;

namespace SVGToPrefab.Custom
{
    struct CustomMath
    {
        public static float[] GetCenter(float[] min, float[] max)
        {
            return new float[] { (max[0] + min[0]) / 2f, (max[1] + min[1]) / 2f };
        }
        public static float[] GetSize(float[] min, float[] max)
        {
            float[] center = GetCenter(min, max);
            return new float[] { (max[0] - center[0]) * 2, (max[1] - center[1]) * 2 };
        }
        public struct Rotations
        {
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
            public static void GetRotatedShape(float rotDegDiff, Vector2 center, Vector2[] points, out Vector2[] RotatedPointsOut, out float finalRotation)
            {
                // Default Values.
                finalRotation = 0;
                RotatedPointsOut = points;
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
                        var absDist = MathF.Abs(rotatedPoints[i].Y - rotatedPoints[i - 1].Y);
                        // Comparing it to the deadzone.
                        if (absDist < 0.1f && rotatedPoints[i].Y > 0) // We want it to be the lowest line below everything. (closest to width)
                        {
                            // If this is a triangle
                            if (points.Length == 3)
                            {
                                Vector2 otherpoint = points[0];
                                if (i == 1) otherpoint = points[2];
                                if (i == 2) otherpoint = points[0];
                                if (MathF.Abs(otherpoint.X) < 1)
                                {
                                    // We dont want to continue if the top point of the triangle isnt close to the center.
                                    // I'll consider a function to analyze the other part of the triangle to see if its a right triangle later.
                                    continue;
                                }
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
