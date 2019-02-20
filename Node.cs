using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
   public class Node
    {
        //Each node contains the snapses that go from it to other nodes.
        protected double Bias = 0; //same as weights - needs to not be a double?
        protected List<Synapse> Inputs = new List<Synapse>();
        protected List<Synapse> Outputs = new List<Synapse>();

        public int Delay = 0;

        public void sendData(Message tx)
        {
            //TODO do stuff with data
            foreach(Synapse output in Outputs)
            {
                output.sendMessage(new Message(tx.Time + Delay,null,tx.Data)); //null because we havent stored the actual Node.
            }
        }

        public void receiveData(Message rx)
        {
            sendData(rx);//for a general node automatically just pass it on.
        }

        public void addSource(Synapse source)
        {
            Inputs.Add(source);
        }

        public void addTarget(Synapse target)
        {
            Outputs.Add(target);
        }

        //if user wants a static delay on the node
        public void setDelay(int delay)
        {
            Delay = delay;
        }

    }

    public class InputNode : Node
    {
        //TODO input from some datastructure

    }

    public class FFHiddenNode : Node
    {
        public int layer { private set; get; } //property to help visualise layers

        //TODO //implent spiking function
    }

    public class OutputNode : Node
    {

        //TODO output to some datastructure 
    }

    public class LIFNode : Node
    {
        //Leaky Integrate and Fire (https://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=6252600&tag=1)

        //TODO this needs to be made better/again - currently for initial testing.

        int CurrentValue = 0;
        int CurrentTime = 0; //time that the currentvalue was at. need to 'leak' current value before doing anything else
        int Threshold = 10; //if neuron exceeds this value then it spikes
        int output = 1; //designated output value
        int leakiness = 1; // how much 'value' node looses per time unit
       
        public void receiveData(Message rx)
        {
            //TODO get weighted pulse from Synapse
            //TODO work out actual current
            int time_difference = rx.Time - CurrentTime;
            CurrentValue -= leakiness * time_difference; //TODO use non-linear decay (also use doubles)
            CurrentValue = (CurrentValue < 0)? 0: CurrentValue; //ensure CurrentValue doesn't go below 0
            CurrentTime = rx.Time;
        }

        //public void sendData(Message tx) //TODO is this changed?

    }
}
