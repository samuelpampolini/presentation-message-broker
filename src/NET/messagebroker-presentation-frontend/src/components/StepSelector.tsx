
import React from 'react';

type StepInfo = {
    value: number;
    name: string;
};

interface StepSelectorProps {
    steps: StepInfo[];
    selectedStep: number | undefined;
    setSelectedStep: (step: number) => void;
    disabled?: boolean;
}

export const StepSelector: React.FC<StepSelectorProps> = ({ steps, selectedStep, setSelectedStep, disabled }) => (
    <div className="mbp-stepper">
        {steps.map((st) => (
            <button
                key={st.value}
                className={`mbp-step-btn${selectedStep === st.value ? ' mbp-step-btn-active' : ''}`}
                onClick={() => setSelectedStep(st.value)}
                disabled={disabled}
            >
                {st.name}
            </button>
        ))}
    </div>
);
