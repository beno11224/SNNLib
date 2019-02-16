using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
    class Synapse
    {

        //targetNode is the index of the node - array so multiple layers can be traversed.
        private Node Source; 
        private Node Target; 
        public double Weight; //TODO this is the bit that the research is done on. needs to be more than just a 'double'
        public int Delay; //maxint ~ 2 billion, so ample time units

        public Synapse(Node target, Node source, double weight, int delay)
        {
            Source = source;
            Target = target;
            Weight = weight; // this needs to be different
            Delay = delay;

            //target.add(this);
        }

        public void sendMessage()
        {
            //TODO create event for output node, remember to add  any transmit delay in HERE not in node class

            //setup event

            //Send event
        }



    }
}
