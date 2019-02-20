using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
    public class Synapse
    {
        private MessageHandling MessageHandle;
        private Node Source; 
        private Node Target; 
        public double Weight; //TODO this is the bit that the research is done on. needs to be more than just a 'double'
        public int Delay; //maxint ~ 2 billion, so ample time units

        public Synapse(MessageHandling messageHandler, Node source, Node target)//, double weight, int delay)
        {
            MessageHandle = messageHandler;
            Source = source; //Source/Target shouldnt change.
            Target = target;
            Weight = 1; //start with weight of 0 - should be randomised in child classes
            Delay = 0;  //automatically zero delay - add this in if otherwise required.
        }

        public void sendMessage(Message rx)
        {
            MessageHandle.addMessage(new Message(rx.Time + Delay, Target, rx.Data));
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
        public FFSynapse(MessageHandling messageHandling, Node source, Node target) : base(messageHandling, source, target)
        {
            Delay = 0;
            Weight = 0;//RANDOMISE
        }

        public void sendMesage() //replace the parent method with a different version. more useful for doing fancy things
        {

        }

    }
}
