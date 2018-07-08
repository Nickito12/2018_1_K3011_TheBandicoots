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
using TGC.Core.Shaders;
using TGC.Core.Text;
using System.Drawing;

namespace TGC.Group.Model.GameObjects.Escenario
{
    public class Escenario1 : EscenarioManual
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
        private TgcPlane PisoCastillo1;
        private TgcPlane PisoCastillo2;
        private TgcPlane PisoCastillo3;
        private TgcPlane PisoCastillo4;
        private TgcPlane PisoCastillo5;
        private TgcPlane PisoCastillo6;
        private TgcPlane PisoCastillo7;
        private TgcPlane PisoCastillo8;
        private TgcPlane PisoCastillo9;
        private TgcPlane PisoCastillo10;
        private TgcPlane PisoCastillo11;
        private TgcPlane PisoAcido;
        public Texture texturaLogo;
        public TgcText2D TextoLogo = new TgcText2D();

        // Epsilon, pos, dir
        private List<Tuple<float, TGCVector3, TGCVector3>> Lights = new List<Tuple<float, TGCVector3, TGCVector3>>(); // Usamos solo la mas cercana

        private TgcBoundingAxisAlignBox checkpoint = new TgcBoundingAxisAlignBox(
             new TGCVector3(799, 0, -97), new TGCVector3(870, 1000, -4));
        private bool checkpointReached = false;
        private TgcBoundingAxisAlignBox checkpoint2 = new TgcBoundingAxisAlignBox(
             new TGCVector3(1000, -200 , -500), new TGCVector3(1200, 1200, -200));
        private bool checkpointReached2 = false;
        private TgcBoundingAxisAlignBox final = new TgcBoundingAxisAlignBox(new TGCVector3(1700, -180, -650), new TGCVector3(1800, -160, -640));
        private bool finalReached = false;
       

        public override void Init(GameModel _env)
        {
            //useShadows = false;
            Env = _env;
            string compilationErrors;
            var d3dDevice = D3DDevice.Instance.Device;
            shadowEffect = TgcShaders.loadEffect(Env.ShadersDir + "ShadowMap.fx");
            var dir = new TGCVector3(0, -1, 0);
            dir.Normalize();
            Lights.Add(new Tuple<float, TGCVector3, TGCVector3>(0.05f, new TGCVector3(0, 80, 100), dir));
            dir = new TGCVector3(1, -2, 0);
            dir.Normalize();
            Lights.Add(new Tuple<float, TGCVector3, TGCVector3>(0.05f, new TGCVector3(220, 80, 390), dir));
            Lights.Add(new Tuple<float, TGCVector3, TGCVector3>(0.05f, new TGCVector3(1370, 80, 57), new TGCVector3(0, -1, 0)));
            Lights.Add(new Tuple<float, TGCVector3, TGCVector3>(0.05f, new TGCVector3(842, 80, -327), new TGCVector3(0, -1, 0)));
            Lights.Add(new Tuple<float, TGCVector3, TGCVector3>(0.05f, new TGCVector3(1050, 80, -110), new TGCVector3(0, -1, 0)));
            g_LightPos = new TGCVector3(140, 40, 390);
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
                texturaLogo = TextureLoader.FromFile(d3dDevice, Env.MediaDir + "\\Menu\\LogoTGC.png");
            }
            catch
            {
                throw new Exception(string.Format("Error at loading texture: {0}", Env.MediaDir + "\\Menu\\heart.png"));
            }

            TextoLogo.Align = TgcText2D.TextAlign.LEFT;
            TextoLogo.changeFont(new System.Drawing.Font("Comic Sans MS", 24, FontStyle.Bold));
            TextoLogo.Position = new Point(D3DDevice.Instance.Width - 70, D3DDevice.Instance.Height - 65);
            TextoLogo.Color = Color.White;


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

            var PisoCastilloTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "rockfloor.jpg");
            PisoCastillo1 = new TgcPlane(new TGCVector3(791f, 21f, -155.4f), new TGCVector3(91f, 5f, 27.7f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo1);
            PisoCastillo2 = new TgcPlane(new TGCVector3(776f, 21f, -604f), new TGCVector3(218.2f, 5f, 331.3f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo2);
            PisoCastillo3 = new TgcPlane(new TGCVector3(1088.4f, 21f, -607.2f), new TGCVector3(158.8f, 5f, 118f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo3);
            PisoCastillo4 = new TgcPlane(new TGCVector3(1436.2f, 21f, -611.2f), new TGCVector3(270.4f, 5f, 331.4f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo4);
            PisoCastillo5 = new TgcPlane(new TGCVector3(1614.6f, 21f, -279.9f), new TGCVector3(87.65f, 5f, 97.9f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo5);
            PisoCastillo6 = new TgcPlane(new TGCVector3(1538.2f, 21f, -182.5f), new TGCVector3(169f, 5f, 58.6f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo6);
            PisoCastillo7 = new TgcPlane(new TGCVector3(1485f, 21f, -75.5f), new TGCVector3(228f, 5f, 192.2f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo7);
            PisoCastillo8 = new TgcPlane(new TGCVector3(1540f, 21f, -124.36f), new TGCVector3(58.35f, 5f, 49.16f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo8);
            PisoCastillo9 = new TgcPlane(new TGCVector3(986f, 21f, -108.7603f), new TGCVector3(134.15f, 5f, 223.6003f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo9);
            PisoCastillo10 = new TgcPlane(new TGCVector3(951f, 21f, -225f), new TGCVector3(130f, 5f, 116.2397f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo10);
            PisoCastillo11 = new TgcPlane(new TGCVector3(1080f, 21f, -185f), new TGCVector3(50f, 5f, 80f), TgcPlane.Orientations.XZplane, PisoCastilloTexture, 15, 15);
            ListaPlanos.Add(PisoCastillo11);
            var Acido = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "slime7.jpg");
            PisoAcido = new TgcPlane(new TGCVector3(1427f, -228.4502f, -1000f), new TGCVector3(400f, 5f, 450f), TgcPlane.Orientations.XZplane, Acido, 15, 15);
            ListaPlanos.Add(PisoAcido);


            //Crear SkyBox
            CreateSkyBox(TGCVector3.Empty, new TGCVector3(10000, 10000, 10000), "SkyBox1");

            // Cargar escena
            Loader = new TgcSceneLoader();
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\escenarioConLogos-TgcScene.xml");

            Reset();
            ///////////agrego pisos subterraneos

            ListaPisosSubterraneos = Scene.Meshes.FindAll(m => m.Name.Contains("Floor"));
            

            foreach (var piso in ListaPisosSubterraneos)
            {
                Scene.Meshes.Remove(piso);
            }

            

            // Paredes
            ListaParedes = Scene.Meshes.FindAll(m => m.Name.Contains("ParedCastillo"));
            TgcMesh paredSinBB = Scene.Meshes.Find(m => m.Name.Contains("ParedCastillo441"));
            foreach (var m in ListaParedes) {
                m.AutoTransform = false;
                var p = (m.BoundingBox.PMax + m.BoundingBox.PMin) * 0.5f;
                var t = new TGCVector3(0, 20.8f, 0);
                m.Transform = TGCMatrix.Translation(-1 * p) * TGCMatrix.Translation(t + p);
                m.BoundingBox.scaleTranslate(new TGCVector3(0, 21, 0), new TGCVector3(1, 1, 1));
            }
            ListaParedes.Remove(paredSinBB); //elimino la pared que no necesita agrandar su BB
            foreach(var m in ListaParedes)
            {
                m.BoundingBox = new TgcBoundingAxisAlignBox(m.BoundingBox.PMin - new TGCVector3(5, 0, 5), m.BoundingBox.PMax + new TGCVector3(5, 0, 5));
            }

            // Paredes de caida
            ListaParedesCaida = Scene.Meshes.FindAll(m => m.Name.Contains("Plane"));
            foreach (var m in ListaParedesCaida)
            {
                var agregado = new TGCVector3(3, -0.8f, 3);
                m.BoundingBox = new TgcBoundingAxisAlignBox(m.BoundingBox.PMin - agregado, m.BoundingBox.PMax + agregado);
            }

            // Pozos
            ListaPozos = Scene.Meshes.FindAll(m => m.Name.Contains("Pozo"));
            foreach (var mesh in ListaPozos)
            {
                Scene.Meshes.Remove(mesh);
            }

            // Logos
            ListaLogos = Scene.Meshes.FindAll(m => m.Name.Contains("LogoTGC"));
            CantLogos = ListaLogos.Count;

            // Alargar algunas AABB
            var r = new Random();
            foreach (var mesh in Scene.Meshes.FindAll(m => m.Name.Contains("Arbusto"))) {
                mesh.BoundingBox.scaleTranslate(new TGCVector3(0, 0, 0), new TGCVector3(1, 10, 1));
                mesh.AutoTransform = false;
                var ang = r.Next(0, 360);
                var p = (mesh.BoundingBox.PMax + mesh.BoundingBox.PMin)*0.5f;
                var s = new TGCVector3(((float)r.Next(90, 110))/100f, 1, ((float)r.Next(90, 110))/100f);
                var rango = 6;
                var t = new TGCVector3(((float)r.Next(-rango*100, rango * 100)) / 100f, 0, ((float)r.Next(-rango * 100, rango * 100)) / 100f);
                mesh.Transform = TGCMatrix.Translation(-1 * p) * TGCMatrix.Scaling(s) * TGCMatrix.RotationY(ang)  * TGCMatrix.Translation(t+p);
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
           // TgcMesh PlataformaY;
            List<TgcMesh> ListaPlataformaZ = new List<TgcMesh>();
            List<TgcMesh> ListaMovibles = new List<TgcMesh>();
            List<TgcMesh> Escalones = new List<TgcMesh>();

            ListaPlataformaEstatica = Scene.Meshes.FindAll(m => m.Name.Contains("Box_0"));
            ListaPlataformaX = Scene.Meshes.FindAll(m => m.Name.Contains("Box_1"));
            ListaPlataformaZ = Scene.Meshes.FindAll(m => m.Name.Contains("Box_2"));
            PlataformaY = Scene.Meshes.Find(Mesh => Mesh.Name.Contains("SubeBaja"));
            ListaMovibles = Scene.Meshes.FindAll(m => m.Name.Contains("Box_M"));
            Escalones = Scene.Meshes.FindAll(m => m.Name.Contains("Escalon"));

            //agrego plataforma que se mueven en X
            foreach (var p in ListaPlataformaX)
            {
                Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, 0f), 25f, true, false,12f, false));
            }

            //agrego plataforma que se mueven en Z
            foreach (var p in ListaPlataformaZ)
            {
                //-20f es para que este centrado en el camino
                if (p.Name == "Box_202" || p.Name == "Box_204")
                {
                    Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, -20f), 25f, false,false, 12f, true));
                }
                else
                {
                    Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, -20f), 25f, false,false, 12f, false));
                }
            }

            Plataformas.Add(new PlataformaLineal(PlataformaY, new TGCVector3(0f, -90f, 0f), 90f, false, true, 15f, true)); 

            //agrego objetos moviles
            foreach (var p in ListaMovibles)
            {
                Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, 0f), 0f, false, false,0f, false));
            }

            //agrego objetos estaticos
            foreach (var p in ListaPlataformaEstatica)
            {
                Plataformas.Add(new PlataformaLineal(p, new TGCVector3(0f, 0f, 0f), 0f, false,false, 0f, false));
            }

            //se agregan plataformas giratorias
            var meshGiratorio = Plataformas[0].Mesh.clone("pGira");
            Plataformas.Add(new PlataformaGiratoria(15, meshGiratorio, new TGCVector3(260f, 0f, 275f), 5f));
            Scene.Meshes.Add(meshGiratorio);
            var meshGiratorio2 = ListaPlataformaZ[4].clone("pGira2");
            Plataformas.Add(new PlataformaGiratoria(28, meshGiratorio2, new TGCVector3(75f, 0f, -20f), 5f));
            Scene.Meshes.Add(meshGiratorio2);
            var meshGiratorio3 = ListaPlataformaZ[4].clone("pGira3");
            Plataformas.Add(new PlataformaGiratoria(28, meshGiratorio3, new TGCVector3(-135f, 0f, 575f), 5f));
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

            Grilla.create(Scene.Meshes.FindAll(m => !m.Name.Contains("Box")), Scene.BoundingBox);
            Grilla.createDebugMeshes();
        }

        public override void Reset()
        {
            //checkpointReached = false;
            // Reset pj (Moverlo a la posicion inicial del escenario
            if (checkpointReached)
                Env.Personaje.Mesh.Position = new TGCVector3(836, 0, -41);
            else if (checkpointReached2)
            {
               
                Env.Personaje.Mesh.Position = new TGCVector3((float)1094.411,(float) -165.0148,(float) -210.6129);
            }
            else if (Env.Personaje.yaJugo)
            {
                Env.NuevaCamara = new TgcThirdPersonCamera(new TGCVector3(0, 0, 0), 20, -75, Env.Input);
                Env.Camara = Env.NuevaCamara;
            }
            else
                Env.Personaje.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
            if (Env.Personaje.vidas == 3)
            {
                ListaLogos.Clear();
                ListaLogos = Scene.Meshes.FindAll(m => m.Name.Contains("LogoTGC"));
                foreach (var logo in ListaLogos)
                {
                    logo.Enabled = true; //esto es para que se renderice
                }
                CantLogos = ListaLogos.Count;
            }
            Env.NuevaCamara = new TgcThirdPersonCamera(new TGCVector3(0, 0, 0), 20, -75, Env.Input);
            Env.Camara = Env.NuevaCamara;
        }

        public override void RenderRealScene()
        {
            if (!checkpointReached && testAABBAABB(Env.Personaje.Mesh.BoundingBox, checkpoint))
            {
                checkpointReached = true;
            }
            else if (!checkpointReached2 && testAABBAABB(Env.Personaje.Mesh.BoundingBox, checkpoint2))
            {
                checkpointReached2 = true;
            }
            if (!finalReached && testAABBAABB(Env.Personaje.Mesh.BoundingBox, final))
                Env.CambiarEscenario("Victoria"); 
            RenderHUD();
            RenderHUDLogos();
            Env.Personaje.RenderHUD();
            realBaseRender();
            RenderScene();
        }
        public Tuple<float, TGCVector3, TGCVector3> ClosestLight()
        {
            Tuple<float, TGCVector3, TGCVector3> min = Lights[0];
            var pos = new TGCVector3(Env.Personaje.Mesh.Position.X, 0, Env.Personaje.Mesh.Position.Z);
            float d = float.MaxValue;
            foreach(var l in Lights)
            {
                var lPos = l.Item2;
                lPos -= new TGCVector3(0, lPos.Y, 0);
                var dist = TGCVector3.Length(pos - l.Item2);
                if (dist < d) {
                    d = dist;
                    min = l;
                }
            }
            return min;
        }
        float rotacionLogos = 0f;
        public override void RenderScene()
        {
            var l = ClosestLight();
            shadowEffect.SetValue("EPSILON", l.Item1);
            g_LightPos = l.Item2;
            g_LightDir = l.Item3;
            baseRender();
            foreach (var plano in ListaPlanos)
            {
                RenderObject(plano);
            }
            foreach(var piso in ListaPisosSubterraneos)
            {             
                RenderObject(piso);
            }
            rotacionLogos += 1f * Env.ElapsedTime;
            rotacionLogos = rotacionLogos > 360f ? 0 : rotacionLogos;
            foreach (var logo in ListaLogos)
            {
               
                    var p = (logo.BoundingBox.PMax + logo.BoundingBox.PMin) * 0.5f;
                    logo.AutoTransform = false;
                    logo.Transform = TGCMatrix.Translation(-p)
                                    * TGCMatrix.RotationYawPitchRoll(rotacionLogos, 0, 0)
                                    * TGCMatrix.Translation(p);
               

            }

            TextoLogo.Text = CantLogos.ToString();
            TextoLogo.render();
            Env.Personaje.Render(this);
        }

        public override void Dispose()
        {
            foreach(var plano in ListaPlanos)
            {
                plano.Dispose();
            }
            foreach(var piso in ListaPisosSubterraneos)
            {
                piso.Dispose();
            }
            TextoLogo.Dispose();
            base.Dispose();
        }

        public override TgcBoundingAxisAlignBox ColisionConPiso(TgcBoundingAxisAlignBox boundingBox)
        {
            foreach (var p in ListaPlanos)
                if (EscenarioManual.testAABBAABB(p.BoundingBox, boundingBox))
                    return p.BoundingBox;
            

            foreach(var meshPiso in ListaPisosSubterraneos)
            {
                if (EscenarioManual.testAABBAABB(meshPiso.BoundingBox, boundingBox))
                    return meshPiso.BoundingBox;
            }
            
            return null;
        }

        public override List<TgcBoundingAxisAlignBox> listaColisionesConCamara()
        { 
            return Scene.Meshes.FindAll(m => !ListaPisosSubterraneos.Contains(m) && !ListaMeshesSinColision.Contains(m) && !ListaEscalones.Contains(m) && !ListaPisosResbalosos.Contains(m) && !ListaPozos.Contains(m) && !ListaLogos.Contains(m)).
                ConvertAll((TgcMesh x) => x.BoundingBox);
        }

        public void RenderHUDLogos()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            //TgcTexture textura;
            var sprite = new Sprite(d3dDevice);
            sprite.Begin(SpriteFlags.AlphaBlend);
            sprite.Draw2D(texturaLogo, Rectangle.Empty, new SizeF(80, 80), new PointF(D3DDevice.Instance.Width - 155, D3DDevice.Instance.Height - 90), Color.Blue);
            sprite.End();
        }

        
    }
}
