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
        List<Node> InputNodes = new List<Node>(); //used for easy passing in
        List<Node> AllNodes = new List<Node>(); //actually stores all nodes
        List<Node> OutputNodes = new List<Node>(); //used for easy passing out //TODO might be worth removing this

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
            while (messageHandling.RunEventsAtNextTime()) // for reference in hardware when number of loops == number of nodes we have looped around one time.
            {
                foreach(Node n in AllNodes)
                {
                    n.PostFire(); //TODO perform any after_fire(decay) functionality
                }
            }

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
                AllNodes.Add(node); //TODO added
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
                    AllNodes.Add(hidden); //TODO added
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
                AllNodes.Add(outnode);
            }
        }

        //backpropagation type training for temporal encoded LeakyIntegrateFireNodes
        public void train(List<Message>[] trainingInput, List<Message>[] trainingTarget, double eta_w = 1, double eta_th = 1, double tau_mp = 1)
        {
            //tell messagehander training is happening
            messageHandling.CurrentlyTraining = true; 

            int data_len = trainingInput.Length;

            //do training
            for (int training_data_count = 0; training_data_count < data_len; training_data_count++)
            {
                //do the forward pass to get output
                List<Message>[] output = Run(trainingInput[training_data_count], training:true);

                List<Node> next_layer = new List<Node>();
                List<Node> current_layer = OutputNodes; //TODO pass by ref/value???

                double g_bar = 0;

                while (true) //TODO what about output layer??
                {
                    foreach (Node node in current_layer) //iterate over current layer (start with 'output' nodes)
                    {
                        //TODO calculate everything in this layer.

                        double g = 1 / node.Bias;
                        g_bar += (g * g);

                        //TODO error - done before?
                    }

                    g_bar = Math.Sqrt(g_bar / current_layer.Count); //g_bar is now useable

                    foreach (Node i in current_layer) //TODO work out the CURRENT error from previous
                    {
                        double g_ratio = (1/i.Bias)/g_bar;
                        double synapse_active_ratio = 1;//Math.Sqrt(total / active); //TODO assuming one for the time being

                        double sum_weight_errors = 0;

                        foreach(Synapse j in i.Outputs) //use j to match equations
                        {
                            sum_weight_errors += j.Weight * j.Target.LastDeltaI;
                        }

                        i.LastDeltaI = g_ratio * synapse_active_ratio * sum_weight_errors; //TODO store this in the node for safe keeping


                        double x_i = 0;

                        foreach (Message m in output[1])    //TODO iterate over all messages sent/received by that node
                                                            //sent for weights, recieved for bias
                        {
                            x_i += Math.Exp((m.Time) / tau_mp); //time - currentTime
                        }

                        //TODO
                        double change_w = eta_w * i.LastDeltaI;// * x; * N/m
                        double change_th = eta_th * i.LastDeltaI;// * a; * N/m
                    }

                    //TODO break when got to input layer
                }

                /*
                for (int training_count = 0; training_count < trainingTarget.Length; training_count++) //training output should be the same length as output nodes!
                {
                    int target_num_spikes = trainingTarget[training_count].Count;

                    int actual_num_spikes = output[training_count].Count;// output.Number of Spikes for that node!;

                    //change is realted to ratio of input to output for current node

                    //lower layer backpropagation - do it for each Synapse, then each before that etc
                    foreach(Synapse input in OutputNodes[training_count].Inputs) //TODO can I just do it off number of time fired?
                    {
                        double E = target_num_spikes - actual_num_spikes;//TODO to start with assume error is just number of spikes  - is this wrong???
                        //- trainingTarget[;// - target;          //TODO calculate error: (t-o)^2 (squared to make it positive error) //TODO is this error in the node potential?
                        E = E * E; //make it positive

                        double delta_w = 0;
                        input.Weight += delta_w;
                    }

                    //TODO backpropagate to lower layers
                    //TODO remember bias
                }
                */

            }
        }
    }
}
