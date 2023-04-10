namespace BrainFailProductions.PolyFew.AsImpL.MathUtil
{
    /// <summary>
    /// Triangle data structure.
    /// </summary>
    /// <seealso cref="Vertex"/> 
    public class Triangle
    {
        public Vertex v1;
        public Vertex v2;
        public Vertex v3;

        public Triangle(Vertex v1, Vertex v2, Vertex v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

    }
}
