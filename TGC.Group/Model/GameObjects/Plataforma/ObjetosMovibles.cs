using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model.GameObjects
{
    class ObjetosMovibles: Plataforma
    {
        private float Rango;
        private bool EjeX;
        private float Velocidad;
        private bool Sentido = false;
        public ObjetosMovibles(TgcMesh mesh, TGCVector3 pos)
        {
            Mesh = mesh;
            Pos = pos;
            Mesh.Position = pos;
            
        }

        public override void Update(float ElapsedTime)
        {

        }

        public override String nombreClase()
        {
            return "MOVIL";
        }
    }
}
