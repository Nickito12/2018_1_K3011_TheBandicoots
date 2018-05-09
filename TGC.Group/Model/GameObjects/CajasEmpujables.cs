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
    class CajaEmpujable 
    {
        public TgcMesh Mesh;
        private TGCVector3 delta;

        public CajaEmpujable(TgcMesh mesh, TGCVector3 pos)
        {
            Mesh = mesh;
            Mesh.Position = pos;
        }

        public void ColisionXZ(Character pj)
        {
            delta = pj.Position();
            delta.Subtract(pj.posBeforeMovingInXZ());
            
           // moduloDelta = (delta.X + delta.Y + delta.Z);

            Mesh.Move(delta*5);
        }
    }
}
