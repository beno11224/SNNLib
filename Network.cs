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

    public class LeakyIntegrateFireNetwork
    {
        //standard FeedForward Network
        List<Node> InputNodes = new List<Node>();
        List<Node> HiddenNodes = new List<Node>();
        List<Node> OutputNodes = new List<Node>();

        public MessageHandling messageHandling = new MessageHandling();

        public List<Message>[] Run(List<Message> input, bool training = false)
        {
            //setup inputs
            messageHandling.resetLists();
            messageHandling.CurrentlyTraining = training;
            foreach (Message m in input)
            {
                messageHandling.addMessage(m);
            }

            //loop round current 'events' till none left
            while (messageHandling.RunEventsAtNextTime()) //TODO in hardware when number of loops == number of nodes we have looped around one time. 
                //then time can be reset to '0' and ACC is leaked. //TODO this needs to impact the simulation of Leaky in as minimal way possible.
            { }

            //output
            if (!training)
            {
                return new List<Message>[] { messageHandling.getOutput() };//messageHandling.getOutput() };
            }
            else
            {
                //if training more than just output is needed
                return new List<Message>[] { messageHandling.getTrainingOutput() };
            }
        }

        //layers[] stores the size of each layer - each layer is fully connected to the next one
        public LeakyIntegrateFireNetwork(int[] layers)
        {
            int last_hidden = layers.Length - 1;

            //setup of Network

            List<Node> prev_layer = new List<Node>();

            //setup input layer
            for (int node_count = 0 ; node_count < layers[0]; node_count++)
            {
                LeakyIntegrateFireNode node = new LeakyIntegrateFireNode(messageHandling);
                prev_layer.Add(node);
                InputNodes.Add(node);
            }

            for (int hidden_layer_count = 1; hidden_layer_count < last_hidden; hidden_layer_count++)
            {
                List<Node> temp_layer = new List<Node>();

                //setup hidden layer & connections to input layer
                for (int node_count = 0; node_count < layers[hidden_layer_count]; node_count++)
                {
                    LeakyIntegrateFireNode hidden = new LeakyIntegrateFireNode(messageHandling);

                    foreach(Node prev_node in prev_layer)
                    {
                        //setup the connections between the nodes
                        Synapse s = new Synapse(prev_node, hidden, 1);
                        prev_node.addTarget(s);
                        hidden.addSource(s);
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
                OutputNode outnode = new OutputNode(messageHandling);

                foreach(Node prev_node in prev_layer)
                {
                    //setup the connections between the nodes
                    Synapse s = new Synapse(prev_node, outnode, 1);
                    prev_node.addTarget(s);
                    outnode.addSource(s);
                }

                OutputNodes.Add(outnode);
            }
        }

        //backpropagation type training for temporal encoded LeakyIntegrateFireNodes
        public void train(List<Message>[] trainingInput, List<Message>[] trainingTarget)
        {
            //tell messagehander training is happening
            messageHandling.CurrentlyTraining = true; 

            int data_len = trainingInput.Length;

            //do training
            for (int data_count = 0; data_count < data_len; data_count++)
            {
                //do the forward pass
                List<Message>[] output = Run(trainingInput[data_count], training:true);

                for (int training_count = 0; training_count < trainingTarget.Length; training_count++) //training output should be the same length as output nodes!
                {
                    int target_num_spikes = trainingTarget[training_count].Count;

                    int actual_num_spikes = output[training_count].Count;// output.Number of Spikes for that node!;

                    //change is realted to ratio of input to output for current node

                    //lower layer backpropagation - do it for each Synapse, then each before that etc
                    foreach(Synapse input in OutputNodes[training_count].Inputs) //TODO can I just do it off number of time fired?
                    {
                        double E = input.Source.Spikes; //- trainingTarget[;// - target;          //TODO calculate error: (t-o)^2 (squared to make it positive error) //TODO is this error in the node potential?
                        E = E * E; //make it positive

                        double delta_w = 0;
                        input.Weight += delta_w;
                    }

                    //TODO backpropagate to lower layers
                    //TODO remember bias
                }
            }
        }
    }
}
