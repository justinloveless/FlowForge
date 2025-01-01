import React from 'react';
import { BrowserRouter as Router, Route, Routes, Link } from 'react-router-dom';
import CreateWorkflow from './pages/CreateWorkflow';
import StartWorkflow from './pages/StartWorkflow';
import TriggerEvent from './pages/TriggerEvent';
import AuditEvents from './pages/AuditEvents';
import WorkflowStatus from './pages/WorkflowStatus';
import MermaidDiagram from './pages/MermaidDiagram';

function App() {
    return (
        <Router>
            <nav>
                <Link to="/">Create Workflow</Link>
                <Link to="/start">Start Workflow</Link>
                <Link to="/trigger">Trigger Event</Link>
                <Link to="/audit">Audit Events</Link>
                <Link to="/status">Workflow Status</Link>
                <Link to="/diagram">Mermaid Diagram</Link>
            </nav>
            <Routes>
                <Route path="/" element={<CreateWorkflow />} />
                <Route path="/start" element={<StartWorkflow />} />
                <Route path="/trigger" element={<TriggerEvent />} />
                <Route path="/audit" element={<AuditEvents />} />
                <Route path="/status" element={<WorkflowStatus />} />
                <Route path="/diagram" element={<MermaidDiagram />} />
            </Routes>
        </Router>
    );
}

export default App;
