using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;

namespace TGC.Group.Model.GameObjects
{
    public class Escenario1 : GameObject
    {
        // El piso del mapa/escenario
        private TgcPlane piso;

        public Escenario1(Character pj)
        {
            // Reset pj (Moverlo a la posicion inicial del escenario)
            pj.Move(new TGCVector3(0, 0, 0), new TGCVector3(-30, 0, 0));
        }
        public override void Init(GameModel _env)
        {
            env = _env;
            //Crear piso
            var pisoWidth = 1000f;
            var pisoLength = pisoWidth;
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, env.MediaDir + "cajaMadera4.jpg");
            piso = new TgcPlane(new TGCVector3(pisoWidth * -0.5f, -1f, pisoWidth * -0.5f), new TGCVector3(pisoWidth, 20f, pisoWidth), TgcPlane.Orientations.XZplane, pisoTexture);
        }
        public override void Update()
        {
            
        }
        public override void Render()
        {
            piso.Render();
            piso.BoundingBox.Render();
        }
        public override void Dispose()
        {
            piso.Dispose();
        }
        public override bool Collision(TgcBoundingAxisAlignBox boundingBox)
        {
            return TgcCollisionUtils.testAABBAABB(piso.BoundingBox, boundingBox);
        }
    }
}
