using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using TGC.Core.Example;
using TGC.Core.Mathematica;

namespace TGC.Group.Model.GameObjects
{
    class PlataformaGiratoria : Plataforma
    {
        private float Radio = 0f;
        private float Period;
        private float Moment = 0f;
        public PlataformaGiratoria(float radio, TgcMesh mesh, TGCVector3 pos, float period)
        {
            Mesh = mesh;
            Radio = radio;
            Pos = pos;
            Period = period;
        }
        public override void Update(float ElapsedTime)
        {
            Moment += ElapsedTime;
            while (Moment >= Period)
                Moment -= Period;
            var old = Mesh.Position;
            Mesh.Position = new TGCVector3(Pos.X+Radio*FastMath.Cos((Moment/Period) * FastMath.TWO_PI), Pos.Y, Pos.Z + Radio * FastMath.Sin((Moment / Period) * FastMath.TWO_PI));
            Delta = Mesh.Position - old;
        }
    }
}
