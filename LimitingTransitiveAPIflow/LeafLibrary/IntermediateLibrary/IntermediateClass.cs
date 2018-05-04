using LeafLibrary;
using System;

namespace IntermediateLibrary
{
    public class IntermediateClass
    {

        private LeafClass LeafClass;
        
        public IntermediateClass()
        {
            LeafClass = new LeafClass();
        }

        public int MeaningOfLife()
        {
            return LeafClass.MeaningOfLife();
        }
    }
}
