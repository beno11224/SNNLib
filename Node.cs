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
            sendData(rx);//TODO - what to do in the general term? (just pass it on as an example?)
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

        public FFHiddenNode() //does this need any parameters?
        {
         
        }

        //TODO //implent spiking function
    }

    public class OutputNode : Node
    {

        //TODO output to some datastructure 
    }
}
