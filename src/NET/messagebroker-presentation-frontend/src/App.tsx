
import React, { useEffect, useState } from 'react';
import './App.css';
import { fetchExamples, fetchSteps, executeStep } from './api';

interface ExampleInfo {
  key: string;
  title: string;
}

interface StepInfo {
  value: number;
  name: string;
}

interface ExecuteStepResponse {
  result: string;
  isComplete: boolean;
}

function App() {
  const [examples, setExamples] = useState<ExampleInfo[]>([]);
  const [steps, setSteps] = useState<StepInfo[]>([]);
  const [selectedExample, setSelectedExample] = useState<string>('');
  const [selectedStep, setSelectedStep] = useState<number>();
  const [result, setResult] = useState<ExecuteStepResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchExamples().then(setExamples).catch(e => setError(e.message));
    fetchSteps().then(setSteps).catch(e => setError(e.message));
  }, []);

  const handleExecute = async () => {
    if (!selectedExample || selectedStep === undefined) return;
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      const res = await executeStep(selectedExample, selectedStep);
      setResult(res);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="App" style={{ maxWidth: 600, margin: '2rem auto', fontFamily: 'sans-serif' }}>
      <h1>MessageBroker API Demo</h1>
      {error && <div style={{ color: 'red' }}>{error}</div>}
      <div style={{ marginBottom: 16 }}>
        <label>
          Example:
          <select value={selectedExample} onChange={e => setSelectedExample(e.target.value)} style={{ marginLeft: 8 }}>
            <option value="">Select example</option>
            {examples.map(ex => (
              <option key={ex.key} value={ex.key}>{ex.title} ({ex.key})</option>
            ))}
          </select>
        </label>
      </div>
      <div style={{ marginBottom: 16 }}>
        <label>
          Step:
          <select value={selectedStep} onChange={e => setSelectedStep(Number(e.target.value))} style={{ marginLeft: 8 }}>
            <option value="">Select step</option>
            {steps.map(st => (
              <option key={st.value} value={st.value}>{st.name}</option>
            ))}
          </select>
        </label>
      </div>
      <button onClick={handleExecute} disabled={loading || !selectedExample || selectedStep === undefined}>
        {loading ? 'Executing...' : 'Execute Step'}
      </button>
      {result && (
        <div style={{ marginTop: 24, background: '#f4f4f4', padding: 16, borderRadius: 8 }}>
          <div><b>Result:</b> {result.result}</div>
          <div><b>Is Complete:</b> {result.isComplete ? 'Yes' : 'No'}</div>
        </div>
      )}
    </div>
  );
}

export default App;
