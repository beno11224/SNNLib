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

    public class FeedForwardNetwork
    {
        //standard FeedForward Network
        List<Node> InputNodes = new List<Node>();
        List<Node> HiddenNodes = new List<Node>();
        List<Node> OutputNodes = new List<Node>();

        public MessageHandling messageHandling = new MessageHandling();

        public void run()
        {
            //remember to initialise input before using it. 

            //loop round current 'events' till none left
            while (messageHandling.RunEventsAtCurrentTime()) 
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
                        prev_node.addTarget(hidden);
                        hidden.addSource(new NodeWeight(prev_node, 1));
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
                    prev_node.addTarget(outnode);
                    outnode.addSource(new NodeWeight(prev_node, 1));
                }

                OutputNodes.Add(outnode);
            }
        }
        //backpropagation type training for temporal encoded LeakyIntegrateFireNodes
        public void train(List<Message[]> trainingData)
        {
            //tell messagehander training is happening
            messageHandling.CurrentlyTraining = true; 

            int data_len = trainingData.Count;

            //do training
            for (int data_count = 0; data_count < data_len; data_count++)
            {
                messageHandling.resetLists();
                //setup input
                foreach (Message inputMessage in trainingData[data_count])
                {
                    messageHandling.addMessage(inputMessage);
                }

                //forward pass
                //List<DoubleMessage> training_output = run(); //TODO store all events

                //for () all of the output FOR ALL LAYERS
                    //reason you use all of the events is because BACKPROPAGATION - need to compare the END potential to DESIRED end potential

                //backwards pass
                    //TODO do it
            }
        }
    }
}
