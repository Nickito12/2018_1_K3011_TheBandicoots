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
    public abstract class Escenario : GameObject
    {
        protected TgcSkyBox SkyBox;
        protected TgcScene Scene;
        protected TgcSceneLoader Loader;

        protected void AddMesh(string carpeta, string nombre, TGCVector3 pos, int rotation = 0, TGCVector3? scale = null)
        {
            scale = scale ?? new TGCVector3(1, 1, 1);
            var Mesh = Loader.loadSceneFromFile(Env.MediaDir + 
                "Meshes\\" + carpeta + "\\" + nombre + "\\" + 
                nombre + "-TgcScene.xml").Meshes[0];
            Mesh.AutoTransform = true;
            Mesh.Position = pos;
            Mesh.Scale = scale.Value;
            Mesh.RotateY(FastMath.ToRad(rotation));
            Scene.Meshes.Add(Mesh);

        }
        protected void CreateSkyBox(TGCVector3 center, TGCVector3 size, string name)
        {
            SkyBox = new TgcSkyBox();
            SkyBox.Center = center;
            SkyBox.Size = size;
            var TexturesPath = Env.MediaDir + name + "\\";
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, TexturesPath + "Up.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, TexturesPath + "Down.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, TexturesPath + "Left.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, TexturesPath + "Right.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, TexturesPath + "Back.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, TexturesPath + "Front.jpg");
            SkyBox.Init();
        }
        public override void Render()
        {
            SkyBox.Render();
            Scene.RenderAll();
            //Dibujar bounding boxes de los mesh (Debugging)
            if (Env.Input.keyDown(Key.LeftControl) || Env.Input.keyDown(Key.RightControl))
                foreach (TgcMesh mesh in Scene.Meshes)
                    mesh.BoundingBox.Render();
        }
        public override void Dispose()
        {
            SkyBox.Dispose();
            Scene.DisposeAll();
        }
    }
}
