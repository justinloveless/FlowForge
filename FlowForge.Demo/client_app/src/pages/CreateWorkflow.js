import React, { useState } from 'react';
import Api from "../api";

const CreateWorkflow = () => {
    const [definition, setDefinition] = useState('');
    const [response, setResponse] = useState(null);

    const handleCreate = async () => {
        try {
            const result = await Api.post('/Workflow/register', JSON.parse(definition));
            setResponse(result.data);
        } catch (error) {
            console.error('Error creating workflow:', error);
        }
    };

    return (
        <div>
            <h1>Create Workflow</h1>
            <textarea
                value={definition}
                onChange={(e) => setDefinition(e.target.value)}
                placeholder="Enter workflow definition in JSON"
            />
            <button onClick={handleCreate}>Create</button>
            {response && <pre>{JSON.stringify(response, null, 2)}</pre>}
        </div>
    );
};

export default CreateWorkflow;
