using System;
using System.Collections.Generic;

namespace SNNLib
{
    public class MessageHandling
    {

        public bool CurrentlyTraining = false;
        private List<Message> trainingList = new List<Message>();

        private List<Message> eventList = new List<Message>();
        private List<Message> output = new List<Message>();

        public bool RunEventsAtCurrentTime()
        {
            if (eventList.Count == 0)
            {
                //eventList is empty, so network has completed.
                return false;
            }

            int current_time = eventList[0].Time; //the current time of the simulation
            int event_time;

            do
            {
                eventList[0].TargetNode.ReceiveData(eventList[0]); //pass the message to the next node

                eventList.RemoveAt(0); //remove the first item as the message is sent

                event_time = eventList[0].Time; //grab the time of the next event
            }
            while (event_time == current_time); //keep grabbing the next event that is at the current time.

            return true;
        }

        public void addMessage(int time, Node target, Node source)
        {
            addMessage(new Message(time, target, source));
            if (CurrentlyTraining)
            {
                trainingList.Add(new Message(time, target, source));
            }
        }

        public void addMessage(Message message)
        {
            //insert at start
            eventList.Insert(0, message);

            //one iteration of 'bubble sort' to move the only out of place element (the one just added) to the correct place.
            for (int index = 1; index < eventList.Count; index ++)
            {
                if (eventList[index - 1].Time > eventList[index].Time)
                {
                    //swap
                    Message temp = eventList[index - 1];
                    eventList[index - 1] = eventList[index];
                    eventList[index] = temp;
                }
                else
                {
                    //sorted
                    break;
                }
            }
        }

        public List<Message> getOutput()
        {
            return output;
        }

        //give list of all messages passed
        public List<Message> getTrainingOutput()
        {
            return trainingList;
        }

        //performs action of cleaning input/output. allows user to pull output and THEN clean outputs.
        public void resetLists()
        {
            trainingList = new List<Message>();            
            eventList = new List<Message>();
            output = new List<Message>();
        }

    }

    //TODO comments
    public class Message
    {
        public int Time { get; private set; }
        public Node TargetNode { get; private set; }
        public Node SourceNode { get; private set; }

        public Message(int time, Node target, Node source)
        {
            Time = time;
            TargetNode = target;
            SourceNode = source;
        }

        public void SetNode(Node target = null, Node source = null)
        {
            if (target != null)
            {
                TargetNode = target;
            }
            if (source != null)
            {
                SourceNode = source;
            }
        }
    }

    //used for storing any output.
    public class OutputDoubleMessage : Message
    {
        public OutputDoubleMessage(int time,Node target, Node source) : base(time, target, source) { }
    }
}
