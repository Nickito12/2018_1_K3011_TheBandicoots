using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Sound;
using TGC.Core.Terrain;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;

namespace TGC.Group.Model.GameObjects
{
    public class Escenario1 : Escenario
    {
        // El piso del mapa/escenario
        private TgcPlane Piso;
        private List<TgcMesh> ListaPozos = new List<TgcMesh>();
        private List<TgcMesh> ListaPisos = new List<TgcMesh>();
        private List<TgcMesh> ListaPisosResbalosos = new List<TgcMesh>();
        private List<TgcMesh> ListaMeshesSinColision = new List<TgcMesh>();

        private TgcMp3Player mp3Player;

        //plataforma
        private TgcMesh plataforma1;
        private TGCMatrix escalaBase;
        private readonly float baseDX = 1.0f;
        private readonly float baseDY = 0.5f;
        private readonly float baseDZ = 1.0f;
        private const float ROTATION_SPEED = 1f;
        private const float MOVEMENT_SPEED = 0.1f;
        private float currentMoveDir = 1f;
        private TGCVector3 posicion;

        public override void Init(GameModel _env)
        {
            Env = _env;
            // Reset pj (Moverlo a la posicion inicial del escenario)
            Env.Personaje.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
            //Crear piso
            var PisoWidth = 1200f;
            var PisoLength = PisoWidth;
            var PisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "pasto.jpg");
            Piso = new TgcPlane(new TGCVector3(/*PisoWidth * -0.5f*/-200, 0f, PisoWidth * -0.5f), new TGCVector3(PisoWidth, 20f, PisoWidth), TgcPlane.Orientations.XZplane, PisoTexture, 15, 15);

            //Crear SkyBox
            CreateSkyBox(TGCVector3.Empty, new TGCVector3(10000, 10000, 10000), "SkyBox1");

            Loader = new TgcSceneLoader();
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\asd13-TgcScene.xml");
            ListaPozos = Scene.Meshes.FindAll(m => m.Name.Contains("Pozo"));
            ListaPisos = Scene.Meshes.FindAll(m => m.Name.Contains("Box"));
            ListaPisosResbalosos = Scene.Meshes.FindAll(m => m.Name.Contains("PisoResbaloso"));
            ListaMeshesSinColision.Add(Scene.Meshes.Find(m => m.Name.Contains("ParedEnvolvente001233")));
            ListaMeshesSinColision.Add(Scene.Meshes.Find(m => m.Name.Contains("ParedEnvolvente001248")));

            mp3Player = new TgcMp3Player();
            //mp3Player.FileName = Env.MediaDir + "\\Sound\\song.mp3";
            mp3Player.FileName = Env.MediaDir + "\\Sound\\crash.mp3";
            mp3Player.play(true);

            //1er plataforma
            plataforma1 = Scene.Meshes.Find(m => m.Name.Contains("Box_1"));
            plataforma1.Transform = TGCMatrix.Identity;
            plataforma1.AutoTransform = true;
        }
        public override void Update()
        {
            //guardo la posicion para el personaje
            posicion = new TGCVector3(MOVEMENT_SPEED * currentMoveDir, 0, 0);

            //para que la plataforma se mueva
            escalaBase = TGCMatrix.Scaling(baseDX, baseDY, baseDZ);
            plataforma1.Move(MOVEMENT_SPEED * currentMoveDir, 0, 0);
            if (FastMath.Abs(plataforma1.Position.X) > 30f)
            {
                currentMoveDir *= -1;
            }
            plataforma1.getVertexPositions();
            
        }
        public override void Render()
        {
            Piso.Render();
            base.Render();

            //1er plataforma
            plataforma1.Transform = escalaBase;
            plataforma1.Render();
        }
        public override void Dispose()
        {
            Piso.Dispose();
            mp3Player.closeFile();
            base.Dispose();
            
            //1era plataforma
            plataforma1.Dispose();
        }
        public override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox)
        {
            // null => no hay colision
            TgcBoundingAxisAlignBox Colisionador = null;
            foreach (var Mesh in Scene.Meshes)
            {
                if (ListaMeshesSinColision.Contains(Mesh) && TgcCollisionUtils.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    break;
                }
                else if (ListaPozos.Contains(Mesh) && TgcCollisionUtils.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                    break;
                }
                else if (ListaPisos.Contains(Mesh) && TgcCollisionUtils.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
					//Para la 1era plataforma
                    Env.Personaje.SetTipoColisionActual(TiposColision.Caja);
                    Env.Personaje.setposition(posicion);
                    break;
                }

                else if (ListaPisosResbalosos.Contains(Mesh) && TgcCollisionUtils.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    Env.Personaje.SetTipoColisionActual(TiposColision.PisoResbaloso);
                    break;
                }

                else if (TgcCollisionUtils.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    Colisionador = Mesh.BoundingBox;
                    break;
                }
            }
            return Colisionador;
        }
        public override TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox)
        {
            TgcBoundingAxisAlignBox Colisionador = null;
            foreach (var Mesh in ListaPisos) {
                if (TgcCollisionUtils.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    Colisionador = Mesh.BoundingBox;
                    break;
                }

            }

            if (Colisionador == null && TgcCollisionUtils.testAABBAABB(Piso.BoundingBox, boundingBox)) {
                Colisionador = Piso.BoundingBox;
            }

            return Colisionador;
        }
    }
}
