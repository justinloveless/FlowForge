import React, { useState } from 'react';
import Api from "../api";

const TriggerEvent = () => {
    const [instanceId, setInstanceId] = useState('');
    const [eventName, setEventName] = useState('');
    const [eventData, setEventData] = useState('{}');
    const [response, setResponse] = useState(null);

    const handleTrigger = async () => {
        try {
            const result = await Api.post(`/Workflow/${instanceId}/trigger`, {
                EventName: eventName,
                EventData: JSON.parse(eventData),
            });
            setResponse(result.data);
        } catch (error) {
            console.error('Error triggering event:', error);
        }
    };

    return (
        <div>
            <h1>Trigger Event</h1>
            <input
                type="text"
                placeholder="Instance ID"
                value={instanceId}
                onChange={(e) => setInstanceId(e.target.value)}
            />
            <input
                type="text"
                placeholder="Event Name"
                value={eventName}
                onChange={(e) => setEventName(e.target.value)}
            />
            <textarea
                placeholder="Event Data (JSON)"
                value={eventData}
                onChange={(e) => setEventData(e.target.value)}
            />
            <button onClick={handleTrigger}>Trigger</button>
            {response && <pre>{JSON.stringify(response, null, 2)}</pre>}
        </div>
    );
};

export default TriggerEvent;
