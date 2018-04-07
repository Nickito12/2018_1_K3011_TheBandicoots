using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model.GameObjects
{
    public abstract class GameObject
    {
        // La referencia al GameModel del juego
        protected GameModel env;

        public abstract void Init(GameModel _env);
        public abstract void Update();
        public abstract void Render();
        public abstract void Dispose();
    }
}
