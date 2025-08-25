import React, { useEffect, useState } from 'react';
import './App.css';
import { fetchExamples, fetchSteps, executeStep } from './api';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';

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


interface HistoryEntry {
  exampleKey: string;
  exampleTitle: string;
  stepValue: number;
  stepName: string;
  result: string;
  isComplete: boolean;
  timestamp: number;
}

function App() {
  const [examples, setExamples] = useState<ExampleInfo[]>([]);
  const [steps, setSteps] = useState<StepInfo[]>([]);
  const [selectedExample, setSelectedExample] = useState<string>('');
  const [selectedStep, setSelectedStep] = useState<number>();
  const [result, setResult] = useState<ExecuteStepResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [history, setHistory] = useState<HistoryEntry[]>([]);
  const [consumerMessages, setConsumerMessages] = useState<string[]>([]);
  const [connection, setConnection] = useState<HubConnection | null>(null);

  useEffect(() => {
    fetchExamples().then(setExamples).catch(e => setError(e.message));
    fetchSteps().then(setSteps).catch(e => setError(e.message));
    // Connect to SignalR for Consumers only
    const newConnection = new HubConnectionBuilder()
      .withUrl('/messageHub')
      .withAutomaticReconnect()
      .build();
    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection.start().then(() => {
        connection.on('ReceiveMessage', (consumerKey: string, message: string) => {
          setConsumerMessages(prev => [`${consumerKey}: ${message}`, ...prev]);
        });
      });
    }
  }, [connection]);


  const handleExecute = async () => {
    if (!selectedExample || selectedStep === undefined) return;
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      const res = await executeStep(selectedExample, selectedStep);
      setResult(res);
      // Add to history
      const exampleTitle = examples.find(e => e.key === selectedExample)?.title || selectedExample;
      const stepName = steps.find(s => s.value === selectedStep)?.name || selectedStep.toString();
      setHistory(prev => [
        {
          exampleKey: selectedExample,
          exampleTitle,
          stepValue: selectedStep,
          stepName,
          result: res.result,
          isComplete: res.isComplete,
          timestamp: Date.now(),
        },
        ...prev
      ]);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  const handleClearHistory = () => setHistory([]);

  return (
    <div className="mbp-outer-center">
      <div className="mbp-root">
        <div className="mbp-card-group">
          <div className="mbp-card">
            <h2>Publisher Examples</h2>
            {/* Add publisher controls here */}
            <div className="mbp-form-row">
              <label className="mbp-label">Example:</label>
              <select className="mbp-select" value={selectedExample} onChange={e => setSelectedExample(e.target.value)}>
                <option value="">Select publisher example</option>
                {examples.filter(ex => ex.key.startsWith('P')).map(ex => (
                  <option key={ex.key} value={ex.key}>{ex.title} ({ex.key})</option>
                ))}
              </select>
            </div>
            <div className="mbp-form-row">
              <label className="mbp-label">Step:</label>
              <div className="mbp-stepper">
                {steps.map(st => (
                  <button
                    key={st.value}
                    className={`mbp-step-btn${selectedStep === st.value ? ' mbp-step-btn-active' : ''}`}
                    onClick={() => setSelectedStep(st.value)}
                    disabled={!selectedExample || loading}
                  >
                    {st.name}
                  </button>
                ))}
              </div>
            </div>
            <button
              className="mbp-execute-btn"
              onClick={handleExecute}
              disabled={loading || !selectedExample || selectedStep === undefined}
            >
              {loading ? <span className="mbp-spinner"></span> : 'Execute Step'}
            </button>
            {result && (
              <div className="mbp-result">
                <div><b>Result:</b> {result.result}</div>
                <div><b>Is Complete:</b> {result.isComplete ? 'Yes' : 'No'}</div>
              </div>
            )}
          </div>
          <div className="mbp-card">
            <h2>Consumer Examples</h2>
            {/* Add consumer controls here */}
            <div className="mbp-form-row">
              <label className="mbp-label">Example:</label>
              <select className="mbp-select" value={selectedExample} onChange={e => setSelectedExample(e.target.value)}>
                <option value="">Select consumer example</option>
                {examples.filter(ex => ex.key.startsWith('C')).map(ex => (
                  <option key={ex.key} value={ex.key}>{ex.title} ({ex.key})</option>
                ))}
              </select>
            </div>
            <div className="mbp-form-row">
              <label className="mbp-label">Step:</label>
              <div className="mbp-stepper">
                {steps.map(st => (
                  <button
                    key={st.value}
                    className={`mbp-step-btn${selectedStep === st.value ? ' mbp-step-btn-active' : ''}`}
                    onClick={() => setSelectedStep(st.value)}
                    disabled={!selectedExample || loading}
                  >
                    {st.name}
                  </button>
                ))}
              </div>
            </div>
            <button
              className="mbp-execute-btn"
              onClick={handleExecute}
              disabled={loading || !selectedExample || selectedStep === undefined}
            >
              {loading ? <span className="mbp-spinner"></span> : 'Execute Step'}
            </button>
            {result && (
              <div className="mbp-result">
                <div><b>Result:</b> {result.result}</div>
                <div><b>Is Complete:</b> {result.isComplete ? 'Yes' : 'No'}</div>
              </div>
            )}
            <div className="mbp-history-card">
              <div className="mbp-history-header">
                <h2 className="mbp-history-title">Live Consumer Messages</h2>
              </div>
              {consumerMessages.length === 0 ? (
                <div className="mbp-history-empty">No consumer messages yet.</div>
              ) : (
                <ul className="mbp-history-list">
                  {consumerMessages.map((msg, idx) => (
                    <li key={idx} className="mbp-history-item">{msg}</li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        </div>
        {/* History Section */}
        <div className="mbp-history-card">
          <div className="mbp-history-header">
            <h2 className="mbp-history-title">Execution History</h2>
            <button className="mbp-clear-btn" onClick={handleClearHistory} disabled={history.length === 0}>Clear History</button>
          </div>
          {history.length === 0 ? (
            <div className="mbp-history-empty">No executions yet.</div>
          ) : (
            <ul className="mbp-history-list">
              {history.map((h, idx) => (
                <li key={h.timestamp + '-' + idx} className="mbp-history-item">
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
      </div>
    </div>
  );
}

export default App;
