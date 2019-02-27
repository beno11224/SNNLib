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
        public double Bias = 0; //same as weights - needs to not be a double?
        public List<Synapse> Inputs { get; protected set; }
        public List<Synapse> Outputs{ get; protected set; }

        protected MessageHandling messageHandler;

        protected int Delay = 0;
        
        public double LastDeltaI = 0;

        List<Message> InputMessages = new List<Message>();
        List<Message> OutputMessages = new List<Message>();

        public Node(MessageHandling h)
        {
            Inputs = new List<Synapse>();
            Outputs = new List<Synapse>();
            messageHandler = h;
            Bias = 1; //should be randomised
            Delay = 0;
        }

        public void Spike(int time)
        {
            //TODO add count for times spiked - just empty messages?
            foreach (Synapse output in Outputs)
            {
                messageHandler.addMessage(new Message(time + Delay, output));
            }
        }

        //e.g. for LIF do the decay
        public void PostFire()
        {
            
        }

        public void ReceiveData(Message rx)
        {
            InputMessages.Add(rx);
        }

        //return a sensible value if needed in derived classes
        public double GetValue()
        {
            return 0;
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

    public class HardwareLeakyIntegrateFireNode : Node
    {
        //explanation of harware implementation in report
        //used for where ALL neurons are connected.

        int LayerSize;

        double Accumulator = 0;
        int CurrentTime = 0; //time that the potential was calcualted at. need to 'leak' potential value before doing anything else //in hardware need the gap between 'me' and the one that sent the message
        int delay = 1; //TODO hardware not sure if this is needed?

        public HardwareLeakyIntegrateFireNode(MessageHandling h, int layerSize) : base(h)
        {
            LayerSize = layerSize;
        }
                
        public new void PostFire()
        {
            //TODO decay
            Accumulator = Accumulator*0.9;
        }

        public new void ReceiveData(Message rx)
        {
            base.ReceiveData(rx); //ensure parent method is run.

            if (rx.Time == LayerSize)
            {
                //whole loop round the nodes complete, all connections made
                //TODO don't loop - just have all nodes connected together as usual, but DELAY of neuron number (that is arbitrary but meh).
            }

            int time_diff = rx.Time - CurrentTime;

            //if we receive a message then there was a spike on that neuron.

            Accumulator += rx.sYnapse.Weight;

            if (Accumulator >= Bias)
            {
                Spike(CurrentTime);
                Accumulator = 0; //TODO discuss - is this correct or just remove threshold from ACC?
            }
        }
    }

    public class LeakyIntegrateFireNode : Node
    {
        public LeakyIntegrateFireNode(MessageHandling h) : base(h) { }

        //Leaky Integrate and Fire (https://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=6252600&tag=1)

        double Potential = 0;
        int CurrentTime = 0; //time that the potential was calcualted at. need to 'leak' potential value before doing anything else
        double MembraneResistance = 1; //the resistance to adding potential to the neuron
        double LeakTime = 30; //time for neuron to leak
        int delay = 1;

        public new void ReceiveData(Message rx)
        {
            base.ReceiveData(rx); //ensure parent method is run.
            int time_diff = rx.Time - CurrentTime;

            double dynamic_weight = (time_diff < LeakTime) ? Math.Pow(time_diff / LeakTime,2) : 1; //TODO better name
            dynamic_weight = (dynamic_weight < 1) ? dynamic_weight : 1;

            Potential = Potential * Math.Exp((rx.Time - CurrentTime) / MembraneResistance) + rx.sYnapse.Weight * dynamic_weight;

            if (Potential >= Bias) //Bias is exactly the same as a threshold in this implementation.
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
