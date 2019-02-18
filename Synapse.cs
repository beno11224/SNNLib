using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
    public class Synapse
    {
        //targetNode is the index of the node - array so multiple layers can be traversed.
        private Node Source; 
        private Node Target; 
        public double Weight; //TODO this is the bit that the research is done on. needs to be more than just a 'double'
        public int Delay; //maxint ~ 2 billion, so ample time units

        public Synapse(Node source, Node target)//, double weight, int delay)
        {
            Source = source; //Source/Target shouldnt change.
            Target = target;
            Weight = 0; //start with weight of 0 - should be randomised in child classes
            Delay = 0;  //automatically zero delay - add this in if otherwise required.
        }

        public void sendMessage(Message rx)
        {
            Message tx = new Message(rx.Time + Delay, Target, rx.Data);
        }

        public void setDelay(int delay)
        {
            Delay = delay;
        }

        public void setWeight(double weight)
        {
            Weight = weight;
        }

    }

    public class FFSynapse : Synapse
    {
        public FFSynapse(Node source, Node target) : base(source, target)
        {
            Delay = 0;
            Weight = 0;//RANDOMISE
        }

        public void sendMesage() //replace the parent method with a different version. more useful for doing fancy things
        {

        }

    }
}
