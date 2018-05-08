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
    abstract class Plataforma
    {
        public TgcMesh Mesh;
        protected TGCVector3 Pos;
        protected TGCVector3 Delta;
        abstract public void Update(float ElapsedTime);
        public TGCVector3 deltaPosicion()
        {
            return Delta;
        }
        abstract public String nombreClase();
        
    }
}
