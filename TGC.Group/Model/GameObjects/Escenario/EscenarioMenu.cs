using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.Sound;

delegate void ClickDelegate();

namespace TGC.Group.Model.GameObjects.Escenario
{
    class EscenarioMenu : Escenario1
    {
        public TGCVector3 pos;
        public Dictionary<string, Tuple<TGCQuad, ClickDelegate, int, int, TgcBoundingAxisAlignBox>> buttons = 
            new Dictionary<string, Tuple<TGCQuad, ClickDelegate, int, int, TgcBoundingAxisAlignBox>>();
        private TgcPickingRay pickingRay;

        // Masomenos +/-80 en x y +/-40 en y
        private void addButton(float X, float Y, string text, ClickDelegate onClick, Color color, float sizeX=15, float sizeY=8)
        {
            var q = new TGC.Core.Geometry.TGCQuad();
            q.Center = new TGCVector3(pos.X+X, pos.Y - 50, pos.Z+Y);
            q.Size = new TGCVector2(sizeX, sizeY);
            q.Normal = new TGCVector3(0, 1, 0);
            q.Color = color;
            q.updateValues();
            var s = q.Size * 0.5f;
            var aabb = new TgcBoundingAxisAlignBox(q.Center - new TGCVector3(s.X, 0, s.Y), q.Center + new TGCVector3(s.X, 0, s.Y), q.Center, new TGCVector3(1, 1, 1));
            buttons[text] = new Tuple<TGCQuad, ClickDelegate, int, int, TgcBoundingAxisAlignBox>(q, onClick, (int)(X*650/20 + 650), (int)(Y*350/20 + 350), aabb);
        }
        public override void Init(GameModel _env)
        {
            base.Init(_env);
            Env.Camara = new Core.Camara.TgcCamera();
            pos = new TGCVector3(1200, 800, -300);
            Env.Camara.SetCamera(pos, new TGCVector3(pos.X, 0, pos.Z-1));
            addButton(0,-15,"Start", ()=> { Env.CambiarEscenario(0); }, Color.YellowGreen);
            //addButton(0, 0, "Options", () => { }, Color.Aqua);
            addButton(0, 15, "Exit", () => { Application.Exit(); throw new Exception("Closing"); }, Color.Red);
            pickingRay = new TgcPickingRay(Env.Input);
        }
        public override void Render()
        {
            if (Env.Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colision en base a posicion del mouse
                pickingRay.updateRay();

                foreach (KeyValuePair<string, Tuple<TGCQuad, ClickDelegate, int, int, TgcBoundingAxisAlignBox>> button in buttons)
                {
                    var aabb = button.Value.Item5;
                    TGCVector3 collisionPoint;
                    var selected = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, aabb, out collisionPoint);
                    if (selected)
                    {
                        button.Value.Item2(); 
                        break;
                    }
                }
            }
            preRender3D();
            baseRender();
            foreach (var plano in ListaPlanos)
            {
                plano.Render();
            }
            Env.DrawText.changeFont(new Font("Arial", 24, FontStyle.Bold));
            foreach (KeyValuePair<string, Tuple<TGCQuad, ClickDelegate, int, int, TgcBoundingAxisAlignBox>> button in buttons)
            {
                button.Value.Item1.Render();
                Env.DrawText.drawText(button.Key, button.Value.Item3, button.Value.Item4, Color.White);
            }
            Env.DrawText.changeFont(TGC.Core.Text.TgcText2D.VERDANA_10);
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
            if (cancionPcpal.getStatus() != TgcMp3Player.States.Playing)
            {
                cancionPcpal.closeFile();
                cancionPcpal.play(true);
            }
        }
    }
}
