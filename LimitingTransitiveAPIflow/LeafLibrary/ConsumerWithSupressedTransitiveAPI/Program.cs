using IntermediateLibrary;
using LeafLibrary;
using System;

namespace ConsumerWithSupressedTransitiveAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("I can call the intermediate API" + (new IntermediateClass()).MeaningOfLife());
            Console.WriteLine("I can call the leaf API" + (new LeafClass()).MeaningOfLife());

            Console.WriteLine("Hello World!");
        }
    }
}
