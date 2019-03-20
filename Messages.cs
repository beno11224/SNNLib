using System;
using System.Collections.Generic;

namespace SNNLib
{
    public class MessageHandling
    {
        private List<Message> eventList = new List<Message>();
        private List<Message>[] output;

        public MessageHandling(int outputSize)//int[] layers)
        {
            output = new List<Message>[outputSize];
            for (int count = 0; count < outputSize; count++)
            {
                output[count] = new List<Message>();
            }
        }

        public int max_time { private set; get; }

        public bool RunEventsAtNextTime()
        {
            if (eventList.Count == 0)
            {
                //eventList is empty, so network has completed.
                return false;
            }

            int current_time = eventList[0].Time; //the current time of the simulation
            int event_time;

            max_time = current_time; //save the last time we ran something.

            do
            {
                eventList[0].sYnapse.Target.ReceiveData(eventList[0]); //pass the message to the next node

                eventList.RemoveAt(0); //remove the first item as the message is sent

                if (eventList.Count == 0)
                {
                    return false; //eventList is empty, so network has completed.
                }

                event_time = eventList[0].Time; //grab the time of the next event
            }
            while (event_time == current_time); //keep grabbing the next event that is at the current time.

            return true;
        }

        public void addMessage(Message message)
        {
            if (message.GetType().Equals(typeof(OutputMessage))) //TODO does this work???
            {
                output[message.sYnapse.Source.LayerIndex].Add(message);
            }
            else
            {
                //insert at start
                eventList.Insert(0, message);



                //one iteration of 'bubble sort' to move the only out of place element (the one just added) to the correct place. ensure it goes to the end of the possible arrangements
                for (int index = 1; index < eventList.Count; index++)
                {
                    if (eventList[index - 1].Time >= eventList[index].Time)
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
        public List<Message>[] getOutput()
        {
            return output;
        }

        //performs action of cleaning input/output. allows user to pull output and THEN clean outputs.
        public void resetLists()
        {
            eventList = new List<Message>();
            for(int output_index = 0; output_index < output.Length; output_index++)
            {
                output[output_index] = new List<Message>();
            }
            max_time = 0;
        }

    }

    //store time and the synapse the message was sent over
    public class Message
    {
        //can only be set privately, but got publicly
        public int Time { get; private set; }
        public Synapse sYnapse { get; private set; } //TODO name
        public double Val { get; private set; } //passing different values to the nodes. set at one unless needed (e.g. for inhibitory)

        public Message(int time, Synapse synapse, double val = 1)
        {
            Time = time;            
            sYnapse = synapse;
            Val = val;
        }
    }

    //used for storing output.
    public class OutputMessage : Message
    {
        public OutputMessage(int time, Synapse synapse, double val = 1) : base(time, synapse, val) { }
    }
}
