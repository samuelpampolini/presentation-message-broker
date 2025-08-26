import React from 'react';

interface ExampleInfo {
    key: string;
    title: string;
    type: 'publisher' | 'consumer' | string;
}

interface ExampleSelectorProps {
    examples: ExampleInfo[];
    selectedExample: string;
    setSelectedExample: (key: string) => void;
    label: string;
    disabled?: boolean;
}

export const ExampleSelector: React.FC<ExampleSelectorProps> = ({ examples, selectedExample, setSelectedExample, label, disabled }) => (
    <div className="mbp-form-row">
        <label className="mbp-label">{label}</label>
        <select className="mbp-select" value={selectedExample} onChange={e => setSelectedExample(e.target.value)} disabled={disabled}>
            <option value="">Select {label.toLowerCase()}</option>
            {examples.map(ex => (
                <option key={ex.key} value={ex.key}>{ex.title} ({ex.key})</option>
            ))}
        </select>
    </div>
);
