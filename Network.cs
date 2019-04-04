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

        public Random random = new Random(Seed:1); //TODO allow for seed

        double Lambda = 0.001;

        public MessageHandling messageHandling;

        public LeakyIntegrateFireNetwork(int[] layers, double lambda = 0.001)
        {
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
                    LeakyIntegrateAndFireNode new_node = new LeakyIntegrateAndFireNode(messageHandling, layerIndex: layer_count, nodeIndex: node_count, lambda:Lambda);


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
                        SynapseObject input_synapse = new SynapseObject(null, new_node, 1); //TODO set weight to normalised value
                        new_node.addSource(input_synapse);
                        InputSynapses[node_count] = input_synapse;
                    }

                    double input_norm = Math.Sqrt(3.0 / (double)new_node.Inputs.Count);
                    double input_range = input_norm * 2;

                    new_node.Bias = input_norm;// * 3;///*alpha*/3 * Math.Sqrt(3/new_node.Inputs.Count);
                    if (layer_count != 0)
                    {
                        foreach (SynapseObject input in new_node.Inputs)
                        {
                            input.Weight = random.NextDouble() * /*input_range -*/ input_norm;
                        }
                    }
                    temp_layer.Add(new_node);
                }

                prev_layer = new List<LeakyIntegrateAndFireNode>(temp_layer);
                
                Nodes[layer_count] = prev_layer;
            }

            List<LeakyIntegrateAndFireNode> outs = new List<LeakyIntegrateAndFireNode>();

            //setup output layer
            for (int node_count = 0; node_count < layers[OutputLayerIndex]; node_count++)
            {                
                OutputNode outnode = new OutputNode(messageHandling, layerIndex: OutputLayerIndex, nodeIndex: node_count, Excitatory: 1);
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
                    input.Weight = random.NextDouble() * /*input_range -*/ input_norm; ;// 1 / outnode.Inputs.Count; //not a 'uniform' distribution - is this right??
                }

                outs.Add(outnode);
            }

            Nodes[OutputLayerIndex] = outs; //add the output nodes to the last layer
        }



        public List<Message>[] Run(List<Message>[] input, string printLocation = "")
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

            if (printLocation != "")
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(printLocation, true))
                {
                    file.Write("\n");
                    foreach (List<LeakyIntegrateAndFireNode> node_list in Nodes)
                    {
                        foreach (Node n in node_list)
                        {
                            {
                                file.Write((n.LastDeltaI * n.LastDeltaI).ToString() + ",");
                            }
                        }
                        file.Write("NEXTLAYER,");
                    }
                }
            }
            return messageHandling.getOutput();
        }




        //backpropagation type training for (single run) temporal encoded LeakyIntegrateFireNodes
        public void TrainLIF(List<Message>[] trainingInput, List<Message>[] trainingTarget, string printLocation = "", double eta_w = 0.002, double eta_th = 0.1, double min_bias = 0.2, double weight_beta = 0.005, double weight_lambda = 1)
        {
            if (trainingInput.Length != Nodes[0].Count || trainingTarget.Length != Nodes[OutputLayerIndex].Count)
            {
                throw new Exception("Input/Target data needs to have same dimensions as the network");
            }
            
            //do the forward pass to get output
            List<Message>[] output = Run(trainingInput, printLocation);
            int current_time = messageHandling.max_time;

            List<LeakyIntegrateAndFireNode> current_layer;
                        
            //iterate backwards through layers (backpropagration)
            for (int layer_count = OutputLayerIndex; layer_count >= 0; layer_count--)
            {
                current_layer = Nodes[layer_count];

                double g_bar = 0;
                double g_bar_backup = 0;

                double Nl = current_layer.Count; //number of Neurons in layer
                double nl = 0; //number of firing neurons in layer

                foreach (Node node in current_layer) //iterate over current layer (start with 'output' nodes)
                {
                    double g = 1.0 / node.Bias;
                    g_bar_backup += (g * g);

                    if (node.OutputMessages.Count > 0)
                    {
                        g_bar += (g * g);

                        nl++;
                    }
                }

                if (g_bar == 0)
                {
                    g_bar = Math.Sqrt(g_bar_backup / Nl);
                }
                else
                {
                    g_bar = Math.Sqrt(g_bar / nl); //g_bar is now useable
                }

                //calcualte delta i for output nodes in one pass.
                double max_outer_delta_i = 0;

                if (layer_count == OutputLayerIndex)
                {
                    foreach (Node outer_node in current_layer)
                    {
                        double actual_output_a = 0;
                        double target_output_a = 0;
                        
                        foreach (Message m in outer_node.OutputMessages)  //iterate over all messages(spikes) sent by that node
                        {
                            actual_output_a += Math.Exp((m.Time - current_time) * Lambda);                     
                        }

                        foreach (Message m in trainingTarget[outer_node.NodeIndex]) //iterate over all target values
                        {
                            target_output_a += Math.Exp((m.Time - current_time) * Lambda);
                        }

                        outer_node.LastDeltaI = target_output_a - actual_output_a;
                                              
                        if (Math.Abs(outer_node.LastDeltaI) > max_outer_delta_i)
                        {
                            max_outer_delta_i = Math.Abs(outer_node.LastDeltaI); //This is the correct way round according to normal backpropagation
                        }
                    }
                }

                double[] x_arr = new double[current_layer.Count];
                double[] a_arr = new double[current_layer.Count];

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

                    //work out error 
                    if (layer_count != OutputLayerIndex) //current_layer != Nodes[OutputLayerIndex])
                    {
                        foreach (SynapseObject i_j_synapse in i.Outputs) //use j to match equations
                        {
                           /* if (i_j_synapse.Target.OutputMessages.Count == 0)
                            {
                                continue;
                            }*/
                            sum_weight_errors += i_j_synapse.Weight * i_j_synapse.Target.LastDeltaI; //TODO not storing the error correctly
                        }

                        i.LastDeltaI = g_ratio * delta_norm * sum_weight_errors;
                    }
                    else
                    {
                        double output_layer_normalisation = max_outer_delta_i/( (ml/Ml) * Math.Sqrt(3.0 / (double)current_layer.Count));

                        i.LastDeltaI = i.LastDeltaI / output_layer_normalisation; //TODO what about normalisation for NOT the output layer
                       
                    }
                                        
                    // restrict all weights so their squares add up to 0
                    if (layer_count != 0)
                    {
                        double weight_sq_sum = 0;

                        double max_abs_weight = 0;

                        foreach (SynapseObject input_synapse in i.Inputs)
                        {
                            weight_sq_sum += input_synapse.Weight * input_synapse.Weight - 1;

                            double abs_weight = Math.Abs(input_synapse.Weight);

                            if (abs_weight > max_abs_weight)
                            {
                                max_abs_weight = abs_weight;
                            }
                        }

                        double weight_divide_factor = max_abs_weight / Math.Sqrt(3.0 / (double)i.Inputs.Count);

                        double weight_decay = 0.5 * weight_lambda * Math.Exp(weight_beta * weight_sq_sum); //TODO check value

                        if (max_abs_weight >= 1)
                        {
                            foreach (SynapseObject input_synapse in i.Inputs)
                            {
                                input_synapse.Weight /= weight_divide_factor;//weight_normalisation;
                            }
                        }

                        foreach (SynapseObject j in i.Inputs) //use j to match equations
                        {
                            double x_j = 0;

                            //TODO Not ALL inputs, just for THAT synapse

                            foreach (Message m in i.InputMessages) //iterate over all messages(spikes) received by that node 
                            {
                                if (m.Synapse == j)
                                {
                                    x_j += Math.Exp((m.Time - current_time) * Lambda);
                                }
                            }

                            x_arr[i.NodeIndex] = x_j;
                            //TODO here----------------------------
                            double change_w = eta_w * d_w_norm * (i.LastDeltaI/* + weight_decay*/) * x_j; //TODO no reduction by size of x_j or i.LastDeltai - keeps growing once weights get above 1
                                                                                                          //TODO check error sign
                            j.Weight += change_w; //TODO this resulted in larger weight???
                        }
                    }                            
                    
                    double a_i = 0;

                    foreach (Message m in i.OutputMessages) //iterate over all messages(spikes) sent by that node
                    {
                        a_i += Math.Exp((m.Time - current_time) * Lambda);
                    }

                    a_arr[i.NodeIndex] = a_i;

                    double change_th = eta_th * d_th_norm * i.LastDeltaI * a_i;

                    if (i.Bias - change_th < min_bias && layer_count != 0) //TODO not hitting this??
                    {
                        foreach(SynapseObject input_synapse in i.Inputs)
                        {
                            input_synapse.Weight += change_th;
                        }
                    }
                    else
                    {
                        i.Bias -= change_th;
                    }
                }

                /*
                //calculating different a_i thing from paper
                double[] new_ai = new double[current_layer.Count];
                
                foreach(Node n in current_layer)
                {
                    //TODO recalcualte a_i
                    new_ai[n.NodeIndex] = 0;
                    double sum = 0;
                    foreach(SynapseObject k in n.Inputs)
                    {
                        if (k.Target.OutputMessages.Count > 0)
                        {
                            sum += x_arr[n.NodeIndex] * k.Weight;
                        }
                    }
                    new_ai[n.NodeIndex] = sum / n.Bias;
                }
                */

             //   using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\beno11224\Desktop\WriteLines2.csv", true))
             //   {
             //       file.Write("NEXTLAYER,"); //TODO write all this in RUN instead, or give the option for run to.
             //   }                
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

        //network is 'untraining'

        public double MSE(List<Message>[] target, List<Message>[] actual, double currentTime) //TODO for currentTime need to use maxTime again
        {
            if (target.Length != actual.Length)
            {
                throw new Exception("target and actual must be the same length");
            }

            double sum_squared_error = 0;

            for (int count = 0; count < target.Length; count++)
            {

                double actual_value = 0;
                double target_value = 0;

                foreach (Message m in actual[count])
                {
                    actual_value += Math.Exp((m.Time - currentTime) * Lambda);
                }

                foreach(Message m in target[count])
                {
                    target_value += Math.Exp((m.Time - currentTime) * Lambda);
                }

                double error = target_value - actual_value;

                sum_squared_error += error * error;
            }

            return sum_squared_error / target.Length;
        }
    }
}
