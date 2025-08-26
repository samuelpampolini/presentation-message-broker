import React from 'react';

interface ResultDisplayProps {
    result: { result: string; isComplete: boolean } | null;
}

export const ResultDisplay: React.FC<ResultDisplayProps> = ({ result }) => (
    result ? (
        <div className="mbp-result">
            <div><b>Result:</b> {result.result}</div>
            <div><b>Is Complete:</b> {result.isComplete ? 'Yes' : 'No'}</div>
        </div>
    ) : null
);
