using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using BulletSharp;
using BulletSharp.Math;

namespace TGC.Group.Model.GameObjects
{
    class PlataformaGiratoriaBullet
    {
        private float Radio = 0f;
        private float Period;
        private float Moment = 0f;
        private ITransformObject Mesh;
        private TGCVector3 Pos;
        public RigidBody Body;
        public Vector3 Delta;
        public PlataformaGiratoriaBullet(float radio, ITransformObject mesh, RigidBody body, TGCVector3 pos, float period)
        {
            Mesh = mesh;
            Radio = radio;
            Pos = pos;
            Period = period;
            Body = body;
            Body.ActivationState = ActivationState.DisableDeactivation;
            Body.Flags = RigidBodyFlags.DisableWorldGravity;
            Body.Gravity = new Vector3(0, 0, 0);
        }
        public void Update(float ElapsedTime)
        {
            Moment += ElapsedTime;
            while (Moment >= Period)
                Moment -= Period;
            var t = Body.CenterOfMassTransform;
            var p = new Vector3(Pos.X+Radio*FastMath.Cos((Moment/Period) * FastMath.TWO_PI), Pos.Y, Pos.Z + Radio * FastMath.Sin((Moment / Period) * FastMath.TWO_PI));
            Delta = p - t.Origin;
            t.Origin = p;
            Body.CenterOfMassTransform = t;
        }
    }
}
