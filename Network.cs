using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{

    /* TODOs:
    * 
    * //TODO backpropagation stuff...
    * //TODO given input, calculate output
    * 
    */



    public abstract class Network
    {
        //abstract way of storing whole collection of nodes - including input/output
        Dictionary<int,Node> Nodes; 
    }


    public class FeedForwardNetwork : Network
    {
        //standard FeedForward Network
        List<Node> InputNodes = new List<Node>();
        List<Node> HiddenNodes = new List<Node>();
        List<Node> OutputNodes = new List<Node>();

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
                        Synapse s = new Synapse(prev_node, hidden, 1, 0);
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
                    Synapse s = new Synapse(prev_node, outnode, 1, 0);
                    outnode.addSource(s);
                    prev_node.addTarget(s);
                }

                OutputNodes.Add(outnode);
            }
        }
    }



}
