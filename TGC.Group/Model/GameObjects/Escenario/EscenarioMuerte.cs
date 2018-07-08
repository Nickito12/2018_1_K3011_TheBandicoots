using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.Text;

namespace TGC.Group.Model.GameObjects.Escenario
{
    class EscenarioMuerte : Escenario1
    {
        public TGCVector3 pos;
        public List<Button> buttons = new List<Button>();
        private TgcPickingRay pickingRay;
        public TgcText2D hasMuerto = new TgcText2D();
        public override void Init(GameModel _env)
        {
            base.Init(_env);
            Reset();

            hasMuerto.Text = "Has muerto, vuelve a intentarlo";
            hasMuerto.Align = TgcText2D.TextAlign.CENTER;
            hasMuerto.changeFont(new System.Drawing.Font("Comic Sans MS", 50, FontStyle.Bold));
            hasMuerto.Position = new Point(D3DDevice.Instance.Width/2 -700, D3DDevice.Instance.Height/2-200);
            hasMuerto.Color = Color.DarkRed;

            buttons.Add(new QuadButton(pos, 0, 10, "Reiniciar", () => {
                Env.Personaje.Reset();
                Env.CambiarEscenario("Escenario1");
            }, Color.DarkCyan));
            buttons.Add(new QuadButton(pos, 0, 15, "Menú Principal", () => {
                Env.CambiarEscenario("Menu");
            }, Color.DarkCyan));
            pickingRay = new TgcPickingRay(Env.Input);
            useShadows = false;
        }
        public override void Render()
        {
            if (Env.Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colision en base a posicion del mouse
                pickingRay.updateRay();

                foreach (var button in buttons)
                {
                    var aabb = button.AABB();
                    TGCVector3 collisionPoint;
                    var selected = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, aabb, out collisionPoint);
                    if (selected)
                    {
                        button.Click();
                        break;
                    }
                }
            }
            preRender3D();
            baseRender();
            hasMuerto.render();
            foreach (var plano in ListaPlanos)
            {
                plano.Render();
            }
            foreach (var button in buttons)
            {
                button.Render(Env);
            }
            postRender3D();
            render2D();
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
        }
        public override void Reset()
        {
            /*Env.Camara = new Core.Camara.TgcCamera();
            pos = new TGCVector3(0, 50, 0);
            Env.Camara.SetCamera(pos, new TGCVector3(0, pos.Y, pos.Z +1 ));*/
            Env.Camara = new Core.Camara.TgcCamera();
            pos = new TGCVector3(1200, 800, -300);
            Env.Camara.SetCamera(pos, new TGCVector3(pos.X, 0, pos.Z - 1));
        }

    }
}
