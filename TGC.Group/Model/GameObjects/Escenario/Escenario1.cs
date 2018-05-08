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
using TGC.Group.Model.Estructuras;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model.GameObjects
{
    public class Escenario1 : Escenario
    {
        // El piso del mapa/escenario
        private TgcPlane Piso;
        private List<TgcMesh> ListaPozos = new List<TgcMesh>();
        private List<TgcMesh> ListaPlataformas = new List<TgcMesh>();
        private List<TgcMesh> ListaPisosResbalosos = new List<TgcMesh>();
        private List<TgcMesh> ListaMeshesSinColision = new List<TgcMesh>();
        private List<TgcMesh> ListaP = new List<TgcMesh>();

        private TgcMp3Player mp3Player;

        private const float ROTATION_SPEED = 1f;
        private List<Plataforma> Plataformas;
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
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\asd18-TgcScene.xml");
            ListaPozos = Scene.Meshes.FindAll(m => m.Name.Contains("Pozo"));
            foreach (var mesh in Scene.Meshes.FindAll(m => m.Name.Contains("Arbusto"))) {
                mesh.BoundingBox.scaleTranslate(new TGCVector3(0, 0, 0), new TGCVector3(1, 10, 1));
            }
            foreach (var mesh in Scene.Meshes.FindAll(m => m.Name.Contains("Arbusto2")))
            {
                mesh.BoundingBox.scaleTranslate(new TGCVector3(0, 0, 0), new TGCVector3(1, 10, 1));
            }
            foreach (var mesh in Scene.Meshes.FindAll(m => m.Name.Contains("Flores")))
            {
                mesh.BoundingBox.scaleTranslate(new TGCVector3(0, 0, 0), new TGCVector3(1, 10, 1));
            }
            ListaPisosResbalosos = Scene.Meshes.FindAll(m => m.Name.Contains("PisoResbaloso"));
            ListaMeshesSinColision.Add(Scene.Meshes.Find(m => m.Name.Contains("ParedEnvolvente001233")));
            ListaMeshesSinColision.Add(Scene.Meshes.Find(m => m.Name.Contains("ParedEnvolvente001248")));

            mp3Player = new TgcMp3Player();
            //mp3Player.FileName = Env.MediaDir + "\\Sound\\song.mp3";
            mp3Player.FileName = Env.MediaDir + "\\Sound\\crash.mp3";
            mp3Player.play(true);


            Plataformas = new List<Plataforma>();

            ListaP = Scene.Meshes.FindAll(m => m.Name.Contains("Box"));
            foreach (var p in ListaP)
            {
                if (p.Name != "Box_1")
                {
                    Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, 0f), 26f, false, 12f));
                }
                else
                {
                    Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, 0f), 26f, true, 12f));
                }

            }

            //Plataformas.Add(new PlataformaLineal(Scene.Meshes.Find(m => m.Name.Contains("Box_1")), new TGCVector3(0f, 0f, 0f), 26f, true, 12f));
            Plataformas.Add(new PlataformaGiratoria(20, Plataformas[0].Mesh.clone("pGira"), new TGCVector3(260f, 0f, 275f), 5f));
            foreach (var plataforma in Plataformas)
            {
                ListaPlataformas.Add(plataforma.Mesh);
            }

            KDTree = new GrillaRegular();
            KDTree.create(Scene.Meshes.FindAll(m => !m.Name.Contains("Box")), Scene.BoundingBox);
            KDTree.createDebugMeshes();
        }
        public override void Update()
        {
            if (Env.ElapsedTime > 10000)
                return;
            if (Env.Personaje.Position().Y <= -100)
                Env.Personaje.Position(new TGCVector3(0, 1, 0));
            ShowKdTree = Env.Input.keyDown(Key.F3);
            foreach (var plataforma in Plataformas)
            {
                plataforma.Update(Env.ElapsedTime);
            }
        }
        public override void Render()
        {
            Piso.Render();
            base.Render();

            foreach (var plataforma in Plataformas)
            {
                plataforma.Mesh.Render();
            }
        }
        public override void Dispose()
        {
            Piso.Dispose();
            mp3Player.closeFile();
            base.Dispose();
        }
        public override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox)
        {
            // null => no hay colision
            TgcBoundingAxisAlignBox Colisionador = null;
            foreach (var Mesh in Scene.Meshes.FindAll(m=>m.Enabled))
            {
                if (!ListaMeshesSinColision.Contains(Mesh) && Escenario.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    if (ListaPozos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                    }
                    else if(ListaPlataformas.Contains(Mesh))
                    {
                        var Plataforma = Plataformas.Find(p=> p.Mesh == Mesh);
                        Env.Personaje.SetTipoColisionActual(TiposColision.Caja);
                        Env.Personaje.setposition(Plataforma.deltaPosicion());
                    }
                    else if (ListaPisosResbalosos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.PisoResbaloso);
                    }
                    else
                    {
                        Colisionador = Mesh.BoundingBox;
                    }
                    break;
                }
            }
            return Colisionador;
        }
        public override TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox)
        {
            TgcBoundingAxisAlignBox Colisionador = null;
            foreach (var plataforma in Plataformas)
            {
                if (Escenario.testAABBAABB(plataforma.Mesh.BoundingBox, boundingBox))
                {
                    Env.Personaje.setposition(plataforma.deltaPosicion());
                    Env.Personaje.SetTipoColisionActual(TiposColision.Caja);
                    Colisionador = plataforma.Mesh.BoundingBox;
                    break;
                }
            }
            foreach (var pozo in ListaPozos)
            {
                if (Escenario.testAABBAABB(pozo.BoundingBox, boundingBox))
                {
                    Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                    break;
                }
            }
            if (Colisionador == null && Escenario.testAABBAABB(Piso.BoundingBox, boundingBox)) {
                Colisionador = Piso.BoundingBox;
            }
            return Colisionador;
        }
    }
}
