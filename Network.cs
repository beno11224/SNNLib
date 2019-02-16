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
        Dictionary <int,Node> Nodes;
        Dictionary <int,Synapse> Synapses;
        //List<Synapse> Synapses; //dict or list?

        public FeedForwardNetwork(int[] layers)
        {
            int prev_layer;
            //layers[] stores the size of each layer - each layer is fully connected to the next one
            int node_count = 0;

            int node_max = layers[0]; //for efficiency etc

            //setup of Network

            //setup input layer
            for ( ; node_count < node_max; node_count++) //TODO node_count needs to encompass all nodes.
            {
                Nodes.Add(node_count, new InputNode());//TODO is this right?
            }

            for (int hidden_layer_count = 1; hidden_layer_count < layers.Length - 1; hidden_layer_count++)
            {
                node_max += layers[hidden_layer_count];
                prev_layer = hidden_layer_count - 1;

                //setup hidden layer & connections to input layer
                for (; node_count < node_max; node_count++)
                {
                    Nodes.Add(node_count,new FFHiddenNode());//[0][node_count] = new InputNode(); //TODO this might need to address problem of input.

                    int synapse_index = node_count * layers[prev_layer]; //indexing issues here - best way to solve?

                    //make synapse connections
                    for (int synapse_count = 0; synapse_count < layers[prev_layer]; synapse_count++) //for each node in the input layer
                    {
                        Synapses.Add(synapse_index + synapse_count, new Synapse(synapse_count, node_count, 1.0)); //TODO randomise weights
                    }
                }
            }

            node_max += layers[layers.Length];

            prev_layer = layers.Length - 1;

            //setup output layer
            for (; node_count < node_max; node_count++)
            {
                Nodes.Add(node_count, new OutputNode());

                int synapse_index = node_count * layers[prev_layer]; //indexing issues here - best way to solve?

                //make synapse connections
                for (int synapse_count = 0; synapse_count < layers[prev_layer]; synapse_count++) //for each node in the input layer
                {
                    Synapses.Add(synapse_index + synapse_count, new Synapse(synapse_count, node_count, 1.0)); //TODO randomise weights
                }
            }
        }
    }



}
