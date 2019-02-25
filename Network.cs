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
    * store forward pass vars
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
                LeakyIntegrateFireNode node = new LeakyIntegrateFireNode();
                prev_layer.Add(node);
                InputNodes.Add(node);
            }

            for (int hidden_layer_count = 1; hidden_layer_count < last_hidden; hidden_layer_count++)
            {
                List<Node> temp_layer = new List<Node>();

                //setup hidden layer & connections to input layer
                for (int node_count = 0; node_count < layers[hidden_layer_count]; node_count++)
                {
                    LeakyIntegrateFireNode hidden = new LeakyIntegrateFireNode();

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
        //backpropagation type training for temporal encoded LeakyIntegrateFireNodes
        public void train(List<DoubleMessage[]> trainingData)
        {
            messageHandling.CurrentlyTraining = true; //tell messagehander training is happening

            int data_len = trainingData.Count;

            //do training
            for (int data_count = 0; data_count < data_len; data_count++)
            {
                messageHandling.resetLists();
                //setup input
                foreach (DoubleMessage inputMessage in trainingData[data_count])
                {
                    messageHandling.addMessage(inputMessage);
                }

                //forward pass
                //List<DoubleMessage> training_output = run(); //TODO store all events

                //for () all of the output FOR ALL LAYERS
                    //reason you use all of the events is because BACKPROPAGATION - need to compare the END potential to DESIRED end potential

                //backwards pass
                    //do it
            }
        }
    }
}
