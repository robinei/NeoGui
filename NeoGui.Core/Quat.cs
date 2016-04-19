using System;
using System.Diagnostics;

namespace NeoGui.Core
{
    public struct Quat
    {
        public float W, X, Y, Z;

        public Quat(float w, float x, float y, float z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        public Quat(float w, Vec3 v)
        {
            W = w;
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public float this[int i]
        {
            get
            {
                Debug.Assert(i >= 0 && i < 4);
                return i == 0 ? W : (i == 1 ? X : (i == 2 ? Y : Z));
            }
            set
            {
                Debug.Assert(i >= 0 && i < 4);
                if (i == 0) {
                    W = value;
                } else if (i == 1) {
                    X = value;
                } else if (i == 2) {
                    Y = value;
                } else {
                    Z = value;
                }
            }
        }
        
        public float Length => (float)Math.Sqrt(SqrLength);
        public float SqrLength => W * W + X * X + Y * Y + Z * Z;
        public Quat Normalized => this * (1.0f / Length);
        public Quat Conjugate => new Quat(W, -X, -Y, -Z);
        public float Dot(Quat q) => W * q.W + X * q.X + Y * q.Y + Z * q.Z;
        
        public static Quat operator +(Quat a, Quat b) => new Quat(a.W + b.W, a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Quat operator -(Quat a, Quat b) => new Quat(a.W - b.W, a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Quat operator *(Quat a, float f) => new Quat(a.W * f, a.X * f, a.Y * f, a.Z * f);
        
        public static Quat operator *(Quat a, Quat b) => new Quat(a.W*b.W - a.X*b.X - a.Y*b.Y - a.Z*b.Z,
						                                          a.W*b.X + a.X*b.W + a.Y*b.Z - a.Z*b.Y,
						                                          a.W*b.Y + a.Y*b.W + a.Z*b.X - a.X*b.Z,
						                                          a.W*b.Z + a.Z*b.W + a.X*b.Y - a.Y*b.X);

        public static Vec3 operator *(Quat q, Vec3 v)
        {
            var qvec = new Vec3(q.X, q.Y, q.Z);
            var uv = qvec.Cross(v);
            var uuv = qvec.Cross(uv);
            uv *= 2.0f * q.W;
            uuv *= 2.0f;
            return v + uv + uuv;
        }

        public static Quat Slerp(Quat v0, Quat v1, float t) {
            // v0 and v1 should be unit length or else
            // something broken will happen.

            // Compute the cosine of the angle between the two vectors.
            var d = v0.Dot(v1);

            if(d > 0.9995f) {
                // If the inputs are too close for comfort, linearly interpolate
                // and normalize the result.
                return (v0 + (v1 - v0) * t).Normalized;
            }

            d = Util.Clamp(d, -1.0f, 1.0f);     // Robustness: Stay within domain of acos()
            var theta0 = (float)Math.Acos(d);   // theta_0 = angle between input vectors
            var theta = theta0*t;               // theta = angle between v0 and result 

            var v2 = (v1 - v0*d).Normalized;
    
            // { v0, v2 } is now an orthonormal basis
            return v0 * (float)Math.Cos(theta) + v2 * (float)Math.Sin(theta);
        }
        
        public static Quat FromEulerAngles(float x, float y, float z)
        {
            var qx = new Quat((float)Math.Cos(x/2), (float)Math.Sin(x/2), 0, 0);
            var qy = new Quat((float)Math.Cos(y/2), 0, (float)Math.Sin(y/2), 0);
            var qz = new Quat((float)Math.Cos(z/2), 0, 0, (float)Math.Sin(z/2));
            return qx * qy * qz;
        }

        public static Quat FromEulerAngles(Vec3 v) => FromEulerAngles(v.X, v.Y, v.Z);

        public static Quat FromAxisAngle(float x, float y, float z, float angle)
        {
            angle *= 0.5f;
            var temp = (float)Math.Sin(angle);
            return new Quat((float)Math.Cos(angle), x * temp, y * temp, z * temp);
        }

        public static Quat FromAxisAngle(Vec3 axis, float angle) => FromAxisAngle(axis.X, axis.Y, axis.Z, angle);

        public static Quat FromArc(Vec3 from, Vec3 to)
        {
            var c = from.Cross(to);
            var d = from.Dot(to);
            return new Quat(d + (float)Math.Sqrt(from.SqrLength * to.SqrLength), c).Normalized;
        }

        public void ToMatrix(float[] m)
        {
            var x2  = 2.0f*X;
            var y2  = 2.0f*Y;
            var z2  = 2.0f*Z;
            var x2W = x2*W;
            var y2W = y2*W;
            var z2W = z2*W;
            var x2X = x2*X;
            var y2X = y2*X;
            var z2X = z2*X;
            var y2Y = y2*Y;
            var z2Y = z2*Y;
            var z2Z = z2*Y;

            m[0] = 1.0f-y2Y-z2Z;
            m[1] = y2X+z2W;
            m[2] = z2X-y2W;
		    m[3] = 0.0f;

            m[4] = y2X-z2W;
            m[5] = 1.0f-x2X-z2Z;
            m[6] = z2Y+x2W;
		    m[7] = 0.0f;

            m[8] = z2X+y2W;
            m[9] = z2Y-x2W;
            m[10] = 1.0f-x2X-y2Y;
		    m[11] = 0.0f;

		    m[12] = 0.0f;
		    m[13] = 0.0f;
		    m[14] = 0.0f;
		    m[15] = 1.0f;
        }

        public static readonly Quat Identity = new Quat(1, 0, 0, 0);

        public override string ToString() => $"Quat({W}, {X}, {Y}, {Z})";
    }
}
