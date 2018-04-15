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
        float Gravedad = -25f;
        float VelocidadTerminal = -50f;
        float DesplazamientoMaximoY = 10f;
        float velocidadSalto = 25f;
        float velocidadRotacion = 30f;
        float VelocidadMovimiento = 35f;
        bool CanJump = true;
        public override void Init(GameModel _env)
        {
            Env = _env;
            Camara = Env.NuevaCamara;

            var SkeletalLoader = new TgcSkeletalLoader();
            Mesh =
                SkeletalLoader.loadMeshAndAnimationsFromFile(
                    // xml del mesh
                    Env.MediaDir + "Robot\\Robot-TgcSkeletalMesh.xml",
                    // Carpeta del mesh 
                    Env.MediaDir + "Robot\\",
                    // Animaciones
                    new[]
                    {
                        Env.MediaDir + "Robot\\Caminando-TgcSkeletalAnim.xml",
                        Env.MediaDir + "Robot\\Parado-TgcSkeletalAnim.xml"
                    });
            Mesh.playAnimation("Parado", true);
            // Eventualmente esto lo vamos a hacer manual
            Mesh.AutoTransform = true;
            Mesh.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
            Mesh.RotateY(FastMath.ToRad(180f));
            Mesh.Move(0, 1, 0);
        }
        public override void Update()
        {
            var ElapsedTime = Env.ElapsedTime;
            var Input = Env.Input;
            if (CanJump && Input.keyPressed(Key.Space))
            {
                VelocidadY = velocidadSalto;
                CanJump = false;
            }
            float VelocidadAdelante = 0f;
            float VelocidadLado = 0;
            if (Input.keyDown(Key.UpArrow))
                VelocidadAdelante += VelocidadMovimiento;
            if (Input.keyDown(Key.DownArrow))
                VelocidadAdelante -= VelocidadMovimiento;
            if (Input.keyDown(Key.RightArrow))
                VelocidadLado += velocidadRotacion;
            if (Input.keyDown(Key.LeftArrow))
                VelocidadLado -= velocidadRotacion;
            var Diff = Camara.LookAt - Camara.Position;
            Diff.Y = 0;
            var versorAdelante = TGCVector3.Normalize(Diff);
            //var versorCostado = TGCVector3.Normalize(TGCVector3.Cross(versorAdelante, new TGCVector3(0, 1, 0)));
            VelocidadY = FastMath.Max(VelocidadY+Gravedad * ElapsedTime, VelocidadTerminal);
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
            Mesh.Position += versorAdelante * VelocidadAdelante * ElapsedTime;
            Collider = Env.Escenario.ColisionXZ(Mesh.BoundingBox);
            if (Collider != null)
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
            if (VelocidadAdelante != 0)
                SetAnimation("Caminando");
            else
                SetAnimation("Parado");
            Camara.Target = Mesh.Position;
            var angulo = FastMath.ToRad(VelocidadLado * ElapsedTime);
            Mesh.RotateY(angulo);
            Camara.RotateY(angulo);
           /* Camara.OffsetForward = -300f;
            Camara.OffsetHeight = 125f; */
            Mesh.updateAnimation(ElapsedTime);
        }
        public override void Render()
        {
            Env.DrawText.drawText("[Personaje]: " + TGCVector3.PrintVector3(Mesh.Position), 0, 30, Color.OrangeRed);
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
    }
}
