﻿using System;
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
        //Network made of Leaky Integrate and Fire nodes both excitatory and inhibitory
        List<Node>[] Nodes; //stores all layers
        int OutputLayerIndex;

        Random random = new Random();

        public MessageHandling messageHandling; //TODO remember to initialise  = new MessageHandling();

        //layers[] stores the size of each layer - each layer is fully connected to the next one
        public LeakyIntegrateFireNetwork(int[] layers, int[] inhibitory_percentages)
        {

            if (layers.Length != inhibitory_percentages.Length)
            {
                throw new Exception("please give an inhibitory percentage for each layer including input & output layers"); //TODO check every n nodes to see that the percentage is abotu right
            }

            Nodes = new List<Node>[layers.Length];
                       
            OutputLayerIndex = layers.Length - 1;
            
            messageHandling = new MessageHandling(layers[OutputLayerIndex]);

            //setup of Network

            List<Node> prev_layer = new List<Node>();
            
            //setup (input and) hidden layers & connections to previous layers
            for (int hidden_layer_count = 0; hidden_layer_count < OutputLayerIndex; hidden_layer_count++)
            {
                List<Node> temp_layer = new List<Node>();
                                
                for (int node_count = 0; node_count < layers[hidden_layer_count]; node_count++)
                {
                    Node hidden;
                    int r = random.Next(0, 100);
                    if (r < inhibitory_percentages[hidden_layer_count])
                    {
                        hidden = new HardwareLeakyIntegrateFireNode(messageHandling, layerIndex: node_count, excitatory: 1);
                    }
                    else
                    {
                        hidden = new HardwareLeakyIntegrateFireNode(messageHandling, layerIndex: node_count, excitatory: -1);
                    }

                    foreach (Node prev_node in prev_layer)
                    {
                        //setup the connections between the nodes
                        Synapse s = new Synapse(prev_node, hidden, 1);
                        prev_node.addTarget(s);
                        hidden.addSource(s);
                    }

                    temp_layer.Add(hidden);
                }
                
                foreach(Node outer in temp_layer)
                {
                    foreach(Node inner in temp_layer)
                    {
                        if (outer != inner)
                        {
                            Synapse s = new Synapse(outer, inner,0.1);
                            outer.addTarget(s);
                            inner.addSource(s);
                        }
                    }
                }

                prev_layer = new List<Node>(temp_layer);
                
                Nodes[hidden_layer_count] = prev_layer;
            }

            List<Node> outs = new List<Node>();

            //setup output layer
            for (int node_count = 0; node_count < layers[OutputLayerIndex]; node_count++)
            {
                OutputNode outnode;
                if (random.Next(0, 100) < inhibitory_percentages[0])
                {
                    outnode = new OutputNode(messageHandling, layerIndex: node_count, Excitatory: 1);
                }
                else
                { 
                    outnode = new OutputNode(messageHandling, layerIndex: node_count, Excitatory: -1);
                }
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
            messageHandling.resetLists();

            foreach (List<Node> current_nodes in Nodes)
            {
                foreach (Node n in current_nodes)
                {
                    n.ResetNode();
                    n.CurrentlyTraining = true;//training;
                }

            }

            //TODO reset accumulators at the end!

            //setup inputs
            for (int node_count = 0; node_count < Nodes[0].Count; node_count++)
            {
                foreach (Message message in input[node_count])
                {
                    messageHandling.addMessage(new Message(message.Time, new Synapse(null, Nodes[0][node_count], 1))); //add inputs
                }
            }

            //loop round current 'events' till none left
            while (messageHandling.RunEventsAtNextTime() && messageHandling.max_time < 1000) // for reference in hardware when number of loops == number of nodes we have looped around one time.
            {
                foreach (List<Node> current_layer in Nodes)
                {
                    foreach (Node n in current_layer)
                    {
                        n.PostFire();
                    }
                }
            }

            return messageHandling.getOutput();
        }

        //backpropagation type training for (single run) temporal encoded LeakyIntegrateFireNodes
        public void train(List<Message>[] trainingInput, List<Message>[] trainingTarget, double eta_w = 0.002, double eta_th = 0.1, double tau_mp = 100)
        {
            if (trainingInput.Length != Nodes[0].Count || trainingTarget.Length != Nodes[OutputLayerIndex].Count)
            {
                throw new Exception("Input/Target data needs to have same dimensions as the network");
            }
            
            //do the forward pass to get output
            List<Message>[] output = Run(trainingInput);//, training: true);
            int current_time = messageHandling.max_time; //TODO get the end time - this is fine right?

            List<Node> current_layer;

            //iterate backwards through layers (backpropagration)
            for( int layer_count = OutputLayerIndex; layer_count > 0; layer_count--)
            {
                current_layer = Nodes[layer_count];

                double g_bar = 0;
                
                foreach (Node node in current_layer) //iterate over current layer (start with 'output' nodes)
                {
                    double g = 1 / node.Bias;
                    g_bar += (g * g);

                    //TODO error value : store in Node?
                }

                g_bar = Math.Sqrt(g_bar / current_layer.Count); //g_bar is now useable

                int output_layer_count = 0; //TODO this might get buggy - a more permanent fix is needed.

                foreach (Node i in current_layer)
                {
                    double g_ratio = (1 / i.Bias) / g_bar;
                    double synapse_active_ratio = 1;//Math.Sqrt(total / active); //TODO assuming one for the time being

                    double sum_weight_errors = 0;

                    if (current_layer != Nodes[OutputLayerIndex])
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

                        int lambda = 1; //TODO like in Node

                        foreach (Message m in i.OutputMessages)    //iterate over all messages(spikes) sent by that node
                        {
                            actual_output_a = actual_output_a * Math.Exp((current_time - m.Time) * lambda);
                            actual_output_a++; //is it just + 1??? need the weight surely?

                            //actual_output_a += Math.Exp((m.Time - current_time) / tau_mp); //TODO get currentTime somehow...
                        }

                        foreach (Message m in trainingTarget[output_layer_count]) //iterate over all target values
                        {
                            target_output_a = target_output_a * Math.Exp((current_time - m.Time) * lambda);
                            target_output_a++; 

                            //target_output_a += Math.Exp((m.Time - current_time) / tau_mp);                            
                        }
                        
                        output_layer_count++; //TODO buggy...

                        i.LastDeltaI = actual_output_a - target_output_a;
                    }

                    foreach (Synapse j in i.Outputs) //use j to match equations
                    {
                        double x_j = 0;

                        foreach (Message m in j.Target.InputMessages)    //iterate over all messages(spikes) received by that node
                        {
                            x_j += Math.Exp((m.Time - current_time) / tau_mp);
                            //x_j = x_j * Math.Exp((current_time - m.Time) * 1);
                            //x_j+= j.Weight; 
                        }

                        double change_w = eta_w * i.LastDeltaI * x_j; //* N/m //TODO can't just do this here...
                        j.Weight += change_w;
                    }

                    double a_i = 0;

                    foreach (Message m in i.OutputMessages)    //iterate over all messages(spikes) sent by that node
                    {
                        a_i += Math.Exp((m.Time - current_time) / tau_mp); //TODO get currentTime somehow... //TODO this is wrong!!!!
                        //a_i = a_i * Math.Exp((current_time - m.Time) * 1);
                        //a_i += m.sYnapse.Weight;
                    }
                    
                    double change_th = eta_th * i.LastDeltaI * a_i; //* N/m
                    i.Bias += change_th;
                }
            }

            //tell nodes training has finished
            foreach (List<Node> current_nodes in Nodes)
            {
                foreach (Node n in current_nodes)
                {
                    n.ResetNode();
                    n.CurrentlyTraining = false;
                }

            }

        }
    }
}
