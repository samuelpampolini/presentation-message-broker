export const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5042';

export const fetchExamples = async () => {
    const res = await fetch(`${API_URL}/examples`);
    if (!res.ok) throw new Error('Failed to fetch examples');
    const data = await res.json();
    return data.examples;
};

export const fetchSteps = async () => {
    const res = await fetch(`${API_URL}/steps`);
    if (!res.ok) throw new Error('Failed to fetch steps');
    const data = await res.json();
    return data.steps;
};

export const executeStep = async (exampleKey: string, step: number) => {
    const res = await fetch(`${API_URL}/examples/execute-step`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ exampleKey, step })
    });
    if (!res.ok) throw new Error('Failed to execute step');
    return res.json();
};
