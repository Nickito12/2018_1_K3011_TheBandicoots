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
using System;
using TGC.Core.Shaders;
using Microsoft.DirectX;
using TGC.Core.SkeletalAnimation;

namespace TGC.Group.Model.GameObjects.Escenario
{
    public abstract class Escenario
    {
        public GameModel Env;
        protected TgcMp3Player cancionPcpal = new TgcMp3Player();
        public GrillaRegular Grilla = new GrillaRegular();
        protected Microsoft.DirectX.Direct3D.Effect EfectoRender3D;
        protected Microsoft.DirectX.Direct3D.Effect EfectoRender2D;
        protected Texture Text;
        public Surface pOldRT;
        public Surface pOldDS;
        public Surface sharpenSurf;
        public VertexBuffer sharpenVBV3D;
        public VertexBuffer outrunVBV3D;
        public Surface sharpenDepthStencil;
        public Texture texturaVida;
        protected Microsoft.DirectX.Direct3D.Effect shadowEffect;
        protected Microsoft.DirectX.Direct3D.Effect outrunEffect;
        protected Microsoft.DirectX.Direct3D.Effect skeletalShadowEffect;
        protected Microsoft.DirectX.Direct3D.Effect shaderArbustos;
        protected Microsoft.DirectX.Direct3D.Effect shaderLiquidos;
        protected Microsoft.DirectX.Direct3D.Effect shaderAceites;
        private readonly int SHADOWMAP_SIZE = 1024;
        protected TGCVector3 g_LightDir; // direccion de la luz
        protected TGCVector3 g_LightPos; // posicion de la luz
        protected TGCMatrix g_LightView; // matriz de view del light
        protected TGCMatrix g_mShadowProj; // Projection matrix for shadow map
        private Surface shadowDepthStencil; // Depth-stencil buffer for rendering to shadow map
        private Surface g_pOutrunDepthStencil; // Depth-stencil buffer for rendering the Outrun
        protected bool useShadows = true;
        private bool doingShadowRender = false;
        private Texture g_pShadowMap; // Texture to which the shadow map is rendered
        private Texture g_pOutrunRenderTarget, g_pOutrunRenderTarget2, g_pOutrunRenderTarget3, g_pOutrunRenderTarget4, g_pOutrunRenderTarget5;

        public abstract void Dispose();
        public abstract void Init(GameModel _env);

        public Escenario()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            sharpenVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            //FullScreen Quad
            CustomVertex.PositionTextured[] verticesSharpen =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            sharpenVBV3D.SetData(verticesSharpen, 0, LockFlags.None);
            CustomVertex.PositionTextured[] verticesOutrun =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            outrunVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            outrunVBV3D.SetData(verticesOutrun, 0, LockFlags.None);
            // inicializo el render target
            Text = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            sharpenDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);// Creo el shadowmap.


            g_pOutrunDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);
            // inicializo el render target del outrun
            g_pOutrunRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pOutrunRenderTarget2 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pOutrunRenderTarget3 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pOutrunRenderTarget4 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pOutrunRenderTarget5 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            // Format.R32F
            // Format.X8R8G8B8
            g_pShadowMap = new Texture(D3DDevice.Instance.Device, SHADOWMAP_SIZE, SHADOWMAP_SIZE, 1, Usage.RenderTarget, Format.R32F, Pool.Default);

            // tengo que crear un stencilbuffer para el shadowmap manualmente
            // para asegurarme que tenga la el mismo tamano que el shadowmap, y que no tenga
            // multisample, etc etc.
            shadowDepthStencil = D3DDevice.Instance.Device.CreateDepthStencilSurface(SHADOWMAP_SIZE, SHADOWMAP_SIZE, DepthFormat.D24S8, MultiSampleType.None, 0, true);
            // por ultimo necesito una matriz de proyeccion para el shadowmap, ya
            // que voy a dibujar desde el pto de vista de la luz.
            // El angulo tiene que ser mayor a 45 para que la sombra no falle en los extremos del cono de luz
            // de hecho, un valor mayor a 90 todavia es mejor, porque hasta con 90 grados es muy dificil
            // lograr que los objetos del borde generen sombras
            var aspectRatio = D3DDevice.Instance.AspectRatio;
            g_mShadowProj = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(80), aspectRatio, 50, 5000);
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

        public abstract TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox);
        public abstract TgcBoundingAxisAlignBox ColisionConPiso(TgcBoundingAxisAlignBox boundingBox);
        public abstract TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox);
        public abstract void RenderScene();
        public virtual void RenderRealScene()
        {
            RenderScene();
        }

        float ftime = 0;
        public virtual void Render()
        {
            preRender3D();
            RenderRealScene();
            postRender3D();
            render2D();
        }
        public void RenderShadowMap()
        {
            // Calculo la matriz de view de la luz
            shadowEffect.SetValue("g_vLightPos", new Vector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
            shadowEffect.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
            skeletalShadowEffect.SetValue("g_vLightPos", new Vector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
            skeletalShadowEffect.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
            g_LightView = TGCMatrix.LookAtLH(g_LightPos, g_LightPos + g_LightDir, new TGCVector3(0, 0, 1));

            // inicializacion standard:
            shadowEffect.SetValue("g_mProjLight", g_mShadowProj.ToMatrix());
            shadowEffect.SetValue("g_mViewLightProj", (g_LightView * g_mShadowProj).ToMatrix());
            skeletalShadowEffect.SetValue("g_mProjLight", g_mShadowProj.ToMatrix());
            skeletalShadowEffect.SetValue("g_mViewLightProj", (g_LightView * g_mShadowProj).ToMatrix());


            // Primero genero el shadow map, para ello dibujo desde el pto de vista de luz
            // a una textura, con el VS y PS que generan un mapa de profundidades.
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            var pShadowSurf = g_pShadowMap.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pShadowSurf);
            var pOldDS = D3DDevice.Instance.Device.DepthStencilSurface;
            D3DDevice.Instance.Device.DepthStencilSurface = shadowDepthStencil;
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();

            // Hago el render de la escena pp dicha
            shadowEffect.SetValue("g_txShadow", g_pShadowMap);
            skeletalShadowEffect.SetValue("g_txShadow", g_pShadowMap);
            // RENDER
            doingShadowRender = true;
            RenderScene(); 


            // Termino
            D3DDevice.Instance.Device.EndScene();
            //TextureLoader.Save("shadowmap.bmp", ImageFileFormat.Bmp, g_pShadowMap);
            // restuaro el render target y el stencil
            D3DDevice.Instance.Device.DepthStencilSurface = pOldDS;
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
        }
        public void preRender3D()
        {
            Env.limpiarTexturas();
            if(useShadows)
                RenderShadowMap();
            var device = D3DDevice.Instance.Device;
            // guardo el Render target anterior y seteo la textura como render target
            pOldRT = device.GetRenderTarget(0);
            sharpenSurf = Text.GetSurfaceLevel(0);
            device.SetRenderTarget(0, sharpenSurf);
            pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = sharpenDepthStencil;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            device.BeginScene();
            doingShadowRender = false;
        }
        public void postRender3D()
        {
            D3DDevice.Instance.Device.EndScene();
            //TextureLoader.Save(Env.ShadersDir + "render_target.bmp", ImageFileFormat.Bmp, Text);
            sharpenSurf.Dispose();
        }
        public void RenderObject(TgcMesh x)
        {
            var t = x.Technique;
            if (useShadows)
            {
                x.Effect = shadowEffect;
                x.Technique = doingShadowRender ? "RenderShadow" : "RenderScene";
            }
           
                x.Render();
                x.Technique = t;
            
        }
        public void RenderObject(TgcSkeletalMesh x)
        {
            var e = x.Effect;
            var t = x.Technique;
            if (useShadows)
            {
                x.Effect = shadowEffect;
                x.Effect = skeletalShadowEffect;
                x.Technique = doingShadowRender ? "RenderShadow" : "RenderScene";
            }
            x.Render();
            x.Technique = t;
            x.Effect = e;
            x.Render();
        }
        public void RenderObject(TGCBox x)
        {
            var t = x.Technique;
            if (useShadows)
            {
                x.Effect = shadowEffect;
                x.Technique = doingShadowRender ? "RenderShadow" : "RenderScene";
            }
            x.Render();
            x.Technique = t;
        }
        public void RenderObject(TgcPlane x)
        {
            var t = x.Technique;
            if (useShadows)
            {
                x.Effect = shadowEffect;
                x.Technique = doingShadowRender ? "RenderShadow" : "RenderScene";
            }
            x.Render();
            x.Technique = t;
        }
        public void render2D()
        {
            var device = D3DDevice.Instance.Device;
            device.DepthStencilSurface = pOldDS;

            var pSurf = g_pOutrunRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.DepthStencilSurface = g_pOutrunDepthStencil;

            var oldFillMode = D3DDevice.Instance.Device.RenderState.FillMode;
            D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
            device.BeginScene();

            EfectoRender2D.Technique = "Sharpen";
            if(Env.Input.keyDown(Key.F6))
                EfectoRender2D.Technique = "Copy";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, sharpenVBV3D, 0);
            EfectoRender2D.SetValue("g_RenderTarget", Text);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            EfectoRender2D.Begin(FX.None);
            EfectoRender2D.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            EfectoRender2D.EndPass();
            EfectoRender2D.End();
            device.EndScene();


            D3DDevice.Instance.Device.RenderState.FillMode = oldFillMode;

            // Ultima pasada vertical va sobre la pantalla pp dicha
            device.DepthStencilSurface = pOldDS;
            device.SetRenderTarget(0, pOldRT);
            device.BeginScene();

            outrunEffect.Technique = "FrameMotionBlur";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, outrunVBV3D, 0);
            outrunEffect.SetValue("g_RenderTarget", g_pOutrunRenderTarget);
            outrunEffect.SetValue("g_RenderTarget2", g_pOutrunRenderTarget2);
            outrunEffect.SetValue("g_RenderTarget3", g_pOutrunRenderTarget3);
            outrunEffect.SetValue("g_RenderTarget4", g_pOutrunRenderTarget4);
            outrunEffect.SetValue("g_RenderTarget5", g_pOutrunRenderTarget5);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            outrunEffect.Begin(FX.None);
            outrunEffect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            outrunEffect.EndPass();
            outrunEffect.End();

            Env.RenderizaAxis();
            Env.RenderizaFPS();
            device.EndScene();
            device.Present();

            ftime += Env.ElapsedTime;
            if (ftime > 0.03f)
            {
                ftime = 0;
                var aux = g_pOutrunRenderTarget5;
                g_pOutrunRenderTarget5 = g_pOutrunRenderTarget4;
                g_pOutrunRenderTarget4 = g_pOutrunRenderTarget3;
                g_pOutrunRenderTarget3 = g_pOutrunRenderTarget2;
                g_pOutrunRenderTarget2 = g_pOutrunRenderTarget;
                g_pOutrunRenderTarget = aux;
            }
        }
        public abstract void Update();
        public virtual List<TgcBoundingAxisAlignBox> listaColisionesConCamara()
        {
            return new List<TgcBoundingAxisAlignBox>();
        }
        public virtual void Reset()
        {
            Env.Personaje.Move(new TGCVector3(0, 1, 0), new TGCVector3(0, 1, 0));
        }
        public void RenderHUD()
        {
            
            var d3dDevice = D3DDevice.Instance.Device;
            //TgcTexture textura;
            var sprite = new Sprite(d3dDevice);
            sprite.Begin(SpriteFlags.AlphaBlend);
            for (var i = 0; i < Env.Personaje.vidas; i++)
                sprite.Draw2D(texturaVida, Rectangle.Empty, new SizeF(32, 32), new PointF(20+32*i, 650), Color.Red);
            sprite.End();
        }
    }
}
