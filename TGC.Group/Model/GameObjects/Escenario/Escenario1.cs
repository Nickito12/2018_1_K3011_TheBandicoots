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

        public override void Init(GameModel _env)
        {
            Env = _env;
            // Reset pj (Moverlo a la posicion inicial del escenario)
            Env.Personaje.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
            //Crear piso
            var PisoWidth = 1200f;
            var PisoLength = PisoWidth;
            var PisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "pasto.jpg");
            Piso = new TgcPlane(new TGCVector3(/*PisoWidth * -0.5f*/-200, 0f, PisoWidth * -0.5f), new TGCVector3(PisoWidth, 5f, PisoWidth), TgcPlane.Orientations.XZplane, PisoTexture, 15, 15);

            //Crear SkyBox
            CreateSkyBox(TGCVector3.Empty, new TGCVector3(10000, 10000, 10000), "SkyBox1");

            // Cargar escena
            Loader = new TgcSceneLoader();
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\asd19-TgcScene.xml");

            // Pozos
            ListaPozos = Scene.Meshes.FindAll(m => m.Name.Contains("Pozo"));
            foreach (var mesh in ListaPozos)
            {
                Scene.Meshes.Remove(mesh);
            }

            // Alargar algunas AABB
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

            // Cajas Empujables
            TgcMesh MeshEmpujable = Scene.Meshes.Find(m => m.Name.Contains("Caja3"));
            Scene.Meshes.Remove(Scene.Meshes.Find(m => m.Name.Contains("Caja3")));
            TGCVector3 PosicionMeshEmpujable = MeshEmpujable.Position;
            CajaEmpujable CajaEmpujable = new CajaEmpujable(MeshEmpujable, new TGCVector3(0f, 0f, 0f));
            ListaCajasEmpujables.Add(CajaEmpujable);

            // Setear cancion
            cancionPcpal.FileName = Env.MediaDir + "\\Sound\\crash.mp3";

            // Setear plataformas
            Plataformas = new List<Plataforma>();
            List<TgcMesh> ListaPlataformaEstatica = new List<TgcMesh>();
            List<TgcMesh> ListaPlataformaX = new List<TgcMesh>();
            List<TgcMesh> ListaPlataformaZ = new List<TgcMesh>();
            List<TgcMesh> ListaMovibles = new List<TgcMesh>();
            ListaPlataformaEstatica = Scene.Meshes.FindAll(m => m.Name.Contains("Box_0"));
            ListaPlataformaX = Scene.Meshes.FindAll(m => m.Name.Contains("Box_1"));
            ListaPlataformaZ = Scene.Meshes.FindAll(m => m.Name.Contains("Box_2"));
            ListaMovibles = Scene.Meshes.FindAll(m => m.Name.Contains("Box_M"));

            //agrego plataforma que se mueven en X
            foreach (var p in ListaPlataformaX)
            {
                Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, 0f), 25f, true, 12f, false));
            }

            //agrego plataforma que se mueven en Z
            foreach (var p in ListaPlataformaZ)
            {
                //-20f es para que este centrado en el camino
                if (p.Name == "Box_202" || p.Name == "Box_204")
                {
                    Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, -20f), 25f, false, 12f, true));
                }
                else
                {
                    Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, -20f), 25f, false, 12f, false));
                }
            }

            //agrego objetos moviles
            foreach (var p in ListaMovibles)
            {
                Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, 0f), 0f, false, 0f, false));
            }

            //agrego objetos estaticos
            foreach (var p in ListaPlataformaEstatica)
            {
                Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, 0f), 0f, false, 0f, false));
            }

            //se agrega plataforma giratoria
            var meshGiratorio = Plataformas[0].Mesh.clone("pGira");
            Plataformas.Add(new PlataformaGiratoria(20, meshGiratorio, new TGCVector3(260f, 0f, 275f), 5f));
            Scene.Meshes.Add(meshGiratorio);
            foreach (var plataforma in Plataformas)
            {
                ListaPlataformas.Add(plataforma.Mesh);
                MeshConMovimiento.Add(plataforma.Mesh);
                Scene.Meshes.Remove(plataforma.Mesh);
            }

            Grilla = new GrillaRegular();
            Grilla.create(Scene.Meshes.FindAll(m => !m.Name.Contains("Box")), Scene.BoundingBox);
            Grilla.createDebugMeshes();
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

        public override TgcBoundingAxisAlignBox ColisionConPiso(TgcBoundingAxisAlignBox boundingBox)
        {
            return Escenario.testAABBAABB(Piso.BoundingBox, boundingBox) ? Piso.BoundingBox : null;
        }

        public List<TgcMesh> listaColisionesConCamara()
        {
            return Scene.Meshes.FindAll(m => !ListaMeshesSinColision.Contains(m) && !ListaPisosResbalosos.Contains(m) && !ListaPozos.Contains(m));
        }
    }
}
