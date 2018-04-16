using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Terrain;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.DirectInput;

namespace TGC.Group.Model.GameObjects
{
    public class Escenario1 : Escenario
    {
        // El piso del mapa/escenario
        private TgcPlane Piso;

        public override void Init(GameModel _env)
        {
            Env = _env;
            // Reset pj (Moverlo a la posicion inicial del escenario)
            Env.Personaje.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
            //Crear piso
            var PisoWidth = 1200f;
            var PisoLength = PisoWidth;
            var PisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "pasto.jpg");
            Piso = new TgcPlane(new TGCVector3(PisoWidth * -0.5f, 0f, PisoWidth * -0.5f), new TGCVector3(PisoWidth, 20f, PisoWidth), TgcPlane.Orientations.XZplane, PisoTexture);

            //Crear SkyBox
            CreateSkyBox(TGCVector3.Empty, new TGCVector3(10000, 10000, 10000), "SkyBox1");

            Loader = new TgcSceneLoader();
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\ESCENACALABERITANUEVA-TgcScene.xml");
        }
        public override void Update()
        {
            
        }
        public override void Render()
        {
            Piso.Render();
            base.Render();
        }
        public override void Dispose()
        {
            Piso.Dispose();
            base.Dispose();
        }
        public override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox)
        {
            // null => no hay colision
            TgcBoundingAxisAlignBox Colisionador = null;
            foreach (var Mesh in Scene.Meshes)
            {
                if (TgcCollisionUtils.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    Colisionador = Mesh.BoundingBox;
                    break;
                }
            }
            return Colisionador;
        }
        public override TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox)
        {
            return TgcCollisionUtils.testAABBAABB(Piso.BoundingBox, boundingBox) ? Piso.BoundingBox : null;
        }
    }
}
