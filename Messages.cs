using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
    public class MessageHandling
    {
        //TODO catch all events from synapses
        //TODO at right time, filter event queue 

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
                eventList[0].NodeToCommunicate.receiveData(eventList[0]); //pass the message to the next node

                eventList.RemoveAt(0); //remove the first item as the message is sent

                event_time = eventList[0].Time; //grab the time of the next event
            }
            while (event_time == current_time); //keep grabbing the next event that is at the current time.

            return true;
        }

        public void addMessage(int time, Node target, Object data)
        {
            addMessage(new Message(time, target, data));
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

    }

    public class Message
    {
        public int Time { get; private set; }
        public Node NodeToCommunicate { get; private set; }
        public Object Data { get; private set; }//TODO do I really just want an int/double?

        public Message(int time, Node node, Object data)
        {
            Time = time;
            NodeToCommunicate = node;
            Data = data;
        }

        public void SetNode(Node target)
        {
            if (target != null)
            {
                NodeToCommunicate = target;
            }
        }
    }

    //used for storing any output.
    public class OutputMessage : Message
    {
        public OutputMessage(int time,Node node, Object data) : base(time, node, data) { }
    }
}
