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

        public void sendData(DoubleMessage tx)
        {
            foreach(Synapse output in Outputs)
            {
                output.sendMessage(new DoubleMessage(tx.Time + Delay,null,tx.Data)); //null because we havent stored the actual Node.
            }
        }

        public void receiveData(DoubleMessage rx)
        {
            sendData(rx);//for an example just pass data on.
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

    public class OutputNode : Node
    {
        public void sendData(OutputDoubleMessage tx)
        {
            foreach (Synapse output in Outputs)
            {
                output.sendMessage(new OutputDoubleMessage(tx.Time + Delay, null, tx.Data)); //null because we havent stored the actual Node.
            }
        }
        //TODO output to some datastructure 
    }

    public class LeakyIntegrateFireNode : Node
    {
        //Leaky Integrate and Fire (https://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=6252600&tag=1)

        //TODO this needs to be made better/again - currently for initial testing.

        double CurrentValue = 0;
        int CurrentTime = 0; //time that the currentvalue was at. need to 'leak' current value before doing anything else
        double Threshold = 10; //if neuron exceeds this value then it spikes
        double output = 1; //designated output value
        double leakiness = 1; // how much 'value' node looses per time unit
        int delay = 1;
       
        //TODO add node internal function

        public void receiveData(DoubleMessage rx)
        {
            //TODO get weighted pulse from Synapse
            //TODO work out actual current
            int time_difference = rx.Time - CurrentTime;
            CurrentValue -= leakiness * time_difference; //TODO use non-linear decay (also use doubles)
            CurrentValue = (CurrentValue < 0)? 0: CurrentValue; //ensure CurrentValue doesn't go below 0
            CurrentTime = rx.Time + delay;

            CurrentValue += rx.Data; //TODO just make it LIFMessage?
            if (CurrentValue >= Threshold)
            {
                sendData(new DoubleMessage(CurrentTime, null, output));
                CurrentValue = 0;
            }

        }

        //public void sendData(Message tx) //TODO is this changed?

    }
}
