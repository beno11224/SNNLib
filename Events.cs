using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNNLib
{
    class EventHandling
    {
        //TODO catch all events from synapses
        //TODO at right time, filter event queue 

        List<CommunicationEvent> eventList = new List<CommunicationEvent>();

        public void RunEventsAtCurrentTime()
        {
            int current_time = eventList[0].Time; //the current time of the simulation
            int event_time;

            do
            {
                eventList[0].NodeToCommunicate.sendData(eventList[0].Data); //pass the message to the next node

                eventList.RemoveAt(0); //remove the first item as the message is sent

                event_time = eventList[0].Time; //grab the time of the next event
            }
            while (event_time == current_time); //keep grabbing the next event that is at the current time.
        }

    }

    class CommunicationEvent
    {
        public int Time { get; private set; }
        public Node NodeToCommunicate { get; private set; }
        public Object Data { get; private set; }//TODO do I really just want an int/double?

        public CommunicationEvent(int time, Node node, Object data)
        {
            Time = time;
            NodeToCommunicate = node;
            Data = data;
        }
    }
}
