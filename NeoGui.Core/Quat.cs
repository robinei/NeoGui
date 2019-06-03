using System;
using System.Diagnostics;

namespace NeoGui.Core
{
    public struct Quat
    {
        public float X, Y, Z, W;

        public Vec3 XYZ => new Vec3(X, Y, Z);

        public Quat(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Quat(Vec3 v, float w)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = w;
        }

        public float this[int i]
        {
            get
            {
                Debug.Assert(i >= 0 && i < 4);
                return i == 0 ? X : (i == 1 ? Y : (i == 2 ? Z : W));
            }
            set
            {
                Debug.Assert(i >= 0 && i < 4);
                if (i == 0) {
                    X = value;
                } else if (i == 1) {
                    Y = value;
                } else if (i == 2) {
                    Z = value;
                } else {
                    W = value;
                }
            }
        }
        
        public float Length => (float)Math.Sqrt(SqrLength);
        public float SqrLength => X * X + Y * Y + Z * Z + W * W;
        public Quat Normalized => this * (1f / Length);
        public Quat Conjugate => new Quat(-X, -Y, -Z, W);
        public float Dot(Quat q) => X * q.X + Y * q.Y + Z * q.Z + W * q.W;
        
        public static Quat operator +(Quat a, Quat b) => new Quat(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static Quat operator -(Quat a, Quat b) => new Quat(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static Quat operator *(Quat q, float f) => new Quat(q.X * f, q.Y * f, q.Z * f, q.W * f);
        
        public static Quat operator *(Quat a, Quat b) => new Quat(a.W*b.X + a.X*b.W + a.Y*b.Z - a.Z*b.Y,
                                                                  a.W*b.Y + a.Y*b.W + a.Z*b.X - a.X*b.Z,
                                                                  a.W*b.Z + a.Z*b.W + a.X*b.Y - a.Y*b.X,
                                                                  a.W*b.W - a.X*b.X - a.Y*b.Y - a.Z*b.Z);

        public static Vec3 operator *(Quat q, Vec3 v)
        {
            var qvec = q.XYZ;
            var t = 2f * qvec.Cross(v);
            return v + q.W * t + qvec.Cross(t);
        }

        public static Quat Slerp(Quat q0, Quat q1, float t) {
            // q0 and q1 should be unit length or else
            // something broken will happen.

            // Compute the cosine of the angle between the two vectors.
            var d = q0.Dot(q1);

            if(d > 0.9995f) {
                // If the inputs are too close for comfort, linearly interpolate
                // and normalize the result.
                return (q0 + (q1 - q0) * t).Normalized;
            }

            d = Util.Clamp(d, -1f, 1f);     // Robustness: Stay within domain of acos()
            var theta0 = (float)Math.Acos(d);   // theta_0 = angle between input vectors
            var theta = theta0*t;               // theta = angle between q0 and result 

            var q2 = (q1 - q0 * d).Normalized;
    
            // { q0, q2 } is now an orthonormal basis
            return q0 * (float)Math.Cos(theta) + q2 * (float)Math.Sin(theta);
        }
        
        public static Quat FromEulerAngles(float x, float y, float z)
        {
            x *= 0.5f;
            y *= 0.5f;
            z *= 0.5f;
            var qx = new Quat((float)Math.Sin(x), 0, 0, (float)Math.Cos(x));
            var qy = new Quat(0, (float)Math.Sin(y), 0, (float)Math.Cos(y));
            var qz = new Quat(0, 0, (float)Math.Sin(z), (float)Math.Cos(z));
            return qx * qy * qz;
        }

        public static Quat FromEulerAngles(Vec3 v) => FromEulerAngles(v.X, v.Y, v.Z);

        public static Quat FromAxisAngle(float x, float y, float z, float angle)
        {
            angle *= 0.5f;
            var sinAngle = (float)Math.Sin(angle);
            return new Quat(x * sinAngle, y * sinAngle, z * sinAngle, (float)Math.Cos(angle));
        }

        public static Quat FromAxisAngle(Vec3 axis, float angle) => FromAxisAngle(axis.X, axis.Y, axis.Z, angle);

        public static Quat FromArc(Vec3 from, Vec3 to) =>
            new Quat(from.Cross(to), from.Dot(to) + (float)Math.Sqrt(from.SqrLength * to.SqrLength)).Normalized;

        public void ToMatrix(out Mat4 m)
        {
            var x2  = 2f * X;
            var y2  = 2f * Y;
            var z2  = 2f * Z;
            var xw2 = x2 * W;
            var yw2 = y2 * W;
            var zw2 = z2 * W;
            var xx2 = x2 * X;
            var xy2 = y2 * X;
            var xz2 = z2 * X;
            var yy2 = y2 * Y;
            var yz2 = z2 * Y;
            var zz2 = z2 * Z;

            m.M11 = 1f - yy2 - zz2;
            m.M12 = xy2 - zw2;
            m.M13 = xz2 + yw2;
            m.M14 = 0f;

            m.M21 = xy2 + zw2;
            m.M22 = 1f - xx2 - zz2;
            m.M23 = yz2 - xw2;
            m.M24 = 0f;

            m.M31 = xz2 - yw2;
            m.M32 = yz2 + xw2;
            m.M33 = 1f - xx2 - yy2;
            m.M34 = 0f;

            m.M41 = 0f;
            m.M42 = 0f;
            m.M43 = 0f;
            m.M44 = 1f;
        }

        public static Quat Identity => new Quat(0, 0, 0, 1);

        public override string ToString() => $"Quat({X}, {Y}, {Z}, {W})";
    }
}
