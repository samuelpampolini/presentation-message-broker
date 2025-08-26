import React from 'react';

interface LiveConsumerMessagesProps {
    messages: string[];
}

export const LiveConsumerMessages: React.FC<LiveConsumerMessagesProps> = ({ messages }) => (
    <div className="mbp-history-card">
        <div className="mbp-history-header">
            <h2 className="mbp-history-title">Live Consumer Messages</h2>
        </div>
        {messages.length === 0 ? (
            <div className="mbp-history-empty">No consumer messages yet.</div>
        ) : (
            <ul className="mbp-history-list">
                {messages.map((msg, idx) => (
                    <li key={idx} className="mbp-history-item">{msg}</li>
                ))}
            </ul>
        )}
    </div>
);
