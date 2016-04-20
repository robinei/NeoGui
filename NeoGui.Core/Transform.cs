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
		    Rotation.Conjugate.ToMatrix(out m);
		    ax = new Vec3(m.M11, m.M12, m.M13);
		    ay = new Vec3(m.M21, m.M22, m.M23);
		    az = new Vec3(m.M31, m.M32, m.M33);
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

        public void ToMatrix(out Mat4 m)
        {
            Rotation.ToMatrix(out m);

            m.M14 = (m.M11 * Scale.X * Translation.X) + (m.M12 * Scale.X * Translation.Y) + (m.M13 * Scale.X * Translation.Z);
            m.M24 = (m.M21 * Scale.Y * Translation.X) + (m.M22 * Scale.Y * Translation.Y) + (m.M23 * Scale.Y * Translation.Z);
            m.M34 = (m.M31 * Scale.Z * Translation.X) + (m.M32 * Scale.Z * Translation.Y) + (m.M33 * Scale.Z * Translation.Z);

            m.M11 *= Scale.X;
            m.M21 *= Scale.Y;
            m.M31 *= Scale.Z;
            
            m.M12 *= Scale.X;
            m.M22 *= Scale.Y;
            m.M32 *= Scale.Z;
            
            m.M13 *= Scale.X;
            m.M23 *= Scale.Y;
            m.M33 *= Scale.Z;
        }
    }
}
