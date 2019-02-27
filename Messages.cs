using System;
using System.Collections.Generic;

namespace SNNLib
{
    public class MessageHandling
    {
        private List<Message> eventList = new List<Message>();
        private List<Message> output = new List<Message>();

        public bool RunEventsAtNextTime()
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
                eventList[0].sYnapse.Target.ReceiveData(eventList[0]); //pass the message to the next node

                eventList.RemoveAt(0); //remove the first item as the message is sent

                event_time = eventList[0].Time; //grab the time of the next event
            }
            while (event_time == current_time); //keep grabbing the next event that is at the current time.

            return true;
        }

        public void addMessage(Message message)
        {
            if (message.GetType().Equals(typeof(OutputMessage))) //TODO does this work???
            {
                output.Add(message);
            }
            else
            {
                //insert at start
                eventList.Insert(0, message);



                //one iteration of 'bubble sort' to move the only out of place element (the one just added) to the correct place.
                for (int index = 1; index < eventList.Count; index++)
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
        }

        //just give contents of output list
        public List<Message> getOutput()
        {
            return output;
        }

        //performs action of cleaning input/output. allows user to pull output and THEN clean outputs.
        public void resetLists()
        {
            eventList = new List<Message>();
            output = new List<Message>();
        }

    }

    //store time and the synapse the message was sent over
    public class Message
    {
        //can only be set privately, but got publicly
        public int Time { get; private set; }
        public Synapse sYnapse { get; private set; }

        public Message(int time, Synapse synapse)
        {
            Time = time;
            sYnapse = synapse;
        }
    }

    //used for storing output.
    public class OutputMessage : Message
    {
        public OutputMessage(int time, Synapse synapse) : base(time, synapse) { }
    }
}
