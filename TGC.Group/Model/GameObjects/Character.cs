using TGC.Core.SkeletalAnimation;
using TGC.Core.Camara;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Mathematica;
using TGC.Core.Collision;
using TGC.Core.BoundingVolumes;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model.GameObjects
{
    public class Character : GameObject
    {
        // El mesh del personaje
        private TgcSkeletalMesh Mesh;
        //Referencia a la camara para facil acceso
        TgcThirdPersonCamera Camara;

        float VelocidadY = 0f;
        float Gravedad = -60f;
        float VelocidadTerminal = -50f;
        float DesplazamientoMaximoY = 10f;
        float velocidadSalto = 60f;
        float VelocidadMovimiento = 35f;
        float ultimoDesplazamientoAdelante = 0f;
        bool CanJump = true;
        bool updateAnimation = true;
        TiposColision TipoColisionActual;
        TiposColision UltimoTipoColision;

        //posicion con respecto a la plataforma
        private TGCVector3 posicionPlataforma;

        public override void Init(GameModel _env)
        {
            Env = _env;
            Camara = Env.NuevaCamara;

            var SkeletalLoader = new TgcSkeletalLoader();
            Mesh =
                SkeletalLoader.loadMeshAndAnimationsFromFile(
                    // xml del mesh
                    Env.MediaDir + "Piloto\\Pilot-TgcSkeletalMesh.xml",
                    // Carpeta del mesh 
                    Env.MediaDir + "Piloto\\",
                    // Animaciones
                    new[]
                    {
                        Env.MediaDir + "Piloto\\Animations\\Walk-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\CrouchWalk-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\LowKick-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Piloto\\Animations\\Jump-TgcSkeletalAnim.xml"
                    });

            Mesh.playAnimation("StandBy", true);
            // Eventualmente esto lo vamos a hacer manual
            Mesh.AutoTransform = true;
            Mesh.Scale = new TGCVector3(0.3f, 0.3f, 0.3f);
            Mesh.RotateY(FastMath.ToRad(180f));
        }

        public override void Update()
        {
            var ElapsedTime = Env.ElapsedTime;
            var Input = Env.Input;
            float VelocidadAdelante = 0f;
            var Diff = Camara.LookAt - Camara.Position;
            Diff.Y = 0;
            TipoColisionActual = TiposColision.SinColision;

            if (CanJump && Input.keyPressed(Key.Space))
            {
                VelocidadY = velocidadSalto;
                CanJump = false;
            }

            if (Input.keyDown(Key.W) || Input.keyDown(Key.UpArrow))
                VelocidadAdelante += VelocidadMovimiento;
            if (Input.keyDown(Key.S) || Input.keyDown(Key.DownArrow))
                VelocidadAdelante -= VelocidadMovimiento;
            Camara.keyboardMovement = 0;
            if (Input.keyDown(Key.D) || Input.keyDown(Key.RightArrow))
                Camara.keyboardMovement += 1;
            if (Input.keyDown(Key.A) || Input.keyDown(Key.LeftArrow))
                Camara.keyboardMovement -= 1;
            if (Input.keyDown(Key.R))
                Mesh.Position = new TGCVector3(0, 1, 0);

            var versorAdelante = TGCVector3.Normalize(Diff);
            VelocidadY = FastMath.Clamp(VelocidadY + Gravedad * ElapsedTime, VelocidadTerminal, -VelocidadTerminal);
            var LastPos = Mesh.Position;
            // Colision en Y
            Mesh.Position += new TGCVector3(0, FastMath.Clamp(VelocidadY * ElapsedTime, -DesplazamientoMaximoY, DesplazamientoMaximoY), 0);
            TgcBoundingAxisAlignBox Collider = Env.Escenario.ColisionY(Mesh.BoundingBox);

            if (Collider != null)
            {
                Mesh.Position = LastPos;
                CanJump = VelocidadY < 0;
            }

            var PosBeforeMovingInXZ = Mesh.Position;
            if (UltimoTipoColision == TiposColision.PisoResbaloso)
                VelocidadAdelante += ultimoDesplazamientoAdelante;
            else
                ultimoDesplazamientoAdelante = 0;
            Mesh.Position += versorAdelante * VelocidadAdelante * ElapsedTime;
            ultimoDesplazamientoAdelante += VelocidadAdelante * ElapsedTime;
            Collider = Env.Escenario.ColisionXZ(Mesh.BoundingBox);
            UltimoTipoColision = TipoColisionActual;

            if (Collider == null && TipoColisionActual == TiposColision.Pozo)
            {
                Mesh.Position += new TGCVector3(0, -15f, 0);
                //Esto estaria codeado a "Manopla", haciendo que el bbox del pj termine por debajo del bbox del Pozo, para que no haya problemas, ya que el analisis de la colision es en XZ
            }
			 //El personaje se movera con la plataforma
			else if (Collider == null && TipoColisionActual == TiposColision.Caja)
            {
                Mesh.Move(posicionPlataforma);
            }
            else if (Collider != null)
            {

                var movementRay = PosBeforeMovingInXZ - Mesh.Position;
                var rs = TGCVector3.Empty;

                if (((Mesh.BoundingBox.PMax.X > Collider.PMax.X && movementRay.X > 0) ||
                    (Mesh.BoundingBox.PMin.X < Collider.PMin.X && movementRay.X < 0)) &&
                    ((Mesh.BoundingBox.PMax.Z > Collider.PMax.Z && movementRay.Z > 0) ||
                    (Mesh.BoundingBox.PMin.Z < Collider.PMin.Z && movementRay.Z < 0)))
                {
                    //Este primero es un caso particularse dan las dos condiciones simultaneamente entonces para saber de que lado moverse hay que hacer algunos calculos mas.
                    //por el momento solo se esta verificando que la posicion actual este dentro de un bounding para moverlo en ese plano.
                    if (Mesh.Position.X > Collider.PMin.X && Mesh.Position.X < Collider.PMax.X)
                    {
                        //El personaje esta contenido en el bounding X
                        //Sliding Z Dentro de X
                        rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                    }
                    if (Mesh.Position.Z > Collider.PMin.Z && Mesh.Position.Z < Collider.PMax.Z)
                    {
                        //El personaje esta contenido en el bounding Z
                        //Sliding X Dentro de Z
                        rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                    }
                    //Seria ideal sacar el punto mas proximo al bounding que colisiona y chequear con eso, en ves que con la posicion.
                }
                else
                {
                    if ((Mesh.BoundingBox.PMax.X > Collider.PMax.X && movementRay.X > 0) ||
                        (Mesh.BoundingBox.PMin.X < Collider.PMin.X && movementRay.X < 0))
                    {
                        //Sliding X
                        rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                    }
                    if ((Mesh.BoundingBox.PMax.Z > Collider.PMax.Z && movementRay.Z > 0) ||
                        (Mesh.BoundingBox.PMin.Z < Collider.PMin.Z && movementRay.Z < 0))
                    {
                        //Sliding Z
                        rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                    }
                }
                Mesh.Position = PosBeforeMovingInXZ - rs;
            }

            updateAnimation = true;
            if (Mesh.Position.Y != LastPos.Y)
            {
                SetAnimation("Jump", false);
            }
            else if (Input.keyDown(Key.LeftShift) || Input.keyDown(Key.RightShift)) {
                SetAnimation("CrouchWalk");
                updateAnimation = VelocidadAdelante != 0;
            }
            else 
                if (VelocidadAdelante != 0)
                    SetAnimation("Walk");
                else
                    SetAnimation("StandBy");
            

            Camara.Target = Mesh.Position;
            Mesh.Rotation = new TGCVector3(0, Camara.rotY + FastMath.PI, 0);
            if (updateAnimation)
                Mesh.updateAnimation(ElapsedTime);
        }

        //setPosicion con respecto a la plataforma
        internal void setposition(TGCVector3 tGCVector3)
        {
            posicionPlataforma = tGCVector3;
        }

        public override void Render()
        {
            Env.DrawText.drawText("[Pos pj]: " + TGCVector3.PrintVector3(Mesh.Position), 0, 20, Color.OrangeRed);
            Env.DrawText.drawText("Velocidad Y: " + VelocidadY.ToString(), 0, 40, Color.OrangeRed);
            Env.DrawText.drawText("Ctrl: Render BB", 0, 60, Color.OrangeRed);
            Env.DrawText.drawText("Shift: Crouch", 0, 80, Color.OrangeRed);
            Env.DrawText.drawText("R: Reiniciar Posición", 0, 100, Color.OrangeRed);
            Mesh.Render();
        }

        public override void Dispose()
        {
            Mesh.Dispose();
        }

        public bool CheckColision(out TgcBoundingAxisAlignBox Collider)
        {
            Collider = Env.Escenario.ColisionXZ(Mesh.BoundingBox);
            var Colision = Collider == null;
            return Colision;
        }

        internal void Move(TGCVector3 posPj, TGCVector3 posCamara)
        { 
            Mesh.Position = posPj;
            Camara.SetCamera(posCamara, Mesh.Position); 
        }

        public bool SetAnimation(string animationName, bool loop=true)
        {
            var AlreadySet = Mesh.CurrentAnimation.Name == animationName;
            if (!AlreadySet)
                Mesh.playAnimation(animationName, loop);
            return !AlreadySet;
        }
        public void SetTipoColisionActual(TiposColision resultadoColision)
        {
            TipoColisionActual = resultadoColision;
        }
    }
}
