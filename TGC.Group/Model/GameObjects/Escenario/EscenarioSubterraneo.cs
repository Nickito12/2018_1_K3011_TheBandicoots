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
    class EscenarioSubterraneo : Escenario
    {
        private TgcPlane PisoSelva;
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



            // Cargar escena
            Loader = new TgcSceneLoader();
            Scene = Loader.loadSceneFromFile(Env.MediaDir + "\\" + "Escenario1\\escenarioSubterraneo-TgcScene.xml");


            // Setear cancion
            cancionPcpal.FileName = Env.MediaDir + "\\Sound\\crash.mp3";

            //piso
            var PisoSelvaTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Env.MediaDir + "pasto.jpg");
            PisoSelva = new TgcPlane(new TGCVector3(-200f, 0f, -100f), new TGCVector3(50, 5f, 200), TgcPlane.Orientations.XZplane, PisoSelvaTexture, 15, 15);

        }

        public override void Reset()
        {
            
            Env.Personaje.Move(new TGCVector3(10, 0, 0), new TGCVector3(10, 0, 0));
            Env.NuevaCamara = new TgcThirdPersonCamera(new TGCVector3(10, 0, 0), 20, -75, Env.Input);
            Env.Camara = Env.NuevaCamara;
        }

        public override void Render()
        {

            preRender3D();
            RenderHUD();
            //base.Render();

            Env.Personaje.Render();
            postRender3D();
            render2D();
        }
        public override void Update()
        {
            if (Env.ElapsedTime > 10000)
                return;
            if (Env.Personaje.Position().Y <= -500)
            {
                Env.Personaje.vidas--;

                if (Env.Personaje.vidas <= 0)
                    Env.CambiarEscenario(1);
                else

                    Reset();
            }
            ShowGrilla = Env.Input.keyDown(Microsoft.DirectX.DirectInput.Key.F3);

            Env.NuevaCamara.UpdateCamera(this);
            if (cancionPcpal.getStatus() != TgcMp3Player.States.Playing)
            {
                cancionPcpal.closeFile();
                cancionPcpal.play(true);
            }
            Env.Personaje.Update();
        }
        public override void Dispose()
        {

            base.Dispose();
        }

        public override TgcBoundingAxisAlignBox ColisionConPiso(TgcBoundingAxisAlignBox boundingBox)
        {
            
                 if (Escenario.testAABBAABB(PisoSelva.BoundingBox, boundingBox))
                     return PisoSelva.BoundingBox; 
            return null;
        }

        public override List<TgcMesh> listaColisionesConCamara()
        {
            return Scene.Meshes.FindAll(m => !ListaMeshesSinColision.Contains(m) && !ListaPisosResbalosos.Contains(m) && !ListaPozos.Contains(m));
        }
        public override TgcBoundingAxisAlignBox ColisionXZ(TgcBoundingAxisAlignBox boundingBox)
        {
            return null;
        }
        public override TgcBoundingAxisAlignBox ColisionY(TgcBoundingAxisAlignBox boundingBox)
        {
            TgcBoundingAxisAlignBox Colisionador = null;
            var piso = ColisionConPiso(boundingBox);
            Colisionador = piso;
           return Colisionador;
        }






        }
}

