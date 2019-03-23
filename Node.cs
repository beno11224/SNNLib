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
        public List<Synapse> Outputs { get; protected set; }

        protected MessageHandling messageHandler;

        public int LayerIndex {get; protected set;}

        protected int Delay = 0;
        
        public double LastDeltaI = 0;
        public bool CurrentlyTraining = false;

        public List<Message> InputMessages { get; protected set; }
        public List<Message> OutputMessages { get; protected set; }
        
        public Node(MessageHandling h, int layerIndex)
        {
            Inputs = new List<Synapse>();
            Outputs = new List<Synapse>();
            messageHandler = h;
            LayerIndex = layerIndex;
            Bias = 1; //should be randomised
            Delay = 1;
            InputMessages = new List<Message>();
            OutputMessages = new List<Message>();
        }

        public virtual void Spike(int time, double val = 1) //val for future use
        {
            if (CurrentlyTraining)
            {
                OutputMessages.Add(new Message(time, new Synapse(this,null), val)); //TODO the null isn't very helpful
            }

            foreach (Synapse output in Outputs)
            {
                messageHandler.addMessage(new Message(time + Delay, output, val));
            }
        }

        //e.g. for LIF do the decay
        public virtual void PostFire()
        {
            
        }

        public virtual void ReceiveData(Message rx)
        {
            if (CurrentlyTraining)
            {
                InputMessages.Add(rx);
            }
        }

        public void addSource(Synapse source)
        {
            Inputs.Add(source);
        }

        public void addTarget(Synapse target)
        {
            Outputs.Add(target);
        }

        public virtual void ResetNode()
        {
            InputMessages = new List<Message>();
            OutputMessages = new List<Message>();
        }

        //if user wants a static delay on the node
        public void setDelay(int delay)
        {
            Delay = delay;
        }

    }

    public class OutputNode : LeakyIntegrateAndFireNode
    {
        public OutputNode(MessageHandling h, int layerIndex, double lambda = 1, int Excitatory = 1) : base(h, layerIndex, lambda, Excitatory) { }

        public override void Spike(int time, double val = 1)
        {
            if (CurrentlyTraining)
            {
                OutputMessages.Add(new OutputMessage(time, new Synapse(this,null), val));
            }

            messageHandler.addMessage(new OutputMessage(time + Delay, new Synapse(this,null), val));
        }
    }

    public class LeakyIntegrateAndFireNode : Node
    {
        //explanation of harware implementation in report
        //used for where ALL neurons are connected.

        //Leaky Integrate and Fire (https://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=6252600&tag=1)

        double Accumulator = 0;
        int TimePrevSpike = 0; //time that the potential was calculated at. need to 'leak' potential value before doing anything else //in hardware need the gap between 'me' and the one that sent the message
        int Excitatory = 1;
        double Lambda = 1;

        public LeakyIntegrateAndFireNode(MessageHandling h, int layerIndex, double lambda = 1, int excitatory = 1) : base(h,layerIndex)
        {
            Excitatory = excitatory;
            Lambda = lambda;
        }
                
        public new void PostFire()
        {
            //TODO decay
            Accumulator = Accumulator*0.9;
        }

        public override void ReceiveData(Message rx)
        {
            int lambda = 1; //decay constant - must be defined somewhere
            base.ReceiveData(rx); //ensure parent method is run.

            int time_diff = rx.Time - TimePrevSpike;

            //if we receive a message then there was a spike on that neuron.

            Accumulator = Accumulator * Math.Exp((TimePrevSpike - rx.Time) * lambda); //decay accumulator correctly

            Accumulator += rx.sYnapse.Weight;

            TimePrevSpike = rx.Time;

            if (Accumulator >= Bias)
            {
                Spike(TimePrevSpike, Excitatory);
                Accumulator = 0; //TODO discuss - is this correct or just remove threshold from ACC? - I mean reduce value in the accuimulator or reset it?
            }
        }

        public override void ResetNode()
        {
            base.ResetNode();
            Accumulator = 0;
            TimePrevSpike = 0;
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
