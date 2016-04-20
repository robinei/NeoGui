using System;

namespace NeoGui.Core
{
    public struct Transform
    {
        public Quat Rotation;
        public Vec3 Translation;
        public Vec3 Scale;

        public Vec3 ApplyForward(Vec3 v) => Rotation * (Scale * v) + Translation;

        public Vec3 ApplyInverse(Vec3 v) => Scale.Inverse * (Rotation.Conjugate * (v - Translation));

        public void Product(ref Transform a, ref Transform b)
        {
            Rotation = a.Rotation * b.Rotation;
            Scale = a.Scale * b.Scale;
            Translation = a.ApplyForward(b.Translation);
        }

        public void GetAxes(out Vec3 ax, out Vec3 ay, out Vec3 az)
        {
            var m = new Mat4();
		    Rotation.Conjugate.ToMatrix(ref m);
		    ax = new Vec3(m.M00, m.M01, m.M02);
		    ay = new Vec3(m.M10, m.M11, m.M12);
		    az = new Vec3(m.M20, m.M21, m.M22);
        }

        public void MakeIdentity()
        {
            Rotation = Quat.Identity;
            Translation = Vec3.Zero;
            Scale = Vec3.UnitScale;
        }

        public void MakeUnitScale()
        {
            Scale = Vec3.UnitScale;
        }

        public float MinScale => Math.Min(Scale.X, Math.Min(Scale.Y, Scale.Z));
        public float MaxScale => Math.Max(Scale.X, Math.Max(Scale.Y, Scale.Z));

        public void ToMatrix(ref Mat4 m)
        {
            Rotation.ToMatrix(ref m);

            m.M03 = (m.M00 * Scale.X * Translation.X) + (m.M01 * Scale.X * Translation.Y) + (m.M02 * Scale.X * Translation.Z);
            m.M13 = (m.M10 * Scale.Y * Translation.X) + (m.M11 * Scale.Y * Translation.Y) + (m.M12 * Scale.Y * Translation.Z);
            m.M23 = (m.M20 * Scale.Z * Translation.X) + (m.M21 * Scale.Z * Translation.Y) + (m.M22 * Scale.Z * Translation.Z);

            m.M00 *= Scale.X;
            m.M10 *= Scale.Y;
            m.M20 *= Scale.Z;
            
            m.M01 *= Scale.X;
            m.M11 *= Scale.Y;
            m.M21 *= Scale.Z;
            
            m.M02 *= Scale.X;
            m.M12 *= Scale.Y;
            m.M22 *= Scale.Z;
        }
    }
}
