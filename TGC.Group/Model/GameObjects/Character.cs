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
        // La referencia al GameModel del juego
        GameModel env;
        // El mesh del personaje
        private TgcSkeletalMesh mesh;
        //Referencia a la camara para facil acceso
        TgcThirdPersonCamera Camara;

        float velocidadY = 0f;
        float gravedad = -3f;
        float velocidadTerminal = -4f;
        float desplazamientoMaximoY = 10f;
        float velocidadSalto = 5f;
        float velocidadRotacion = 15f;
        float velocidadMovimiento = 15f;
        bool canJump = true;
        public override void Init(GameModel _env)
        {
            env = _env;
            Camara = env.NuevaCamara;

            var skeletalLoader = new TgcSkeletalLoader();
            mesh =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    // xml del mesh
                    env.MediaDir + "Robot\\Robot-TgcSkeletalMesh.xml",
                    // Carpeta del mesh 
                    env.MediaDir + "Robot\\",
                    // Animaciones
                    new[]
                    {
                        env.MediaDir + "Robot\\Caminando-TgcSkeletalAnim.xml",
                        env.MediaDir + "Robot\\Parado-TgcSkeletalAnim.xml"
                    });
            mesh.playAnimation("Parado", true);
            // Eventualmente esto lo vamos a hacer manual
            mesh.AutoTransform = true;
            mesh.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
            mesh.RotateY(FastMath.ToRad(180f));
        }
        public override void Update()
        {
            var ElapsedTime = env.ElapsedTime;
            var Input = env.Input;
            if (canJump && Input.keyPressed(Key.Space))
            {
                velocidadY = velocidadSalto;
                canJump = false;
            }
            float velocidadAdelante = 0f;
            float velocidadLado = 0;
            if (Input.keyDown(Key.UpArrow))
                velocidadAdelante += velocidadMovimiento;
            if (Input.keyDown(Key.DownArrow))
                velocidadAdelante -= velocidadMovimiento;
            if (Input.keyDown(Key.RightArrow))
                velocidadLado += velocidadRotacion;
            if (Input.keyDown(Key.LeftArrow))
                velocidadLado -= velocidadRotacion;
            var diff = Camara.LookAt - Camara.Position;
            diff.Y = 0;
            var versorAdelante = TGCVector3.Normalize(diff);
            //var versorCostado = TGCVector3.Normalize(TGCVector3.Cross(versorAdelante, new TGCVector3(0, 1, 0)));
            velocidadY = FastMath.Max(velocidadY+gravedad * ElapsedTime, velocidadTerminal);
            var lastPos = mesh.Position;
            mesh.Position += new TGCVector3(0, FastMath.Clamp(velocidadY * ElapsedTime, -desplazamientoMaximoY, desplazamientoMaximoY), 0);

            List<TgcBoundingAxisAlignBox> colliders;
            var collision = checkColision(out colliders);
            if (collision)
            {
                // Colision en Y
                mesh.Position = lastPos;
                canJump = velocidadY < 0;
                collision = checkColision(out colliders);

                // Hack: Movimiento en XZ en el piso no funciona sin esto
                if (collision)
                    mesh.Position += new TGCVector3(0, 0.1f, 0);
            }
            var posBeforeMovingInXZ = mesh.Position;
            mesh.Position += versorAdelante * velocidadAdelante * ElapsedTime;
            collision = checkColision(out colliders);
            if (collision)
                mesh.Position = posBeforeMovingInXZ;
            if (velocidadAdelante != 0)
                SetAnimation("Caminando");
            else
                SetAnimation("Parado");
            Camara.Target = mesh.Position;
            var angulo = FastMath.ToRad(velocidadLado * ElapsedTime);
            mesh.RotateY(angulo);
            Camara.rotateY(angulo);
            mesh.updateAnimation(ElapsedTime);
        }
        public override void Render()
        {
            env.DrawText.drawText("[Personaje]: " + TGCVector3.PrintVector3(mesh.Position), 0, 30, Color.OrangeRed);
            mesh.Render();
        }
        public override void Dispose()
        {
            mesh.Dispose();
        }
        public bool checkColision(out List<TgcBoundingAxisAlignBox> colliders)
        {
            var collision = false;
            colliders = new List<TgcBoundingAxisAlignBox>();
            foreach (var objeto in (env.objetos))
            {
                var thisCollision = objeto.Collision(mesh.BoundingBox);
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
            mesh.Position = posPj;
            Camara.SetCamera(posCamara, mesh.Position); 
        }
         public bool SetAnimation(string animationName, bool loop=true)
        {
            var alreadySet = mesh.CurrentAnimation.Name == animationName;
            if (!alreadySet)
                mesh.playAnimation(animationName, loop);
            return !alreadySet;
        }
    }
}
