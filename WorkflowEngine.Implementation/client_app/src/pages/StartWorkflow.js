import React, { useState } from 'react';
import Api from "../api";

const StartWorkflow = () => {
    const [workflowId, setWorkflowId] = useState('');
    const [initialData, setInitialData] = useState('{}');
    const [response, setResponse] = useState(null);

    const handleStart = async () => {
        try {
            const result = await Api.post(`/Workflow/start/${workflowId}`, JSON.parse(initialData));
            setResponse(result.data);
        } catch (error) {
            console.error('Error starting workflow:', error);
        }
    };

    return (
        <div>
            <h1>Start Workflow</h1>
            <input
                type="text"
                placeholder="Workflow Definition ID"
                value={workflowId}
                onChange={(e) => setWorkflowId(e.target.value)}
            />
            <textarea
                placeholder="Initial Data (JSON)"
                value={initialData}
                onChange={(e) => setInitialData(e.target.value)}
            />
            <button onClick={handleStart}>Start</button>
            {response && <pre>{JSON.stringify(response, null, 2)}</pre>}
        </div>
    );
};

export default StartWorkflow;
