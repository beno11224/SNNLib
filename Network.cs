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
    * 
    */

    public class FeedForwardNetwork
    {
        //standard FeedForward Network
        List<Node> InputNodes = new List<Node>();
        List<Node> HiddenNodes = new List<Node>();
        List<Node> OutputNodes = new List<Node>();

        public MessageHandling messageHandling = new MessageHandling();

        //every network will be run with a game like loop
        //Input/Output is determined outside of Library
        public void run()
        {
            //remember to initialise input before using it. 

            while(messageHandling.RunEventsAtCurrentTime()) //loop round current 'events' till none left
            { }

            //remember to get output
        }

        //layers[] stores the size of each layer - each layer is fully connected to the next one
        public FeedForwardNetwork(int[] layers)
        {
            int last_hidden = layers.Length - 1;

            //setup of Network

            List<Node> prev_layer = new List<Node>();

            //setup input layer
            for (int node_count = 0 ; node_count < layers[0]; node_count++)
            {
                LIFNode node = new LIFNode();
                prev_layer.Add(node);
                InputNodes.Add(node);
            }

            for (int hidden_layer_count = 1; hidden_layer_count < last_hidden; hidden_layer_count++)
            {
                List<Node> temp_layer = new List<Node>();

                //setup hidden layer & connections to input layer
                for (int node_count = 0; node_count < layers[hidden_layer_count]; node_count++)
                {
                    LIFNode hidden = new LIFNode();

                    //make synapse connections
                    foreach(Node prev_node in prev_layer)
                    {
                        //setup the synapses
                        Synapse s = new Synapse(messageHandling, prev_node, hidden);
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
            for (int node_count = 0; node_count < layers[last_hidden]; node_count++)
            {
                OutputNode outnode = new OutputNode();

                //make synapse connections
                foreach(Node prev_node in prev_layer) //for each node in the last layer
                {
                    //setup the synapses
                    Synapse s = new Synapse(messageHandling, prev_node, outnode);
                    outnode.addSource(s);
                    prev_node.addTarget(s);
                }

                OutputNodes.Add(outnode);
            }
        }
    }
}
