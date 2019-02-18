using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{

    /* TODOs:
    * 
    * backpropagation
    * forward pass calculations
    * input/output
    * custom event queue
    * 
    */

    public class NetworkLoop
    {
        

        public void run() 
        
    }

    public class FeedForwardNetwork
    {
        //standard FeedForward Network
        List<Node> InputNodes = new List<Node>();
        List<Node> HiddenNodes = new List<Node>();
        List<Node> OutputNodes = new List<Node>();

        public MessageHandling messageHandling = new MessageHandling();

        //every network will be run with a game like loop
        //basically run the next events in Messages in a while.
        public void run() //TODO pass the output layer?
        {
            bool exit = false;
            while(exit == false)
            {
                messageHandling.RunEventsAtCurrentTime(); //TODO is this done?
            }
        }

        public FeedForwardNetwork(int[] layers)
        {
            int last_hidden = layers.Length - 1;
            //layers[] stores the size of each layer - each layer is fully connected to the next one

            //setup of Network

            List<Node> prev_layer = new List<Node>();

            //setup input layer
            for (int node_count = 0 ; node_count < layers[0]; node_count++)
            {
                InputNode node = new InputNode();
                prev_layer.Add(node);
                InputNodes.Add(node);
            }

            for (int hidden_layer_count = 1; hidden_layer_count < last_hidden; hidden_layer_count++)
            {
                List<Node> temp_layer = new List<Node>();

                //setup hidden layer & connections to input layer
                for (int node_count = 0; node_count < layers[hidden_layer_count]; node_count++)
                {
                    FFHiddenNode hidden = new FFHiddenNode();

                    //make synapse connections
                    foreach(Node prev_node in prev_layer)
                    {
                        //setup the synapses
                        Synapse s = new Synapse(prev_node, hidden);
                        hidden.addSource(s);
                        prev_node.addTarget(s);
                    }

                    temp_layer.Add(hidden);
                    HiddenNodes.Add(hidden);
                }

                prev_layer.Clear();
                prev_layer = new List<Node>(temp_layer);

            }

            //setup output layer
            for (int node_count = 0; node_count < layers[layers.Length -1]; node_count++) //TODO not layers.length
            {
                OutputNode outnode = new OutputNode();

                //make synapse connections
                foreach(Node prev_node in prev_layer) //for each node in the last layer
                {
                    //setup the synapses
                    Synapse s = new Synapse(prev_node, outnode);
                    outnode.addSource(s);
                    prev_node.addTarget(s);
                }

                OutputNodes.Add(outnode);
            }
        }
    }
}
