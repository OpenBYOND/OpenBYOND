using System;
using Microsoft.Xna.Framework;

namespace OpenBYOND
{
        public struct VectorHelpers
        {
            public const float kEpsilon = 1E-05f;
            
            public static Vector3 normalized(Vector3 v)
            {

                    return Normalize(v);
  
            }
            public static double magnitude(Vector3 v)
            {
                
                
                    return Math.Sqrt(v.X * v.X + v.X * v.Y + v.Z * v.Z);
                
            }
            public static float sqrMagnitude(Vector3 v)
            {
                
                
                    return v.X * v.X + v.Y * v.Y + v.Z * v.Z;
                
            }
            public static Vector3 Zero
            {
                get
                {
                    return new Vector3(0f, 0f, 0f);
                }
            }
            public static Vector3 one
            {
                get
                {
                    return new Vector3(1f, 1f, 1f);
                }
            }
            public static Vector3 forward
            {
                get
                {
                    return new Vector3(0f, 0f, 1f);
                }
            }
            public static Vector3 back
            {
                get
                {
                    return new Vector3(0f, 0f, -1f);
                }
            }
            public static Vector3 up
            {
                get
                {
                    return new Vector3(0f, 1f, 0f);
                }
            }
            public static Vector3 down
            {
                get
                {
                    return new Vector3(0f, -1f, 0f);
                }
            }
            public static Vector3 left
            {
                get
                {
                    return new Vector3(-1f, 0f, 0f);
                }
            }
            public static Vector3 right
            {
                get
                {
                    return new Vector3(1f, 0f, 0f);
                }
            }


            public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
            {
                t = MathHelper.Clamp(t,0,1);
                return new Vector3(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t, from.Z + (to.Z - from.Z) * t);
            }

            public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maXDistanceDelta)
            {
                Vector3 a = target - current;
                double mag = Magnitude(a);
                if (mag <= maXDistanceDelta || mag == 0f)
                {
                    return target;
                }
                return current + a / (float)mag * maXDistanceDelta;
            }

            public static Vector3 Scale(Vector3 a, Vector3 b)
            {
                return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
            }

            public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
            {
                return new Vector3(lhs.Y * rhs.Z - lhs.Z * rhs.Y, lhs.Z * rhs.X - lhs.X * rhs.Z, lhs.X * rhs.Y - lhs.Y * rhs.X);
            }
            public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
            {
                return -2f * Vector3.Dot(inNormal, inDirection) * inNormal + inDirection;
            }
            public static Vector3 Normalize(Vector3 value)
            {
                double num = Magnitude(value);
                if (num > 1E-05f)
                {
                    return value / (float)num;
                }
                return Vector3.Zero;
            }



            public static float Dot(Vector3 lhs, Vector3 rhs)
            {
                return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
            }
            public static Vector3 Project(Vector3 vector, Vector3 onNormal)
            {
                float num = Vector3.Dot(onNormal, onNormal);
                if (num < 1.401298E-45f)
                {
                    return Vector3.Zero;
                }
                return onNormal * Vector3.Dot(vector, onNormal) / num;
            }
            public static Vector3 EXclude(Vector3 eXcludeThis, Vector3 fromThat)
            {
                return fromThat - Project(fromThat, eXcludeThis);
            }
            public static double Angle(Vector3 from, Vector3 to)
            {
                return Math.Acos(MathHelper.Clamp(Vector3.Dot(Normalize(from), Normalize(to)), -1f, 1f)) * 57.29578f;
            }
            public static double Distance(Vector3 a, Vector3 b)
            {
                Vector3 vector = new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
                return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            }
            public static Vector3 ClampMagnitude(Vector3 vector, float maXLength)
            {
                if (sqrMagnitude(vector) > maXLength * maXLength)
                {
                    return normalized(vector) * maXLength;
                }
                return vector;
            }
            public static double Magnitude(Vector3 a)
            {
                return Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
            }
            public static float SqrMagnitude(Vector3 a)
            {
                return a.X * a.X + a.Y * a.Y + a.Z * a.Z;
            }
            public static Vector3 Min(Vector3 lhs, Vector3 rhs)
            {
                return new Vector3(Math.Min(lhs.X, rhs.X), Math.Min(lhs.Y, rhs.Y), Math.Min(lhs.Z, rhs.Z));
            }
        }
    

}

