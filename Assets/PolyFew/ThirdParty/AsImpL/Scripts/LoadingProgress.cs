using System.Collections.Generic;

namespace BrainFailProductions.PolyFew.AsImpL
{
    /// <summary>
    /// Loading progress information for a single OBJ loader
    /// </summary>
    public class SingleLoadingProgress
    {
        public string fileName;
        public string message;
        public float percentage = 0;
        // TODO: split percentages, e.g.: public float parsingPercentage = 0;
        public int numObjects = 0;
        public int numSubObjects = 0;
        public bool error = false;
    }

    /// <summary>
    /// Overall loading progress for all the active OBJ loaders (list of SingleLoadingProgress).
    /// See <see cref="SingleLoadingProgress"/>.
    /// </summary>
    public class LoadingProgress
    {
        public List<SingleLoadingProgress> singleProgress = new List<SingleLoadingProgress>();
    }
}
