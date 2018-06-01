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

namespace TGC.Group.Model.GameObjects.Escenario
{
    public abstract class Escenario : GameObject
    {
        protected Microsoft.DirectX.Direct3D.Effect EfectoRender2D;
        protected Texture Text;
        public Surface pOldRT;
        public Surface pSurf;
        public VertexBuffer g_pVBV3D;
        public Surface g_pDepthStencil;
        public Surface pOldDS;
        public Texture texturaVida;
        protected TgcMp3Player cancionPcpal = new TgcMp3Player();
        public GrillaRegular Grilla = new GrillaRegular();

        public abstract override void Render();
        public abstract override void Dispose();

        public Escenario()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
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

        public abstract override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox);
        public abstract TgcBoundingAxisAlignBox ColisionConPiso(TgcBoundingAxisAlignBox boundingBox);

        public abstract override TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox);
        public void preRender3D()
        {

            Env.limpiarTexturas();
            var device = D3DDevice.Instance.Device;
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

            var oldFillMode = D3DDevice.Instance.Device.RenderState.FillMode;
            D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
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
            D3DDevice.Instance.Device.RenderState.FillMode = oldFillMode;
        }
        public abstract override void Update();
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
                sprite.Draw2D(texturaVida, Rectangle.Empty, new SizeF(32, 32), new PointF(10+32*i, 10), Color.Red);
            sprite.End();
        }
    }
}
