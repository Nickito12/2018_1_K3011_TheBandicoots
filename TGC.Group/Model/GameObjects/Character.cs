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
        float Gravedad = -4f;
        float VelocidadTerminal = -4f;
        float DesplazamientoMaximoY = 7f;
        float velocidadSalto = 1f;
        float velocidadRotacion = 15f;
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
            Mesh.Position += new TGCVector3(0, FastMath.Clamp(VelocidadY, -DesplazamientoMaximoY, DesplazamientoMaximoY), 0);

            List<TgcBoundingAxisAlignBox> Colliders;
            var Collision = CheckColision(out Colliders);
            if (Collision)
            {
                // Colision en Y
                Mesh.Position = LastPos;
                CanJump = VelocidadY < 0;
                Collision = CheckColision(out Colliders);

                // Hack: Movimiento en XZ en el piso no funciona sin esto
                if (Collision)
                    Mesh.Position += new TGCVector3(0, 0.1f, 0);
            }
            var PosBeforeMovingInXZ = Mesh.Position;
            Mesh.Position += versorAdelante * VelocidadAdelante * ElapsedTime;
            Collision = CheckColision(out Colliders);
            if (Collision)
                Mesh.Position = PosBeforeMovingInXZ;
            if (VelocidadAdelante != 0)
                SetAnimation("Caminando");
            else
                SetAnimation("Parado");
            Camara.Target = Mesh.Position;
            var angulo = FastMath.ToRad(VelocidadLado * ElapsedTime);
            Mesh.RotateY(angulo);
            Camara.RotateY(angulo);
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
        public bool CheckColision(out List<TgcBoundingAxisAlignBox> colliders)
        {
            var collision = false;
            colliders = new List<TgcBoundingAxisAlignBox>();
            foreach (var objeto in (Env.Objetos))
            {
                var thisCollision = objeto.Collision(Mesh.BoundingBox);
                if (thisCollision)
                {
                    colliders.Add(objeto.Collider());
                    collision = true;
                }
            }
            return collision;
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
