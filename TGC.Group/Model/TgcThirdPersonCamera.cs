using TGC.Core.Camara;
using TGC.Core.Mathematica;
using TGC.Core.Input;
using TGC.Core.Collision;
using System.Collections.Generic;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.DirectInput;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Camara en tercera persona que sigue a un objeto a un determinada distancia.
    /// </summary>
    public class TgcThirdPersonCamera : TgcCamera
    {
        //private TGCVector3 Position;
       public static TGCVector3 DEFAULT_DOWN = new TGCVector3(0f, -1f, 0f);
        public static float DEFAULT_ROTATION_SPEED = 2.5f;
        public float rotX;
        public float rotY;
        public float keyboardMovement;
        public bool colisiones = true;
        /// <summary>
        ///     Crear una nueva camara
        /// </summary>
        public TgcThirdPersonCamera()
        {
            ResetValues();
        }

        public TgcThirdPersonCamera(TGCVector3 target, float offsetHeight, float offsetForward, TgcD3dInput input)
        {
            Target = target;
            OffsetHeight = offsetHeight;
            OffsetForward = offsetForward;
            Input = input;
            UpVector = new TGCVector3(0f, 1f, 0f);
            RotationSpeed = DEFAULT_ROTATION_SPEED;
        }

        #region Getters y Setters
        /// <summary>
        ///     Desplazamiento en altura de la camara respecto del target
        /// </summary>
        public float OffsetHeight { get; set; }

        /// <summary>
        ///     Desplazamiento hacia adelante o atras de la camara repecto del target.
        ///     Para que sea hacia atras tiene que ser negativo.
        /// </summary>
        public float OffsetForward { get; set; }

        /// <summary>
        ///     Rotacion absoluta en Y de la camara
        /// </summary>
        public float RotationY { get; set; }

        /// <summary>
        ///     Objetivo al cual la camara tiene que apuntar
        /// </summary>
        public TGCVector3 Target { get; set; }
        public TgcD3dInput Input { get; set; }
        public float DiffX { get; set; }
        public float DiffY { get; set; }
        public float DiffZ { get; set; }
        /// <summary>
        ///     Velocidad de rotacion de la camara
        /// </summary>
        public float RotationSpeed { get; set; }

        /// <summary>
        ///     Centro de la camara sobre la cual se rota
        /// </summary>
        public TGCVector3 CameraCenter { get; set; }

        public TGCVector3 NextPos { get; set; }

        #endregion Getters y Setters


        public override void UpdateCamera(float elapsedTime)
        {

        }

        public void UpdateCamera(GameModel Modelo)
        {
            if (Modelo.Input.keyPressed(Key.F5))
                colisiones = !colisiones;
            if(!colisiones)
            { 
                OffsetForward = Modelo.CameraOffsetForward;
                Update(Modelo.ElapsedTime);
                return;
            }
            TGCVector3 NextPos, up, target;

            //OffsetHeight = Modelo.CameraOffsetHeight;
            this.Update(Modelo.ElapsedTime, out NextPos, out target, out up);

            //Detectar colisiones entre el segmento de recta camara-personaje y todos los objetos del escenario
            TGCVector3 q;
            var minDistSq = FastMath.Pow2(Modelo.CameraOffsetForward);
            foreach (var obstaculo in Modelo.Escenario.listaColisionesConCamara())
            {
                //Hay colision del segmento camara-personaje y el objeto
                if (TgcCollisionUtils.intersectSegmentAABB(target, NextPos, obstaculo.BoundingBox, out q))
                {
                    //Si hay colision, guardar la que tenga menor distancia
                    var distSq = TGCVector3.Subtract(q, target).LengthSq();
                    //Hay dos casos singulares, puede que tengamos mas de una colision hay que quedarse con el menor offset.
                    //Si no dividimos la distancia por 2 se acerca mucho al target.
                    minDistSq = FastMath.Min(distSq, minDistSq);
                }
            }

            //Acercar la camara hasta la minima distancia de colision encontrada (pero ponemos un umbral maximo de cercania)
            var newOffsetForward = -FastMath.Sqrt(minDistSq);
            this.Update(Modelo.ElapsedTime);

            if (FastMath.Abs(newOffsetForward) < 10)
            {
                newOffsetForward = 10;
            }
            OffsetForward = newOffsetForward;

            this.Update(Modelo.ElapsedTime);
        }

        public void Update(float elapsedTime, out TGCVector3 next, out TGCVector3 target, out TGCVector3 up)
        {
            //Obtener variacion XY del mouse
            var mouseX = 0f;
            var mouseY = 0f;
            var DiffX = this.DiffX;
            var DiffY = this.DiffY;
            var UpVector = this.UpVector;
            var NextPos = this.NextPos;
            var Target = this.Target;
            if (Input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                mouseX = Input.XposRelative;
                mouseY = Input.YposRelative;

                DiffX += mouseX * elapsedTime * RotationSpeed;
                DiffY += mouseY * elapsedTime * RotationSpeed;
            }
            else
            {
                DiffX += mouseX;
                DiffY += mouseY;
            }
            DiffX += keyboardMovement * elapsedTime * RotationSpeed;

            //Calcular rotacion a aplicar
            var rotX = -DiffY / FastMath.PI;
            var rotY = DiffX / FastMath.PI;

            //Truncar valores de rotacion fuera de rango
            if (rotX > FastMath.PI * 2 || rotX < -FastMath.PI * 2)
            {
                DiffY = 0;
                rotX = 0;
            }
            //Invertir Y de UpVector segun el angulo de rotacion
            if (rotX < -FastMath.PI / 2 && rotX > -FastMath.PI * 3 / 2)
            {
                UpVector = DEFAULT_DOWN;
            }
            else if (rotX > FastMath.PI / 2 && rotX < FastMath.PI * 3 / 2)
            {
                UpVector = DEFAULT_DOWN;
            }
            else
            {
                UpVector = DEFAULT_UP_VECTOR;
            }


            CalculatePositionTarget(rotX, rotY);

            next = NextPos;
            target = Target;
            up = UpVector;
        }

        public void Update(float elapsedTime)
        {
            //Obtener variacion XY del mouse
            var mouseX = 0f;
            var mouseY = 0f;
            if (Input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                mouseX = Input.XposRelative;
                mouseY = Input.YposRelative;

                DiffX += mouseX * elapsedTime * RotationSpeed;
                DiffY += mouseY * elapsedTime * RotationSpeed;
            }
            else
            {
                DiffX += mouseX;
                DiffY += mouseY;
            }
            DiffX += keyboardMovement * elapsedTime * RotationSpeed;

            //Calcular rotacion a aplicar
            rotX = -DiffY / FastMath.PI;
            rotY = DiffX / FastMath.PI;

            //Truncar valores de rotacion fuera de rango
            if (rotX > FastMath.PI * 1/4 || rotX < -FastMath.PI * 0)
            {
                DiffY = 0;
                rotX = 0;
            }
            //Invertir Y de UpVector segun el angulo de rotacion
            if (rotX < -FastMath.PI / 2 && rotX > -FastMath.PI * 3 / 2)
            {
                UpVector = DEFAULT_DOWN;
            }
            else if (rotX > FastMath.PI /2 && rotX < FastMath.PI * 3 / 2)
            {
                UpVector = DEFAULT_DOWN;
            }
            else
            {
                UpVector = DEFAULT_UP_VECTOR;
            }


            CalculatePositionTarget(rotX, rotY);

            //asigna las posiciones de la camara.
            base.SetCamera(NextPos, Target, UpVector);
        }


        public override void SetCamera(TGCVector3 position, TGCVector3 target)
        {
            NextPos = position;
            CameraCenter = target;
            base.SetCamera(NextPos, CameraCenter, UpVector);
        }

        /// <summary>
        ///     Carga los valores default de la camara y limpia todos los cálculos intermedios
        /// </summary>
        public void ResetValues()
        {
            OffsetHeight = 20;
            OffsetForward = -120;
            RotationY = 0;
            Target = TGCVector3.Empty;
            Position = TGCVector3.Empty;
        }

        /// <summary>
        ///     Configura los valores iniciales de la cámara
        /// </summary>
        /// <param name="target">Objetivo al cual la camara tiene que apuntar</param>
        /// <param name="offsetHeight">Desplazamiento en altura de la camara respecto del target</param>
        /// <param name="offsetForward">Desplazamiento hacia adelante o atras de la camara repecto del target.</param>
        public void SetTargetOffsets(TGCVector3 target, float offsetHeight, float offsetForward, TgcD3dInput input)
        {
            Target = target;
            OffsetHeight = offsetHeight;
            OffsetForward = offsetForward;
            Input = input;
        }

        /// <summary>
        ///     Genera la proxima matriz de view, sin actualizar aun los valores internos
        /// </summary>
        /// <param name="pos">Futura posicion de camara generada</param>
        /// <param name="targetCenter">Futuro centro de camara a generada</param>
        public void CalculatePositionTarget(float rotX, float rotY)
        {
            //alejarse, luego rotar y lueg ubicar camara en el centro deseado
            var m = TGCMatrix.Translation(0, OffsetHeight, OffsetForward)
                       * TGCMatrix.RotationX(rotX)
                       * TGCMatrix.RotationY(rotY)
                       * TGCMatrix.Translation(Target);

            //Extraer la posicion final de la matriz de transformacion
            NextPos = new TGCVector3(m.M41, m.M42, m.M43);
        }

        /// <summary>
        ///     Rotar la camara respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void RotateY(float angle)
        {
            RotationY += angle;
        }
    }
}