using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
    class Node
    {
        //Each node contains the snapses that go from it to other nodes.
        protected double Bias = 0; //same as weights - needs to not be a double?
        protected List<Synapse> Inputs = new List<Synapse>();
        protected List<Synapse> Outputs = new List<Synapse>();

        public void fire() { }

        public void sendData(Object data)
        {
            //TODO do stuff with data
        }

        public void addSource(Node source)
        {
            Inputs.Add(new Synapse(source, this, 0.0, 1));
        }

        public void addSource(Synapse source)
        {
            Inputs.Add(source);
            //TODO make sure to add everything else backwards...
        }

        public void addTarget(Node target)
        {
            Outputs.Add(new Synapse(this, target, 0.0, 1));
        }

        public void addTarget(Synapse target)
        {
            Outputs.Add(target);
        }

    }

    class InputNode : Node
    {
        //TODO input from some datastructure

    }

    class FFHiddenNode : Node
    {
        public int layer { private set; get; } //property to help visualise layers

        public FFHiddenNode() //does this need any parameters?
        {
         
        }

        //TODO //implent spiking function
    }

    class OutputNode : Node
    {

        //TODO output to some datastructure 
    }
}
