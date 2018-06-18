using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using static TGC.Group.Model.GameObjects.GameObject;
using TGC.Core.BoundingVolumes;

namespace TGC.Group.Model.GameObjects
{
    public class CajaEmpujable 
    {
        public TgcMesh Mesh;
        private TGCVector3 Delta;
        public float MargenError = 3f;
        public bool Caida = false;
        public TGCVector3 PosInicial;
        public float LongitudCaida = 10f;
        public float VelocidadCaida = 8f;
        
        public CajaEmpujable(TgcMesh mesh, TGCVector3 pos)
        {
            Mesh = mesh;
            Mesh.Position = pos;
            PosInicial = pos;
        }

        public TgcBoundingAxisAlignBox ColisionY(Character pj, float ElapsedTime)
        {
            var aabb = Mesh.BoundingBox;
            if (Caida)
            {
                if (Mesh.Position.Y > PosInicial.Y - LongitudCaida)
                    Mesh.Move(new TGCVector3(0f, -ElapsedTime * VelocidadCaida, 0f));
                return Escenario.EscenarioManual.testAABBAABB(pj.Mesh.BoundingBox, aabb) ? aabb : null;
            }
            if (Escenario.EscenarioManual.testAABBAABB(pj.Mesh.BoundingBox, aabb))
                if (pj.Mesh.BoundingBox.PMin.Y > aabb.PMax.Y - MargenError)
                {
                    pj.SetTipoColisionActual(TiposColision.SinColision);
                    return aabb;
                }
            return null;
        }

        public void ColisionXZ(Character pj)
        {
            if (Caida || Mesh.BoundingBox.PMax.Y- MargenError < pj.Mesh.BoundingBox.PMin.Y)
                return;
            Delta = pj.Position();
            Delta.Subtract(pj.posBeforeMovingInXZ());
            
           // moduloDelta = (delta.X + delta.Y + delta.Z);

            Mesh.Move(Delta*5);
        }

        // Caer en un pozo
        public void caer()
        {
            Caida = true;
        }
    }
}
