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
    class PlataformaLineal : Plataforma
    {
        private float Rango;
        private bool EjeX;
        private float Velocidad;
        private bool Sentido = false;
        public PlataformaLineal(TgcMesh mesh, TGCVector3 pos, float rango, bool ejeX, float velocidad, bool direccion)
        {
            Mesh = mesh;
            Pos = pos;
            Mesh.Position = pos;
            EjeX = ejeX;
            Rango = rango;
            Velocidad = velocidad;
            Sentido = direccion;
        }
        public override void Update(float ElapsedTime)
        {
            var desplazamiento = Velocidad;
            desplazamiento *= ElapsedTime;
            if(!Sentido)
                desplazamiento *= -1f;
            var dist = EjeX ? Mesh.Position.X - Pos.X : Mesh.Position.Z - Pos.Z;
            desplazamiento = FastMath.Clamp(desplazamiento, -Rango-dist, Rango-dist);
            var movimiento = EjeX ? new TGCVector3(desplazamiento, 0, 0) : new TGCVector3(0, 0, desplazamiento);
            var old = Mesh.Position;
            Mesh.Position += movimiento;
            if (FastMath.Abs(EjeX ? Mesh.Position.X - Pos.X : Mesh.Position.Z - Pos.Z) >= Rango)
            {
                Sentido = !Sentido; 
            }
            Delta = movimiento;
        }

        public override String nombreClase()
        {
            return null;
        }

    }
}
