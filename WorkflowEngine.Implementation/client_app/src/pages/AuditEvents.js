import React, { useState } from 'react';
import Api from "../api";

const AuditEvents = () => {
    const [instanceId, setInstanceId] = useState('');
    const [events, setEvents] = useState(null);

    const fetchEvents = async () => {
        try {
            const { data } = await Api.get(`/Workflow/events/${instanceId}`);
            setEvents(data);
        } catch (error) {
            console.error('Error fetching events:', error);
        }
    };

    return (
        <div>
            <h1>Audit Events</h1>
            <input
                type="text"
                placeholder="Instance ID"
                value={instanceId}
                onChange={(e) => setInstanceId(e.target.value)}
            />
            <button onClick={fetchEvents}>Fetch Events</button>
            {events && <pre>{JSON.stringify(events, null, 2)}</pre>}
        </div>
    );
};

export default AuditEvents;
