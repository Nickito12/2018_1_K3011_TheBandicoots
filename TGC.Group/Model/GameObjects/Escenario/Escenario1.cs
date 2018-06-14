using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Sound;
using TGC.Core.Terrain;
using TGC.Core.SceneLoader;
using System.Collections.Generic;
using TGC.Group.Model.Estructuras;
using Microsoft.DirectX.Direct3D;
using System;

namespace TGC.Group.Model.GameObjects.Escenario
{
    public class Escenario1 : Escenario
    {
        // El piso del mapa/escenario
        //private TgcPlane PisoSelva;
        private TgcPlane PisoSelva1;
        private TgcPlane PisoSelva2;
        private TgcPlane PisoSelva3;
        private TgcPlane PisoSelva4;
        private TgcPlane PisoSelva5;
        private TgcPlane PisoSelva6;
        private TgcPlane PisoSelva7;
        private TgcPlane PisoSelva8;
        private TgcPlane PisoSelva9;
        private TgcPlane PisoSelva10;
        private TgcPlane PisoSelva11;
        private TgcPlane PisoSelva12;
        private TgcPlane PisoSelva13;
        private TgcPlane PisoSelva14;
        private TgcPlane PisoSelva15;
        private TgcPlane PisoSelva16;
        private TgcPlane PisoCastilloEntrada;
        private TgcPlane PisoCastilloMain;
        private TgcBoundingAxisAlignBox checkpoint = new TgcBoundingAxisAlignBox(
             new TGCVector3(799, 0, -97), new TGCVector3(870, 1000, -4));
        private bool checkpointReached = false;

        public override void Init(GameModel _env)
        {
            Env = _env;
            string compilationErrors;
            var d3dDevice = D3DDevice.Instance.Device;
            EfectoRender3D = Effect.FromFile(d3dDevice, Env.ShadersDir + "render3D.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (EfectoRender3D == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            EfectoRender2D = Effect.FromFile(d3dDevice, Env.ShadersDir + "render2D.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (EfectoRender2D == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            EfectoRender2D.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            EfectoRender2D.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);
            try
            {
                texturaVida = TextureLoader.FromFile(d3dDevice, Env.MediaDir + "\\Menu\\heart.png");
            }
            catch
            {
                throw new Exception(string.Format("Error at loading texture: {0}", Env.MediaDir + "\\Menu\\heart.png"));
            }
            Reset();
            //Crear pisos
            /*var PisoSelvaWidth = 1200f;
            var PisoSelvaLength = PisoSelvaWidth;
            var PisoSelvaTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "pasto.jpg");
            PisoSelva = new TgcPlane(new TGCVector3(-200f, 0f, -100f), new TGCVector3(PisoSelvaWidth, 5f, PisoSelvaWidth), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva);*/
            var PisoSelvaTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "pasto.jpg");
            PisoSelva1 = new TgcPlane(new TGCVector3(-81f, 0f, -74f), new TGCVector3(168f, 5f, 161f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva1);
            PisoSelva2 = new TgcPlane(new TGCVector3(-81f, 0f, 150f), new TGCVector3(168f, 5f, 110f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva2);
            PisoSelva3 = new TgcPlane(new TGCVector3(-81f, 0f, 278f), new TGCVector3(302f, 5f, 247f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva3);
            PisoSelva4 = new TgcPlane(new TGCVector3(309f, 0f, 278f), new TGCVector3(111f, 5f, 247f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva4);
            PisoSelva5 = new TgcPlane(new TGCVector3(595f, 0f, 236f), new TGCVector3(387f, 5f, 350f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva5);
            PisoSelva6 = new TgcPlane(new TGCVector3(707f, 0f, -50f), new TGCVector3(259f, 5f, 77f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva6);
            PisoSelva7 = new TgcPlane(new TGCVector3(834f, 0f, 49f), new TGCVector3(132f, 5f, 48f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva7);
            PisoSelva8 = new TgcPlane(new TGCVector3(707f, 0f, 147.5f), new TGCVector3(130f, 5f, 38f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva8);
            PisoSelva9 = new TgcPlane(new TGCVector3(707f, -0.1f, 0f), new TGCVector3(81f, 5f, 148f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva9);
            PisoSelva10 = new TgcPlane(new TGCVector3(881f, -0.1f, -0.2f), new TGCVector3(79.5f, 5f, 50f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva10);
            PisoSelva11 = new TgcPlane(new TGCVector3(883f, -0.1f, 96f), new TGCVector3(83f, 5f, 145f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva11);
            PisoSelva12 = new TgcPlane(new TGCVector3(43.7f, -0.1f, 219f), new TGCVector3(552f, 5f, 133.5f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva12);
            PisoSelva13 = new TgcPlane(new TGCVector3(218f, -0.1f, 432f), new TGCVector3(379f, 5f, 148.5f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva13);
            PisoSelva14 = new TgcPlane(new TGCVector3(-81f, -0.1f, 86f), new TGCVector3(40f, 5f, 193f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva14);
            PisoSelva15 = new TgcPlane(new TGCVector3(41f, -0.1f, 82f), new TGCVector3(46f, 5f, 70f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva15);
            PisoSelva16 = new TgcPlane(new TGCVector3(705f, -0.1f, 185f), new TGCVector3(84f, 5f, 58f), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);
            ListaPlanos.Add(PisoSelva16);



            var PisoCastilloEntradaWidth = 250f;
            var PisoCastilloEntradaLength = 550f;
            var PisoCastilloEntradaTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "rockfloor.jpg");
            PisoCastilloEntrada = new TgcPlane(new TGCVector3(700f, 21f, -650f), new TGCVector3(PisoCastilloEntradaWidth, 5f, PisoCastilloEntradaLength), TgcPlane.Orientations.XZplane, PisoCastilloEntradaTexture, 15, 15);
            ListaPlanos.Add(PisoCastilloEntrada);

            var PisoCastilloMainWidth = 800f;
            var PisoCastilloMainLength = 800f;
            var PisoCastilloMainTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "rockfloor.jpg");
            PisoCastilloMain = new TgcPlane(new TGCVector3(950f, 21f, -650f), new TGCVector3(PisoCastilloMainWidth, 5f, PisoCastilloMainLength), TgcPlane.Orientations.XZplane, PisoCastilloMainTexture, 15, 15);
            ListaPlanos.Add(PisoCastilloMain);


            //Crear SkyBox
            CreateSkyBox(TGCVector3.Empty, new TGCVector3(10000, 10000, 10000), "SkyBox1");

            // Cargar escena
            Loader = new TgcSceneLoader();
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\escenarioEscalon-TgcScene.xml");

            // Paredes
            ListaParedes = Scene.Meshes.FindAll(m => m.Name.Contains("ParedCastillo"));
            TgcMesh paredSinBB = Scene.Meshes.Find(m => m.Name.Contains("ParedCastillo441"));
            ListaParedes.Remove(paredSinBB); //elimino la pared que no necesita agrandar su BB
            foreach (var m in ListaParedes) {
                m.BoundingBox = new TgcBoundingAxisAlignBox(m.BoundingBox.PMin - new TGCVector3(5, 0, 5), m.BoundingBox.PMax+ new TGCVector3(5, 0, 5));
            }

            // Pozos
            ListaPozos = Scene.Meshes.FindAll(m => m.Name.Contains("Pozo"));
            foreach (var mesh in ListaPozos)
            {
                Scene.Meshes.Remove(mesh);
            }

            // Alargar algunas AABB
            var r = new Random();
            foreach (var mesh in Scene.Meshes.FindAll(m => m.Name.Contains("Arbusto"))) {
                mesh.BoundingBox.scaleTranslate(new TGCVector3(0, 0, 0), new TGCVector3(1, 10, 1));
                mesh.AutoTransform = false;
                var ang = r.Next(0, 180);
                var p = (mesh.BoundingBox.PMax + mesh.BoundingBox.PMin)*0.5f;
                var s = new TGCVector3(((float)r.Next(90, 110))/100f, 1, ((float)r.Next(90, 110))/100f);
                mesh.Transform = TGCMatrix.Translation(-1 * p) * TGCMatrix.Scaling(s) * TGCMatrix.RotationY(ang)  * TGCMatrix.Translation(p);
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
            /*TgcMesh MeshEmpujable = Scene.Meshes.Find(m => m.Name.Contains("Caja3"));
            Scene.Meshes.Remove(Scene.Meshes.Find(m => m.Name.Contains("Caja3")));
            TGCVector3 PosicionMeshEmpujable = MeshEmpujable.Position;
            CajaEmpujable CajaEmpujable = new CajaEmpujable(MeshEmpujable, new TGCVector3(0f, 0f, 0f));
            ListaCajasEmpujables.Add(CajaEmpujable);*/
            foreach (var mesh in Scene.Meshes.FindAll(m => m.Name.Contains("Caja3")))
            {
                Scene.Meshes.Remove(mesh);
                TGCVector3 PosicionMeshEmpujable = mesh.Position;
                CajaEmpujable CajaEmpujable = new CajaEmpujable(mesh, new TGCVector3(0f, 0f, 0f));
                ListaCajasEmpujables.Add(CajaEmpujable);
            }

            // Setear cancion
            cancionPcpal.FileName = Env.MediaDir + "\\Sound\\crash.mp3";

            // Setear plataformas
            Plataformas = new List<Plataforma>();
            List<TgcMesh> ListaPlataformaEstatica = new List<TgcMesh>();
            List<TgcMesh> ListaPlataformaX = new List<TgcMesh>();
            List<TgcMesh> ListaPlataformaZ = new List<TgcMesh>();
            List<TgcMesh> ListaMovibles = new List<TgcMesh>();
            List<TgcMesh> Escalones = new List<TgcMesh>();
            ListaPlataformaEstatica = Scene.Meshes.FindAll(m => m.Name.Contains("Box_0"));
            ListaPlataformaX = Scene.Meshes.FindAll(m => m.Name.Contains("Box_1"));
            ListaPlataformaZ = Scene.Meshes.FindAll(m => m.Name.Contains("Box_2"));
            ListaMovibles = Scene.Meshes.FindAll(m => m.Name.Contains("Box_M"));
            Escalones = Scene.Meshes.FindAll(m => m.Name.Contains("Escalon"));

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

            //se agregan plataformas giratorias
            var meshGiratorio = Plataformas[0].Mesh.clone("pGira");
            Plataformas.Add(new PlataformaGiratoria(20, meshGiratorio, new TGCVector3(260f, 0f, 275f), 5f));
            Scene.Meshes.Add(meshGiratorio);
            var meshGiratorio2 = ListaPlataformaZ[4].clone("pGira2");
            Plataformas.Add(new PlataformaGiratoria(32, meshGiratorio2, new TGCVector3(75f, 0f, -20f), 5f));
            Scene.Meshes.Add(meshGiratorio2);
            var meshGiratorio3 = ListaPlataformaZ[4].clone("pGira3");
            Plataformas.Add(new PlataformaGiratoria(31, meshGiratorio3, new TGCVector3(-135f, 0f, 575f), 5f));
            Scene.Meshes.Add(meshGiratorio3);

            foreach (var plataforma in Plataformas)
            {
                ListaPlataformas.Add(plataforma.Mesh);
                MeshConMovimiento.Add(plataforma.Mesh);
                Scene.Meshes.Remove(plataforma.Mesh);
            }
            //agrego escalones
           
            
            foreach(var escalon in Escalones)
            {
                ListaEscalones.Add(escalon);

            }

            Grilla = new GrillaRegular();
            Grilla.create(Scene.Meshes.FindAll(m => !m.Name.Contains("Box")), Scene.BoundingBox);
            Grilla.createDebugMeshes();
        }

        public override void Reset()
        {
            checkpointReached = false;
            // Reset pj (Moverlo a la posicion inicial del escenario
            if (checkpointReached)
                Env.Personaje.Mesh.Position = new TGCVector3(836, 0, -41);
            else if (Env.Personaje.yaJugo)
            {
                Env.NuevaCamara = new TgcThirdPersonCamera(new TGCVector3(0, 0, 0), 20, -75, Env.Input);
                Env.Camara = Env.NuevaCamara;
            }
            else
                Env.Personaje.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
            Env.NuevaCamara = new TgcThirdPersonCamera(new TGCVector3(0, 0, 0), 20, -75, Env.Input);
            Env.Camara = Env.NuevaCamara;
        }

        public override void Render()
        {
            if (!checkpointReached && testAABBAABB(Env.Personaje.Mesh.BoundingBox, checkpoint))
                checkpointReached = true;
            preRender3D();
            RenderHUD();
            base.Render();
            foreach (var plano in ListaPlanos)
            {
                plano.Render();
            }
            Env.Personaje.Render();
            postRender3D();
            render2D(); 
        }

        public override void Dispose()
        {
            foreach(var plano in ListaPlanos)
            {
                plano.Dispose();
            }
            base.Dispose();
        }

        public override TgcBoundingAxisAlignBox ColisionConPiso(TgcBoundingAxisAlignBox boundingBox)
        {
            foreach (var p in ListaPlanos)
                if (Escenario.testAABBAABB(p.BoundingBox, boundingBox))
                    return p.BoundingBox;
            return null;
        }

        public override List<TgcMesh> listaColisionesConCamara()
        {
            return Scene.Meshes.FindAll(m => !ListaMeshesSinColision.Contains(m) && !ListaPisosResbalosos.Contains(m) && !ListaPozos.Contains(m));
        }
    }
}
