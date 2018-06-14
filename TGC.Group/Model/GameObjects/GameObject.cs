using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.BoundingVolumes;

namespace TGC.Group.Model.GameObjects
{
    public abstract class GameObject
    {
        // La referencia al GameModel del juego
        public GameModel Env;
        public enum TiposColision { SinColision, Pozo, Caja, PisoResbaloso, Techo, Escalon };

        public abstract void Init(GameModel _env);
        public abstract void Update();
        public abstract void Render();
        public abstract void Dispose();
        virtual public TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox box) { return null; }
        virtual public TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox box) { return null; }
    }
}
