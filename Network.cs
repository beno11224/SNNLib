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
        //standard FeedForward Network
        List<Node> InputNodes = new List<Node>(); //used for easy passing in
        List<Node> AllNodes = new List<Node>(); //actually stores all nodes
        List<Node> OutputNodes = new List<Node>(); //used for easy passing out //TODO might be worth removing this

        public MessageHandling messageHandling = new MessageHandling();

        public List<Message>[] Run(List<Message>[] input, bool training = false)
        {
            //setup inputs
            for (int node_count = 0; node_count < InputNodes.Count; node_count++)
            {
                foreach (Message message in input[node_count])
                {
                    messageHandling.addMessage(new Message(message.Time, new Synapse(null, InputNodes[node_count], 1))); //add inputs
                }
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
                return new List<Message>[] { null };// messageHandling.getTrainingOutput() }; //TODO
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

        //backpropagation type training for (single run) temporal encoded LeakyIntegrateFireNodes
        public void train(List<Message>[] trainingInput, List<Message>[] trainingTarget, double eta_w = 1, double eta_th = 1, double tau_mp = 1)
        {
            if (trainingInput.Length != InputNodes.Count || trainingTarget.Length != OutputNodes.Count)
            {
                throw new Exception("Input/Target data needs to have same dimensions as the network");
            }

            //tell nodes training is happening
            foreach(Node n in AllNodes) //TODO remember to tell nodes they're not training at the end of training.
            {
                n.ResetTrainingLists();
                n.CurrentlyTraining = true;
            }

            int data_len = trainingInput.Length;

            //do the forward pass to get output
            List<Message>[] output = Run(trainingInput, training: true);

            //TODO iterate backwards through layers (backpropagration)
            while (true) //TODO what about output layer??
            {
                double g_bar = 0;
                List<Node> next_layer = new List<Node>();
                List<Node> current_layer = OutputNodes; //TODO pass by ref/value???

                foreach (Node node in current_layer) //iterate over current layer (start with 'output' nodes)
                {
                    //TODO calculate everything in this layer.

                    double g = 1 / node.Bias;
                    g_bar += (g * g);

                    //TODO error : store in Node?
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
        }
    }
}
