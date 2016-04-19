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
            var m = new float[16];
		    Rotation.Conjugate.ToMatrix(m);
		    ax = new Vec3(m[0], m[4], m[8]);
		    ay = new Vec3(m[1], m[5], m[9]);
		    az = new Vec3(m[2], m[6], m[10]);
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
    }
}
