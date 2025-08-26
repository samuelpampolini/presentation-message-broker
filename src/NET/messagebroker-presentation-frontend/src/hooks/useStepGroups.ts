import { useEffect, useState } from 'react';
import { fetchSteps } from '../api';

export interface StepInfo {
    value: number;
    name: string;
}

export interface StepGroup {
    type: 'publisher' | 'consumer' | string;
    steps: StepInfo[];
}

// Accepts a dependency (e.g. selectedType) to re-fetch steps when it changes
export function useStepGroups() {
    const [stepGroups, setStepGroups] = useState<StepGroup[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetchSteps()
            .then((data) => {
                setStepGroups(data || []);
                setLoading(false);
            })
            .catch((e) => {
                setError(e.message);
                setLoading(false);
            });
    }, []);

    return { stepGroups, loading, error };
}
