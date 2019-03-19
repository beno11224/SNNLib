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
        //List<Node> InputNodes = new List<Node>(); //used for easy passing in
        //List<Node> AllNodes = new List<Node>(); //actually stores all nodes
        List<Node>[] Nodes; //TODO stores all layers
        int OutputLayerIndex;
        //List<Node> OutputNodes = new List<Node>(); //used for easy passing out //TODO might be worth removing this

        public MessageHandling messageHandling = new MessageHandling();

        //layers[] stores the size of each layer - each layer is fully connected to the next one
        public LeakyIntegrateFireNetwork(int[] layers)
        {
            Nodes = new List<Node>[layers.Length];

            //TODO add loops in hidden layers AND add percentage of inhibitory nodes


            OutputLayerIndex = layers.Length - 1;

            //setup of Network

            List<Node> prev_layer = new List<Node>();

            //setup input layer
            for (int node_count = 0; node_count < layers[0]; node_count++)
            {
                HardwareLeakyIntegrateFireNode node = new HardwareLeakyIntegrateFireNode(messageHandling);
                prev_layer.Add(node);
            }

            Nodes[0] = prev_layer;

            for (int hidden_layer_count = 1; hidden_layer_count < OutputLayerIndex; hidden_layer_count++)
            {
                List<Node> temp_layer = new List<Node>();

                //setup hidden layer & connections to input layer
                for (int node_count = 0; node_count < layers[hidden_layer_count]; node_count++)
                {
                    HardwareLeakyIntegrateFireNode hidden = new HardwareLeakyIntegrateFireNode(messageHandling);

                    foreach (Node prev_node in prev_layer)
                    {
                        //setup the connections between the nodes
                        Synapse s = new Synapse(prev_node, hidden, 1);
                        prev_node.addTarget(s);
                        hidden.addSource(s);
                    }

                    temp_layer.Add(hidden);
                }
                
                prev_layer = new List<Node>(temp_layer);
                
                Nodes[hidden_layer_count] = prev_layer;
            }

            List<Node> outs = new List<Node>();

            //setup output layer
            for (int node_count = 0; node_count < layers[OutputLayerIndex]; node_count++)
            {
                OutputNode outnode = new OutputNode(messageHandling);

                foreach (Node prev_node in prev_layer)
                {
                    //setup the connections between the nodes
                    Synapse s = new Synapse(prev_node, outnode, 1);
                    prev_node.addTarget(s);
                    outnode.addSource(s);
                }

                outs.Add(outnode);
            }

            Nodes[OutputLayerIndex] = outs; //add the output nodes to the last layer
        }

        public List<Message>[] Run(List<Message>[] input, bool training = false)
        {
            //setup inputs
            for (int node_count = 0; node_count < Nodes[0].Count; node_count++)
            {
                foreach (Message message in input[node_count])
                {
                    messageHandling.addMessage(new Message(message.Time, new Synapse(null, Nodes[0][node_count], 1))); //add inputs
                }
            }

            //loop round current 'events' till none left
            while (messageHandling.RunEventsAtNextTime()) // for reference in hardware when number of loops == number of nodes we have looped around one time.
            {
                foreach (List<Node> current_layer in Nodes)
                {
                    foreach (Node n in current_layer)
                    {
                        n.PostFire();
                    }
                }
            }

            return new List<Message>[] { messageHandling.getOutput() };
        }

        //backpropagation type training for (single run) temporal encoded LeakyIntegrateFireNodes
        public void train(List<Message>[] trainingInput, List<Message>[] trainingTarget, double eta_w = 0.002, double eta_th = 0.1, double tau_mp = 100)
        {
            if (trainingInput.Length != Nodes[0].Count || trainingTarget.Length != Nodes[OutputLayerIndex].Count)
            {
                throw new Exception("Input/Target data needs to have same dimensions as the network");
            }

            messageHandling.resetLists();

            //tell nodes training is happening
            foreach (List<Node> current_nodes in Nodes)
            {
                foreach (Node n in current_nodes)
                {
                    //TODO remember to tell nodes they're not training at the end of training.
                    n.ResetTrainingLists();
                    n.CurrentlyTraining = true;
                }

            }

            //do the forward pass to get output
            List<Message>[] output = Run(trainingInput, training: true); //TODO get the end time
            int current_time = messageHandling.max_time;

            List<Node> current_layer;//TODO pass by ref/value???

            //iterate backwards through layers (backpropagration)
            for( int layer_count = OutputLayerIndex; layer_count > 0; layer_count--)
            {
                current_layer = Nodes[OutputLayerIndex];

                double g_bar = 0;
                
                foreach (Node node in current_layer) //iterate over current layer (start with 'output' nodes)
                {
                    //TODO calculate everything in this layer.

                    double g = 1 / node.Bias;
                    g_bar += (g * g);

                    //TODO for inhibition just add it all up and take away the current node from total?

                    //TODO error : store in Node?
                }

                g_bar = Math.Sqrt(g_bar / current_layer.Count); //g_bar is now useable

                int output_layer_count = 0; //TODO this might get buggy - a more permanent fix is needed.

                foreach (Node i in current_layer)
                {
                    double g_ratio = (1 / i.Bias) / g_bar;
                    double synapse_active_ratio = 1;//Math.Sqrt(total / active); //TODO assuming one for the time being

                    double sum_weight_errors = 0;

                    if (current_layer != Nodes[OutputLayerIndex]) //TODO test //TODO use a_i for output
                    {
                        //TODO only for fired synapses?
                        foreach (Synapse j in i.Outputs) //use j to match equations
                        {
                            sum_weight_errors += j.Weight * j.Target.LastDeltaI;
                        }

                        i.LastDeltaI = g_ratio * synapse_active_ratio * sum_weight_errors; //TODO store this in the node for safe keeping
                    }
                    else
                    {
                        double actual_output_a = 0;
                        double target_output_a = 0;

                        foreach (Message m in i.OutputMessages)    //iterate over all messages(spikes) sent by that node
                        {
                            actual_output_a += Math.Exp((m.Time - current_time) / tau_mp); //TODO get currentTime somehow...
                        }

                        foreach (Message m in trainingTarget[output_layer_count]) //iterate over all target values
                        {
                            target_output_a += Math.Exp((m.Time - current_time) / tau_mp); //TODO get currentTime somehow...
                        }

                        output_layer_count++; //TODO buggy...

                        i.LastDeltaI = actual_output_a - target_output_a; //TODO is it target- or actual-??
                    }

                    foreach (Synapse j in i.Outputs) //use j to match equations
                    {
                        double x_j = 0;

                        foreach (Message m in j.Target.InputMessages)    //iterate over all messages(spikes) received by that node
                        {
                            x_j += Math.Exp((m.Time - current_time) / tau_mp);
                        }

                        double change_w = eta_w * i.LastDeltaI * x_j; //* N/m //TODO can't just do this here...
                        j.Weight += change_w;
                    }

                    double a_i = 0;

                    foreach (Message m in i.OutputMessages)    //iterate over all messages(spikes) sent by that node
                    {
                        a_i += Math.Exp((m.Time - current_time) / tau_mp); //TODO get currentTime somehow...
                    }
                    
                    double change_th = eta_th * i.LastDeltaI * a_i; //* N/m
                    i.Bias += change_th;
                }
            }
        }
    }
}
