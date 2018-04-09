using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Terrain;

namespace TGC.Group.Model.GameObjects
{
    public class Escenario1 : GameObject
    {
        // El piso del mapa/escenario
        private TgcPlane Piso;
        private TgcSkyBox SkyBox;

        public Escenario1(Character pj)
        {
            // Reset pj (Moverlo a la posicion inicial del escenario)
            pj.Move(new TGCVector3(0, 0, 0), new TGCVector3(-30, 0, 0));
        }
        public override void Init(GameModel _env)
        {
            Env = _env;
            //Crear piso
            var PisoWidth = 1000f;
            var PisoLength = PisoWidth;
            var PisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "pasto.jpg");
            Piso = new TgcPlane(new TGCVector3(PisoWidth * -0.5f, 0f, PisoWidth * -0.5f), new TGCVector3(PisoWidth, 20f, PisoWidth), TgcPlane.Orientations.XZplane, PisoTexture);

            //Crear SkyBox
            SkyBox = new TgcSkyBox();
            SkyBox.Center = TGCVector3.Empty;
            SkyBox.Size = new TGCVector3(10000, 10000, 10000);
            var TexturesPath = Env.MediaDir + "SkyBox1\\";
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, TexturesPath + "Up.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, TexturesPath + "Down.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, TexturesPath + "Left.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, TexturesPath + "Right.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, TexturesPath + "Back.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, TexturesPath + "Front.jpg");
            SkyBox.Init();
        }
        public override void Update()
        {
            
        }
        public override void Render()
        {
            Piso.Render();
            SkyBox.Render();
        }
        public override void Dispose()
        {
            Piso.Dispose();
            SkyBox.Dispose();
        }
        public override bool Collision(TgcBoundingAxisAlignBox boundingBox)
        {
            return TgcCollisionUtils.testAABBAABB(Piso.BoundingBox, boundingBox);
        }
    }
}
