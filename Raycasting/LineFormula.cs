using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public class LineFormula 
    {
        public float A { get; set; }

        public float B { get; set; }

        public override string ToString()
        {
            return String.Format("y = {0}x + {1}", A, B);
        }

        public static LineFormula FromCoordinateAndDirection(Vector2 coordinate, Vector2 direction)
        {
            float slope = direction.Y / direction.X;
            return  new Raycasting.LineFormula() { A = slope, B = FindYIntersect(slope, coordinate) };
        }
       
        public static LineFormula FromCoordinateAndDirection(Vector2 coordinate, float angleInRadians)
        {
            return FromCoordinateAndDirection(coordinate, angleInRadians.AngleAsVector());
        }

        public static float FindYIntersect(float slope, Vector2 knownPointOnLine)
        {
            //y = ax+b
            //y - ax = b;
            return knownPointOnLine.Y - slope * knownPointOnLine.X;
        }

        public float? GetInterSectWithVerticalLine(float xValueOfVerticalLine)
        {
            return A * xValueOfVerticalLine + B;
        }

        public float? GetInterSectWithHorizontalLine(float yValueOfHorizontalLine)
        {
            //y = ax + b;
            //y-b = ax;
            //(y-b)/a = x

            return ((yValueOfHorizontalLine - B) / A);
        }


        #region overrides

        public override bool Equals(object obj)
        {
            var otherLine = obj as LineFormula;

            if (otherLine == null) { return false; }
            return A.Equals(otherLine.A) && B.Equals(otherLine.B);
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + A.GetHashCode();
            hash = (hash * 7) + B.GetHashCode();
            return hash;
        }


        #endregion

    }
}
