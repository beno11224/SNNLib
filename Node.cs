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

        protected MessageHandling messageHandler;

        protected int Delay = 0;

        public Node(MessageHandling h)
        {
            messageHandler = h;
            Bias = 1; //should be randomised
            Delay = 0;
        }

        public void Spike(int time)
        {
            foreach (Synapse output in Outputs)
            {
                messageHandler.addMessage(new Message(time + Delay, output));
            }
        }

        public void ReceiveData(Message rx)
        {
            Spike(rx.Time);//for an example just pass data on.
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
        public OutputNode(MessageHandling h) : base(h) { }

        public void sendData(OutputMessage tx)
        {
            foreach (Synapse output in Outputs)
            {
                //TODO output data - where does this go?
                output.Target.ReceiveData(new OutputMessage(tx.Time + Delay, output)); //null because we havent stored the actual Node.
            }
        }
    }

    public class LeakyIntegrateFireNode : Node
    {
        public LeakyIntegrateFireNode(MessageHandling h) : base(h) { }

        //Leaky Integrate and Fire (https://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=6252600&tag=1)

        double Potential = 0;
        int CurrentTime = 0; //time that the potential was calcualted at. need to 'leak' potential value before doing anything else
        double Threshold = 10; //if neuron exceeds this value then it spikes
        double MembraneResistance = 1; //the resistance to adding potential to the neuron
        double LeakTime = 30; //time for neuron to leak
        int delay = 1;
       
        public new void ReceiveData(Message rx)
        {
            int time_diff = rx.Time - CurrentTime;

            double dynamic_weight = (time_diff < LeakTime) ? Math.Pow(time_diff / LeakTime,2) : 1; //TODO better name
            dynamic_weight = (dynamic_weight < 1) ? dynamic_weight : 1;

            Potential = Potential * Math.Exp((rx.Time - CurrentTime) / MembraneResistance) + rx.sYnapse.Weight * dynamic_weight;

            if (Potential >= Threshold)
            {
                Spike(CurrentTime);
                Potential = 0;
            }

        }
    }

    public class Synapse
    {
        //store the associated weight on the input edge
        //public for ease
        //will store any child of Node
        public Node Source;
        public Node Target;
        public double Weight;

        //default weight of 1
        public Synapse(Node source, Node target, double weight = 1)
        {
            Source = source;
            Target = target;
            Weight = weight;
        }
    }

}
