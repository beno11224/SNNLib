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
