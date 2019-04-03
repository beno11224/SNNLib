using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
    [DebuggerDisplay("Layer = {LayerIndex}, Index = {NodeIndex}")]
    public class Node
    {
        //Each node contains the snapses that go from it to other nodes.
        public double Bias = 0; //same as weights - needs to not be a double?
        public List<SynapseObject> Inputs { get; protected set; }
        public List<SynapseObject> Outputs { get; protected set; }

        protected MessageHandling messageHandler;

        public int LayerIndex { get; protected set; }
        public int NodeIndex  {get; protected set; }

        protected int Delay = 0;
        
        public double LastDeltaI = 0;
        public bool CurrentlyTraining = false;

        public List<Message> InputMessages { get; protected set; }
        public List<Node> InputMesssageNodes { get; protected set; }
        public List<Message> OutputMessages { get; protected set; }
        
        public Node(MessageHandling h, int layerIndex, int nodeIndex)
        {
            Inputs = new List<SynapseObject>();
            Outputs = new List<SynapseObject>();
            messageHandler = h;
            LayerIndex = layerIndex;
            NodeIndex = nodeIndex;
            Bias = 1; //should be randomised
            Delay = 0;
            InputMessages = new List<Message>();
            InputMesssageNodes = new List<Node>();
            OutputMessages = new List<Message>();
        }

        public virtual void Spike(int time, double val = 1) //val for future use
        {
            if (CurrentlyTraining)
            {
                OutputMessages.Add(new Message(time, new SynapseObject(this,null), val));
            }

            foreach (SynapseObject output in Outputs)
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

                bool contains = false;
                foreach (LeakyIntegrateAndFireNode n in InputMesssageNodes)
                {
                    if (n == rx.Synapse.Source)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    InputMesssageNodes.Add(rx.Synapse.Source);
                }
            }
        }

        public void addSource(SynapseObject source)
        {
            Inputs.Add(source);
        }

        public void addTarget(SynapseObject target)
        {
            Outputs.Add(target);
        }

        public virtual void ResetNode()
        {
            InputMessages = new List<Message>();
            InputMesssageNodes = new List<Node>();
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
        public OutputNode(MessageHandling h, int layerIndex, int nodeIndex, double lambda = 1, int Excitatory = 1) : base(h, layerIndex, nodeIndex, lambda) { }

        public override void Spike(int time, double val = 1)
        {
            if (CurrentlyTraining)
            {
                OutputMessages.Add(new OutputMessage(time, new SynapseObject(this,null), val));
            }

            messageHandler.addMessage(new OutputMessage(time + Delay, new SynapseObject(this,null), val));
        }
    }

    public class LeakyIntegrateAndFireNode : Node
    {
        //explanation of harware implementation in report
        //used for where ALL neurons are connected.

        //Leaky Integrate and Fire (https://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=6252600&tag=1)

        double Accumulator = 0;
        int TimePrevSpike = 0; //time that the potential was calculated at. need to 'leak' potential value before doing anything else //in hardware need the gap between 'me' and the one that sent the message
        double Lambda = 1;
        
        public LeakyIntegrateAndFireNode(MessageHandling h, int layerIndex, int nodeIndex, double lambda = 1) : base(h,layerIndex,nodeIndex)
        {
            Lambda = lambda;
        }
                
        public new void PostFire()
        {
            //Accumulator = Accumulator*0.9;            //TODO maybe do decay in here??
        }

        public override void ReceiveData(Message rx)
        {
            base.ReceiveData(rx); //ensure parent method is run.
            
            //if we receive a message then there was a spike on that neuron.

            Accumulator = Accumulator * Math.Exp((TimePrevSpike - rx.Time) * Lambda); //decay accumulator correctly

            Accumulator += rx.Synapse.Weight;

            if (Accumulator < -Bias)
            {
                Accumulator = -Bias;
            }

            TimePrevSpike = rx.Time;

            if (Accumulator >= Bias)
            {
                Spike(TimePrevSpike);
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

    [DebuggerDisplay("Source = {Source.LayerIndex}_{Source.NodeIndex} Target = {Target.LayerIndex}_{Target.NodeIndex}")]
    public class SynapseObject
    {
        //store the associated weight on the input edge
        //public for ease
        //will store any child of Node
        public Node Source;
        public Node Target;
        public double Weight;

        //default weight of 1
        public SynapseObject(Node source, Node target, double weight = 1)
        {
            Source = source;
            Target = target;
            Weight = weight;
        }
    }

}
