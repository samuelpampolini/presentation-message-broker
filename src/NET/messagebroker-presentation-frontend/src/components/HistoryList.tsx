import React from 'react';

interface HistoryEntry {
    exampleKey: string;
    exampleTitle: string;
    stepValue: number;
    stepName: string;
    result: string;
    isComplete: boolean;
    timestamp: number;
}

interface HistoryListProps {
    history: HistoryEntry[];
    onClear: () => void;
}

export const HistoryList: React.FC<HistoryListProps> = ({ history, onClear }) => (
    <div className="mbp-history-card">
        <div className="mbp-history-header">
            <h2 className="mbp-history-title">Execution History</h2>
            <button className="mbp-clear-btn" onClick={onClear} disabled={history.length === 0}>Clear History</button>
        </div>
        {history.length === 0 ? (
            <div className="mbp-history-empty">No executions yet.</div>
        ) : (
            <ul className="mbp-history-list">
                {history.map((h) => (
                    <li key={h.timestamp + '-' + h.exampleKey} className="mbp-history-item">
                        <div><b>Example:</b> {h.exampleTitle} ({h.exampleKey})</div>
                        <div><b>Step:</b> {h.stepName}</div>
                        <div><b>Result:</b> {h.result}</div>
                        <div><b>Is Complete:</b> {h.isComplete ? 'Yes' : 'No'}</div>
                        <div className="mbp-history-time">{new Date(h.timestamp).toLocaleString()}</div>
                    </li>
                ))}
            </ul>
        )}
    </div>
);
