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
        private List<TgcMesh> MeshConMovimiento = new List<TgcMesh>();
        private List<CajaEmpujable> ListaCajasEmpujables = new List<CajaEmpujable>();

        TgcMp3Player cancionPcpal = new TgcMp3Player();

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
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\asd19-TgcScene.xml");
            ListaPozos = Scene.Meshes.FindAll(m => m.Name.Contains("Pozo"));
            foreach (var mesh in ListaPozos)
            {
                Scene.Meshes.Remove(mesh);
            }
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

            // por el momento agrego una sola.. 
            TgcMesh MeshEmpujable = Scene.Meshes.Find(m => m.Name.Contains("Caja3"));
            TGCVector3 PosicionMeshEmpujable = MeshEmpujable.Position;
            CajaEmpujable CajaEmpujable = new CajaEmpujable(MeshEmpujable,PosicionMeshEmpujable);
            ListaCajasEmpujables.Add(CajaEmpujable);
            ///////////

            cancionPcpal.FileName = Env.MediaDir + "\\Sound\\crash.mp3";


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
                if(p.Name=="Box_202" || p.Name == "Box_204")
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
            Plataformas.Add(new PlataformaGiratoria(20, meshGiratorio,  new TGCVector3(260f, 0f, 275f), 5f));
            Scene.Meshes.Add(meshGiratorio);
            foreach (var plataforma in Plataformas)
            {
                ListaPlataformas.Add(plataforma.Mesh);
                MeshConMovimiento.Add(plataforma.Mesh);
                Scene.Meshes.Remove(plataforma.Mesh);
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
            Env.NuevaCamara.UpdateCamera(Env);
            if (cancionPcpal.getStatus() != TgcMp3Player.States.Playing)
            {
                cancionPcpal.closeFile();
                cancionPcpal.play(true);
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
            foreach (var pozo in ListaPozos)
            {
                pozo.Render();
            }
            if (Env.Input.keyDown(Key.LeftControl) || Env.Input.keyDown(Key.RightControl))
                foreach (TgcMesh mesh in ListaPozos)
                    mesh.BoundingBox.Render();
        }

        public override void Dispose()
        {
            Piso.Dispose();
            cancionPcpal.closeFile();
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
                        break;
                    }
                    else if(ListaPlataformas.Contains(Mesh))
                    {
                        if (Mesh.BoundingBox.PMax.Y > Env.Personaje.Mesh.Position.Y || Mesh.BoundingBox.PMin.Y < Env.Personaje.Mesh.Position.Y)
                        {
                            Colisionador = Mesh.BoundingBox;
                            break;
                        }
                        
                    }
                    else if (ListaPisosResbalosos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.PisoResbaloso);
                        break;
                    }
                    else if (ListaCajasEmpujables.Exists(UnaCajaEmpujable => UnaCajaEmpujable.Mesh.Equals(Mesh)))
                    {
                                          
                       CajaEmpujable cajaMoverse =  ListaCajasEmpujables.Find(UnaCajaEmpujable => UnaCajaEmpujable.Mesh.Equals(Mesh));

                        cajaMoverse.ColisionXZ(Env.Personaje);
                        break;
                    }

                    else
                    {
                        Colisionador = Mesh.BoundingBox;
                    }
                    break;
                }
            }
            foreach (var Mesh in MeshConMovimiento)
            {
                if (!ListaMeshesSinColision.Contains(Mesh) && Escenario.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    if (ListaPozos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                    }
                    else if (ListaPlataformas.Contains(Mesh))
                    {
                        if (Mesh.BoundingBox.PMax.Y > Env.Personaje.Mesh.Position.Y || (Mesh.BoundingBox.PMin.Y < Env.Personaje.Mesh.BoundingBox.PMax.Y && Mesh.BoundingBox.PMax.Y > Env.Personaje.Mesh.BoundingBox.PMax.Y))
                            Colisionador = Mesh.BoundingBox;
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
                    if (plataforma.Mesh.BoundingBox.PMin.Y < Env.Personaje.Mesh.BoundingBox.PMax.Y && plataforma.Mesh.BoundingBox.PMin.Y + (plataforma.Mesh.BoundingBox.PMax.Y - plataforma.Mesh.BoundingBox.PMin.Y) * 0.5f > Env.Personaje.Mesh.BoundingBox.PMax.Y)
                    {
                        Env.Personaje.setposition(plataforma.Mesh.BoundingBox.PMin - (Env.Personaje.Mesh.BoundingBox.PMax - Env.Personaje.Mesh.BoundingBox.PMin));
                        Env.Personaje.SetTipoColisionActual(TiposColision.Techo);
                    }
                    else
                    {
                        if (plataforma.Mesh.BoundingBox.PMin.Y < Env.Personaje.Mesh.Position.Y)
                            Colisionador = plataforma.Mesh.BoundingBox;
                        Env.Personaje.setposition(plataforma.deltaPosicion());
                        Env.Personaje.SetTipoColisionActual(TiposColision.Caja);
                    }
                }
            }
            if (Colisionador == null && Escenario.testAABBAABB(Piso.BoundingBox, boundingBox))
            {
                bool agujero = false;
                foreach (var pozo in ListaPozos)
                {
                    if (Escenario.testAABBAABBXZIn(boundingBox, pozo.BoundingBox))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                        agujero = true;
                        break;
                    }
                }
                if(!agujero)
                    Colisionador = Piso.BoundingBox;
            }
            return Colisionador;
        }

        public List<TgcMesh> listaColisionesConCamara()
        {
            return Scene.Meshes.FindAll(m => !ListaMeshesSinColision.Contains(m) && !ListaPisosResbalosos.Contains(m) && !ListaPozos.Contains(m));
        }
    }
}
