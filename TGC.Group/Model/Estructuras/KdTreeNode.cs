using TGC.Core.SceneLoader;

namespace TGC.Group.Model.Estructuras
{
    /// <summary>
    ///     Nodo del �rbol KdTree
    /// </summary>
    internal class KdTreeNode
    {
        public KdTreeNode[] children;
        public TgcMesh[] models;

        //Corte realizado
        public float xCut;

        public float yCut;
        public float zCut;

        public bool isLeaf()
        {
            return children == null;
        }
    }
}