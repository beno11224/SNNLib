using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
    public class LeakyIntegrateFireNetwork
    {

        //Network made of Leaky Integrate and Fire nodes both excitatory and inhibitory
        List<LeakyIntegrateAndFireNode>[] Nodes; //stores the size of each layer - each layer is fully connected to the next one
        SynapseObject[] InputSynapses;
        int OutputLayerIndex;

        public Random random = new Random();

        double Lambda = 0;

        public MessageHandling messageHandling;

        public LeakyIntegrateFireNetwork(int[] layers, int[] inhibitory_percentages, double lambda = 1)
        {

            if (layers.Length != inhibitory_percentages.Length)
            {
                throw new Exception("please give an inhibitory percentage for each layer including input & output layers"); //TODO could check every n nodes to see that the percentage is about right
            }

            Lambda = lambda;

            Nodes = new List<LeakyIntegrateAndFireNode>[layers.Length];
            InputSynapses = new SynapseObject[layers[0]]; //input same size as input nodes
                       
            OutputLayerIndex = layers.Length - 1;
            
            messageHandling = new MessageHandling(layers[OutputLayerIndex]);

            //setup of Network

            List<LeakyIntegrateAndFireNode> prev_layer = new List<LeakyIntegrateAndFireNode>();
            
            //setup (input and) hidden layers & connections to previous layers
            for (int layer_count = 0; layer_count < OutputLayerIndex; layer_count++)
            {
                List<LeakyIntegrateAndFireNode> temp_layer = new List<LeakyIntegrateAndFireNode>();
                                
                for (int node_count = 0; node_count < layers[layer_count]; node_count++)
                {
                    LeakyIntegrateAndFireNode new_node;
                    int r = random.Next(0, 100);
                    if (r < inhibitory_percentages[layer_count])
                    {
                        new_node = new LeakyIntegrateAndFireNode(messageHandling, layerIndex: layer_count, nodeIndex: node_count, excitatory: 1);
                    }
                    else
                    {
                        new_node = new LeakyIntegrateAndFireNode(messageHandling, layerIndex: layer_count, nodeIndex: node_count, excitatory: -1);
                    }


                    foreach (LeakyIntegrateAndFireNode prev_node in prev_layer)
                    {
                        //setup the connections between the nodes
                        SynapseObject s = new SynapseObject(prev_node, new_node, 1);
                        prev_node.addTarget(s);
                        new_node.addSource(s);
                    }

                    if (layer_count == 0) // input layer
                    {
                        //setup the input synapses (one synapse going to first layer of nodes)
                        SynapseObject input_synapse = new SynapseObject(null, new_node, 1);
                        new_node.addSource(input_synapse);
                        InputSynapses[node_count] = input_synapse;
                    }

                    double input_norm = Math.Sqrt(3.0 / (double)new_node.Inputs.Count);
                    double input_range = input_norm * 2;

                    new_node.Bias = input_norm;// * 3;///*alpha*/3 * Math.Sqrt(3/new_node.Inputs.Count);
                    foreach (SynapseObject input in new_node.Inputs)
                    {
                        input.Weight = random.NextDouble() * input_range - input_norm;
                    }

                    temp_layer.Add(new_node);
                }

                /* //TODO work out how this affects everything and if it's needed/wanted
                foreach(LeakyIntegrateAndFireNode outer in temp_layer)
                {
                    foreach(LeakyIntegrateAndFireNode inner in temp_layer)
                    {
                        if (outer != inner)
                        {
                            Synapse s = new Synapse(outer, inner,0.1);
                            outer.addTarget(s);
                            inner.addSource(s);
                        }
                    }
                }*/

                prev_layer = new List<LeakyIntegrateAndFireNode>(temp_layer);
                
                Nodes[layer_count] = prev_layer;
            }

            List<LeakyIntegrateAndFireNode> outs = new List<LeakyIntegrateAndFireNode>();

            //setup output layer
            for (int node_count = 0; node_count < layers[OutputLayerIndex]; node_count++)
            {                
                OutputNode outnode;
                if (random.Next(0, 100) < inhibitory_percentages[0])
                {
                    outnode = new OutputNode(messageHandling, layerIndex: OutputLayerIndex, nodeIndex: node_count, Excitatory: 1);
                }
                else
                { 
                    outnode = new OutputNode(messageHandling, layerIndex: OutputLayerIndex, nodeIndex: node_count, Excitatory: -1);
                }
                foreach (LeakyIntegrateAndFireNode prev_node in prev_layer)
                {
                    //setup the connections between the nodes
                    SynapseObject s = new SynapseObject(prev_node, outnode, 1);
                    prev_node.addTarget(s);
                    outnode.addSource(s);
                }

                double input_norm = Math.Sqrt(3.0 / (double)outnode.Inputs.Count); //TODO other layers
                double input_range = input_norm * 2;

                outnode.Bias = input_norm;// * 3;//* alpha ;
                foreach (SynapseObject input in outnode.Inputs)
                {
                    input.Weight = random.NextDouble() * input_range - input_norm; ;// 1 / outnode.Inputs.Count; //not a 'uniform' distribution - is this right??
                }

                outs.Add(outnode);
            }

            Nodes[OutputLayerIndex] = outs; //add the output nodes to the last layer
        }

        public List<Message>[] Run(List<Message>[] input, bool training = false)
        {
            messageHandling.resetLists();

            foreach (List<LeakyIntegrateAndFireNode> current_nodes in Nodes)
            {
                foreach (LeakyIntegrateAndFireNode n in current_nodes)
                {
                    n.ResetNode();
                    n.CurrentlyTraining = true;//training;
                }
            }
            
            //setup inputs
            for (int node_count = 0; node_count < Nodes[0].Count; node_count++)
            {
                foreach (Message message in input[node_count])
                {
                    messageHandling.addMessage(new Message(message.Time, InputSynapses[node_count], 1)); //add inputs
                }
            }

            //loop round current 'events' till none left
            while (messageHandling.RunEventsAtNextTime() && messageHandling.max_time < 1000) // for reference in hardware when number of loops == number of nodes we have looped around one time.
            {
                foreach (List<LeakyIntegrateAndFireNode> current_layer in Nodes)
                {
                    foreach (LeakyIntegrateAndFireNode n in current_layer)
                    {
                        n.PostFire();
                    }
                }
            }

            return messageHandling.getOutput();
        }

        //backpropagation type training for (single run) temporal encoded LeakyIntegrateFireNodes
        public void TrainLIF(List<Message>[] trainingInput, List<Message>[] trainingTarget, double eta_w = 0.002, double eta_th = 0.1)
        {
            if (trainingInput.Length != Nodes[0].Count || trainingTarget.Length != Nodes[OutputLayerIndex].Count)
            {
                throw new Exception("Input/Target data needs to have same dimensions as the network");
            }
            
            //do the forward pass to get output
            List<Message>[] output = Run(trainingInput, training: true);
            int current_time = messageHandling.max_time;

            List<LeakyIntegrateAndFireNode> current_layer;

            //iterate backwards through layers (backpropagration)
            for( int layer_count = OutputLayerIndex; layer_count >= 0; layer_count--)
            {
                current_layer = Nodes[layer_count];

                double g_bar = 0;

                double Nl = current_layer.Count; //number of Neurons in layer
                double nl = 0; //number of firing neurons in layer


                //TODO what about inhibiting neurons - surely they are the reverse?
                

                foreach (Node node in current_layer) //iterate over current layer (start with 'output' nodes)
                {
                    double g = 1 / node.Bias;
                    g_bar += (g * g);
                    
                    if (node.OutputMessages.Count > 0)
                    {
                        nl++;
                    }
                }                             

                g_bar = Math.Sqrt(g_bar / current_layer.Count); //g_bar is now useable
                
                foreach (Node i in current_layer)
                {                    
                    double ml = i.InputMesssageNodes.Count; //number of active synapses of a neuron (assumed over all neurons in layer) //TODO
                    double Ml = i.Inputs.Count; //number of Synapses of neuron (assumed input synapses)

                    if (ml == 0)
                    { //divide by zero error - no messages given to this node keep as it is.
                        continue;
                    }

                    double d_w_norm = Math.Sqrt(Nl / ml);
                    double d_th_norm = Math.Sqrt(Nl / (ml * Ml));
                    double delta_norm = Math.Sqrt(Nl / nl); //ml+1 == nl etc

                    double g_ratio = (1 / i.Bias) / g_bar;                   

                    double sum_weight_errors = 0;

                    if (current_layer != Nodes[OutputLayerIndex])
                    {
                        foreach (SynapseObject i_j_synapse in i.Outputs) //use j to match equations
                        {
                            sum_weight_errors += i_j_synapse.Weight * i_j_synapse.Target.LastDeltaI; //TODO not storing the error correctly
                        }

                        i.LastDeltaI = g_ratio * delta_norm * sum_weight_errors;
                    }
                    else
                    {
                        double actual_output_a = 0;
                        double target_output_a = 0;

                        double lambda = 0.001; //TODO like in Node

                        foreach (Message m in i.OutputMessages)  //iterate over all messages(spikes) sent by that node
                        {
                            actual_output_a = actual_output_a * Math.Exp((current_time - m.Time) * lambda);
                            actual_output_a++;//= m.Synapse.Weight;                            
                        }

                        foreach (Message m in trainingTarget[i.NodeIndex]) //iterate over all target values
                        {
                            target_output_a = target_output_a * Math.Exp((current_time - m.Time) * lambda);
                            target_output_a++;                        
                        }

                        i.LastDeltaI = target_output_a - actual_output_a;
                    }

                    foreach (SynapseObject j in i.Outputs) //use j to match equations
                    {
                        double x_j = 0;

                        foreach (Message m in j.Target.InputMessages) //iterate over all messages(spikes) received by that node
                        {
                            x_j = x_j * Math.Exp((m.Time - current_time) * 1);
                            x_j+= j.Weight; 
                        }

                        double change_w = eta_w * d_w_norm * i.LastDeltaI * x_j;
                        j.Weight -= change_w; //TODO this resulted in larger weight???
                    }

                    double a_i = 0;

                    foreach (Message m in i.OutputMessages) //iterate over all messages(spikes) sent by that node
                    {
                        a_i = a_i * Math.Exp((m.Time - current_time) * 1);
                        a_i += m.Synapse.Weight; //TODO this doesn't make sense...
                    }
                    
                    double change_th = eta_th * d_th_norm * i.LastDeltaI * a_i;
                    i.Bias -= change_th;
                }
            }

            //tell nodes training has finished
            foreach (List<LeakyIntegrateAndFireNode> current_nodes in Nodes)
            {
                foreach (LeakyIntegrateAndFireNode n in current_nodes)
                {
                    n.ResetNode();
                    n.CurrentlyTraining = false;
                }

            }

        }
    }
}
