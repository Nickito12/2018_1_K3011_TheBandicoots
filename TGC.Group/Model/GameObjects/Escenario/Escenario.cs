using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Terrain;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Estructuras;
using System.Collections.Generic;
using TGC.Core.Sound;
using System.Drawing;
using Microsoft.DirectX.DirectInput;

namespace TGC.Group.Model.GameObjects
{
    public abstract class Escenario : GameObject
    {
        protected TgcSkyBox SkyBox;
        protected TgcScene Scene;
        protected TgcSceneLoader Loader;
        public GrillaRegular Grilla;
        public bool ShowGrilla = false;
        protected List<TgcMesh> ListaPozos = new List<TgcMesh>();
        protected List<TgcMesh> ListaPlataformas = new List<TgcMesh>();
        protected List<TgcMesh> ListaPisosResbalosos = new List<TgcMesh>();
        protected List<TgcMesh> ListaMeshesSinColision = new List<TgcMesh>();
        protected List<TgcMesh> MeshConMovimiento = new List<TgcMesh>();
        protected List<CajaEmpujable> ListaCajasEmpujables = new List<CajaEmpujable>();
        protected TgcMp3Player cancionPcpal = new TgcMp3Player();
        protected const float ROTATION_SPEED = 1f;
        protected List<Plataforma> Plataformas;
        protected List<TgcPlane> ListaPlanos = new List<TgcPlane>();
        protected Microsoft.DirectX.Direct3D.Effect EfectoRender3D;
        protected Microsoft.DirectX.Direct3D.Effect EfectoRender2D;
        protected Texture Text;
        public Surface pOldRT;
        public Surface pSurf;
        public VertexBuffer g_pVBV3D;
        public Surface g_pDepthStencil;
        public Surface pOldDS;

        public Escenario()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice , Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            //FullScreen Quad
            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);
            // inicializo el render target
            Text = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);
        }
        protected void AddMesh(string carpeta, string nombre, TGCVector3 pos, int rotation = 0, TGCVector3? scale = null)
        {
            scale = scale ?? new TGCVector3(1, 1, 1);
            var Mesh = Loader.loadSceneFromFile(Env.MediaDir + 
                "Meshes\\" + carpeta + "\\" + nombre + "\\" + 
                nombre + "-TgcScene.xml").Meshes[0];
            Mesh.AutoTransform = true; //hay que usar matrices segun la correcion del tp.
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
            //Dibujar bounding boxes de los mesh (Debugging)
            if (Env.Input.keyDown(Key.LeftControl) || Env.Input.keyDown(Key.RightControl))
            {
                foreach (TgcMesh mesh in Scene.Meshes)
                    mesh.BoundingBox.Render();
                foreach (TgcMesh mesh in ListaPozos)
                    mesh.BoundingBox.Render();
            }
            foreach (var plataforma in Plataformas)
            {
                plataforma.Mesh.Render();
            }
            foreach (var pozo in ListaPozos)
            {
                pozo.Render();
            }
            foreach (var caja in ListaCajasEmpujables)
            {
                caja.Mesh.Render();
            }
            Grilla.render(Env.Frustum, ShowGrilla);
        }
        public override void Dispose()
        {
            cancionPcpal.closeFile();
            SkyBox.Dispose();
            Scene.DisposeAll();
        }

        public static bool testAABBAABB(TgcBoundingAxisAlignBox a, TgcBoundingAxisAlignBox b)
        {
            return (a.PMin.X <= b.PMax.X && a.PMax.X >= b.PMin.X) &&
                   (a.PMin.Y <= b.PMax.Y && a.PMax.Y >= b.PMin.Y) &&
                   (a.PMin.Z <= b.PMax.Z && a.PMax.Z >= b.PMin.Z);
        }

        public static bool testAABBAABBXZ(TgcBoundingAxisAlignBox a, TgcBoundingAxisAlignBox b)
        {
            return (a.PMin.X <= b.PMax.X && a.PMax.X >= b.PMin.X) &&
                   (a.PMin.Z <= b.PMax.Z && a.PMax.Z >= b.PMin.Z);
        }

        public static bool testAABBAABBXZIn(TgcBoundingAxisAlignBox a, TgcBoundingAxisAlignBox b)
        {
            return (a.PMin.X <= b.PMax.X && a.PMin.X >= b.PMin.X && a.PMax.X >= b.PMin.X && a.PMax.X <= b.PMax.X) &&
                   (a.PMin.Z <= b.PMax.Z && a.PMin.Z >= b.PMin.Z && a.PMax.Z >= b.PMin.Z && a.PMax.Z <= b.PMax.Z);
        }

        public override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox)
        {
            // null => no hay colision
            TgcBoundingAxisAlignBox Colisionador = null;
            foreach (var Mesh in Scene.Meshes.FindAll(m => m.Enabled))
            {
                if (!ListaMeshesSinColision.Contains(Mesh) && Escenario.testAABBAABB(Mesh.BoundingBox, boundingBox))
                {
                    if (ListaPozos.Contains(Mesh))
                    {
                        Env.Personaje.SetTipoColisionActual(TiposColision.Pozo);
                        break;
                    }
                    else if (ListaPlataformas.Contains(Mesh))
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
                    else
                    {
                        Colisionador = Mesh.BoundingBox;
                    }
                    break;
                }
            }
            foreach (var caja in ListaCajasEmpujables)
            {
                var aabb = caja.Mesh.BoundingBox;
                if (!Escenario.testAABBAABB(aabb, boundingBox))
                    continue;
                var oldCajaPos = caja.Mesh.Position;
                caja.ColisionXZ(Env.Personaje);
                bool colisionDeCaja = false;
                foreach (var Mesh in Scene.Meshes.FindAll(m => m.Enabled))
                {
                    if (Escenario.testAABBAABB(aabb, Mesh.BoundingBox))
                    {
                        colisionDeCaja = true;
                        continue;
                    }
                }
                if (colisionDeCaja)
                {
                    Colisionador = aabb;
                    caja.Mesh.Position = oldCajaPos;
                    break;
                }
                foreach (var pozo in ListaPozos)
                {
                    if (Escenario.testAABBAABBXZIn(aabb, pozo.BoundingBox))
                    {
                        caja.caer();
                        continue;
                    }
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
        abstract public TgcBoundingAxisAlignBox ColisionConPiso(TgcBoundingAxisAlignBox boundingBox);

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
            foreach (var caja in ListaCajasEmpujables)
            {
                var colision = caja.ColisionY(Env.Personaje, Env.ElapsedTime);
                if (colision != null)
                    Colisionador = colision;
            }
            if (Colisionador != null)
                return Colisionador;
            var piso = ColisionConPiso(boundingBox);
            if (piso != null)
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
                if (!agujero)
                    Colisionador = piso;
            }
            return Colisionador;
        }
        public void preRender3D()
        {

            Env.limpiarTexturas();
            var device = D3DDevice.Instance.Device;

            //Cargar variables de shader

            // dibujo la escena una textura
            EfectoRender3D.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            pOldRT = device.GetRenderTarget(0);
            pSurf = Text.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            device.BeginScene();
        }
        public void postRender3D()
        {
            D3DDevice.Instance.Device.EndScene();
            //TextureLoader.Save(Env.ShadersDir + "render_target.bmp", ImageFileFormat.Bmp, Text);
            pSurf.Dispose();
        }
        public void render2D()
        {
            var device = D3DDevice.Instance.Device;
            device.DepthStencilSurface = pOldDS;
            device.SetRenderTarget(0, pOldRT);

            device.BeginScene();

            EfectoRender2D.Technique = "Sharpen";
            if(Env.Input.keyDown(Key.F6))
                EfectoRender2D.Technique = "Copy";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            EfectoRender2D.SetValue("g_RenderTarget", Text);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            EfectoRender2D.Begin(FX.None);
            EfectoRender2D.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            EfectoRender2D.EndPass();
            EfectoRender2D.End();
            device.EndScene();

            device.BeginScene();
            Env.RenderizaAxis();
            Env.RenderizaFPS();
            device.EndScene();

            device.Present();
        }
        public override void Update()
        {
            if (Env.ElapsedTime > 10000)
                return;
            if (Env.Personaje.Position().Y <= -100)
                Env.Personaje.Position(new TGCVector3(0, 1, 0));
            ShowGrilla = Env.Input.keyDown(Key.F3);
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
    }
}
