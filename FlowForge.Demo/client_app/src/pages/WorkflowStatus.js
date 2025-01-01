import React, { useState } from 'react';
import Api from "../api";

const WorkflowStatus = () => {
    const [instanceId, setInstanceId] = useState('');
    const [status, setStatus] = useState(null);

    const fetchStatus = async () => {
        try {
            const { data } = await Api.get(`/Workflow/instance?workflowInstanceId=${instanceId}`);
            setStatus(data);
        } catch (error) {
            console.error('Error fetching workflow status:', error);
        }
    };

    return (
        <div>
            <h1>Workflow Status</h1>
            <input
                type="text"
                placeholder="Instance ID"
                value={instanceId}
                onChange={(e) => setInstanceId(e.target.value)}
            />
            <button onClick={fetchStatus}>Fetch Status</button>
            {status && <pre>{JSON.stringify(status, null, 2)}</pre>}
        </div>
    );
};

export default WorkflowStatus;
