using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Terrain;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model.GameObjects
{
    public class Escenario1 : GameObject
    {
        // El piso del mapa/escenario
        private TgcPlane Piso;
        private TgcSkyBox SkyBox;
        private TgcSceneLoader Loader;
        private TgcScene Scene;

        public Escenario1(Character pj)
        {
            // Reset pj (Moverlo a la posicion inicial del escenario)
            pj.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
            
        }
        public override void Init(GameModel _env)
        {
            Env = _env;
            //Crear piso
            var PisoWidth = 1200f;
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

            Loader = new TgcSceneLoader();
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\ESCENACALABERITANUEVA-TgcScene.xml");

            /* AddVegetacion("Planta", new TGCVector3(-205, 0,  49), 0, new TGCVector3(0.6f, 0.6f, 0.6f));
             AddVegetacion("Arbusto", new TGCVector3(231, -5, 40), 66, new TGCVector3(1.15f, 1.15f, 1.15f));
             AddVegetacion("Arbusto2", new TGCVector3(150, -5, -110), 164, new TGCVector3(0.95f, 0.95f, 0.95f));
             AddVegetacion("Planta2", new TGCVector3(170, -5, -170), 205, new TGCVector3(0.55f, 1.1f, 0.55f));
             AddVegetacion("Planta3", new TGCVector3(-187, -5, 166), 276, new TGCVector3(0.55f, 1f, 0.55f));
             AddVegetacion("ArbolSelvatico", new TGCVector3(90, 0, -190), 143, new TGCVector3(0.15f, 0.15f, 0.15f));
             AddVegetacion("ArbolSelvatico", new TGCVector3(-180, 0, 260), 43, new TGCVector3(0.2f, 0.2f, 0.2f));

             AddVegetacion("Planta", new TGCVector3(200, 0, -250), 0, new TGCVector3(0.4f, 0.4f, 0.4f));
             AddVegetacion("Arbusto", new TGCVector3(164, -5, -260), 66, new TGCVector3(1f, 1f, 1f));
             AddVegetacion("Arbusto2", new TGCVector3(60, -5, -230), 164, new TGCVector3(0.7f, 0.7f, 0.7f));
             AddVegetacion("Planta2", new TGCVector3(-300, -5, 50), 205, new TGCVector3(0.3f, 0.7f, 0.3f));
             AddVegetacion("Planta3", new TGCVector3(100, -5, 240), 276, new TGCVector3(0.6f, 1.1f, 0.6f));
             AddVegetacion("ArbolSelvatico", new TGCVector3(45, 0, 250), 143, new TGCVector3(0.05f, 0.05f, 0.05f));
             AddVegetacion("ArbolSelvatico", new TGCVector3(-300, 0, 200), 43, new TGCVector3(0.05f, 0.05f, 0.05f));

             AddVegetacion("Planta", new TGCVector3(50, 0, 0), 0, new TGCVector3(0.5f, 0.5f, 0.5f));
             AddVegetacion("Arbusto", new TGCVector3(164, -5, 150), 66, new TGCVector3(1.1f, 1.1f, 1.1f));
             AddVegetacion("Arbusto2", new TGCVector3(60, -5, 290), 164, new TGCVector3(0.9f, 0.9f, 0.9f));
             AddVegetacion("Planta2", new TGCVector3(-100, -5, -300), 205, new TGCVector3(0.5f, 1f, 0.5f));
             AddVegetacion("Planta3", new TGCVector3(-100, -5, -280), 276, new TGCVector3(0.5f, 1f, 0.5f));
             AddVegetacion("ArbolSelvatico", new TGCVector3(45, 0, 190), 143, new TGCVector3(0.1f, 0.1f, 0.1f));
             AddVegetacion("ArbolSelvatico", new TGCVector3(-140, 0, -190), 43, new TGCVector3(0.15f, 0.15f, 0.15f)); */
        }
        public override void Update()
        {
            
        }
        public override void Render()
        {
            Piso.Render();
            SkyBox.Render();
            Scene.RenderAll();
            //Dibujar bounding boxes de los mesh (Debugging?)
            /*
            foreach (TgcMesh mesh in Meshes.Meshes)
                mesh.BoundingBox.Render();
            */
        }
        public override void Dispose()
        {
            Piso.Dispose();
            SkyBox.Dispose();
            Scene.DisposeAll();
        }
        public override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox)
        {
           TgcBoundingAxisAlignBox HuboColision = null;
            foreach (var Mesh in Scene.Meshes)
            {
                if (TgcCollisionUtils.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    HuboColision = Mesh.BoundingBox;
                    break;
                }
            }
            return HuboColision;
        }
        public override TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox)
        {
            return TgcCollisionUtils.testAABBAABB(Piso.BoundingBox, boundingBox) ? Piso.BoundingBox : null;
            
        }
      /*  private void AddVegetacion(string nombre, TGCVector3 pos, int rotation=0, TGCVector3? scale = null)
        {
            scale = scale ?? new TGCVector3(1, 1, 1);
            var Mesh = Loader.loadSceneFromFile(Env.MediaDir + "Meshes\\Vegetacion\\" + nombre + "\\" + nombre + "-TgcScene.xml").Meshes[0];
            Mesh.AutoTransform = true;
            Mesh.Position = pos;
            Mesh.Scale = scale.Value;
            Mesh.RotateY(FastMath.ToRad(rotation));
            Meshes.Meshes.Add(Mesh);

        }

    */
    }
}
