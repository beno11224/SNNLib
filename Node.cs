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
        public bool CurrentlyTraining = false;

        public List<Message> InputMessages { get; protected set; }
        public List<Message> OutputMessages { get; protected set; }

        public Node(MessageHandling h)
        {
            Inputs = new List<Synapse>();
            Outputs = new List<Synapse>();
            messageHandler = h;
            Bias = 1; //should be randomised
            Delay = 1;
            InputMessages = new List<Message>();
            OutputMessages = new List<Message>();
        }

        public virtual void Spike(int time)
        {
            //TODO add count for times spiked - just one spike with the time - don't need to know where it went
            if (CurrentlyTraining)
            {
                OutputMessages.Add(new Message(time, null)); //TODO this isn't very helpful...
            }

            foreach (Synapse output in Outputs)
            {
                messageHandler.addMessage(new Message(time + Delay, output));
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

        public void ResetTrainingLists()
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

    public class OutputNode : HardwareLeakyIntegrateFireNode
    {
        public OutputNode(MessageHandling h) : base(h) { }

        public override void Spike(int time)
        {
            //TODO add count for times spiked - just one spike with the time - don't need to know where it went
            if (CurrentlyTraining)
            {
                OutputMessages.Add(new OutputMessage(time, null));
            }

            //foreach (Synapse output in Outputs)
            //{
                messageHandler.addMessage(new OutputMessage(time + Delay, null));
            //}
        }
    }

    public class HardwareLeakyIntegrateFireNode : Node
    {
        //explanation of harware implementation in report
        //used for where ALL neurons are connected.

        double Accumulator = 0;
        int CurrentTime = 0; //time that the potential was calcualted at. need to 'leak' potential value before doing anything else //in hardware need the gap between 'me' and the one that sent the message

        public HardwareLeakyIntegrateFireNode(MessageHandling h) : base(h) { }
                
        public new void PostFire()
        {
            //TODO decay
            Accumulator = Accumulator*0.9;
        }

        public override void ReceiveData(Message rx)
        {
            CurrentTime = rx.Time;
            base.ReceiveData(rx); //ensure parent method is run.

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
