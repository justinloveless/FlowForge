import React, { useState, useEffect, useRef } from 'react';
import api from '../api';
import mermaid from 'mermaid';

const MermaidDiagram = () => {
    const [definitionId, setDefinitionId] = useState('');
    const [diagram, setDiagram] = useState('');
    const [showDetails, setShowDetails] = useState(false);
    const diagramContainerRef = useRef(null); // Reference to the diagram container

    const fetchDiagram = async () => {
        if (!definitionId) return;
        try {
            const { data } = await api.get(`/Workflow/mermaid/generate/${definitionId}`, {
                params: { showDetails }
            });
            setDiagram(data); // Update the diagram text
        } catch (error) {
            console.error('Error fetching Mermaid diagram:', error);
        }
    };

    useEffect(() => {
        // Fetch diagram whenever `showDetails` toggles
        fetchDiagram();
    }, [showDetails]);

    useEffect(() => {
        if (diagram && diagramContainerRef.current) {
            // Inject the raw diagram text into the container
            diagramContainerRef.current.innerHTML = `<div class="mermaid">${diagram}</div>`;
            try {
                // Process the new Mermaid diagram in the DOM
                mermaid.run();
            } catch (error) {
                console.error('Error running Mermaid:', error);
            }
        }
    }, [diagram]); // Re-run the effect when the diagram changes

    const toggleDetails = () => {
        setShowDetails(!showDetails);
    };

    return (
        <div>
            <h1>Mermaid Diagram</h1>
            <input
                type="text"
                placeholder="Workflow Definition ID"
                value={definitionId}
                onChange={(e) => setDefinitionId(e.target.value)}
            />
            <button onClick={fetchDiagram}>Fetch Diagram</button>
            <label>
                <input
                    type="checkbox"
                    checked={showDetails}
                    onChange={toggleDetails}
                />
                Show Details
            </label>
            {/* Container for the Mermaid diagram */}
            <div ref={diagramContainerRef}></div>
        </div>
    );
};

export default MermaidDiagram;
