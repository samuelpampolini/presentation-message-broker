
import React, { useEffect, useState } from 'react';
import './App.css';
import { fetchExamples, executeStep } from './api';
import { HubConnection } from '@microsoft/signalr';
import { createSignalRConnection } from './signalr';
import { ExampleSelector } from './components/ExampleSelector';
import { StepSelector } from './components/StepSelector';
import { ResultDisplay } from './components/ResultDisplay';
import { LiveConsumerMessages } from './components/LiveConsumerMessages';
import { HistoryList } from './components/HistoryList';
import { useStepGroups } from './hooks/useStepGroups';

interface ExampleInfo {
  key: string;
  title: string;
  type: 'publisher' | 'consumer' | string;
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

const App: React.FC = () => {
  const [examples, setExamples] = useState<ExampleInfo[]>([]);
  const [selectedExample, setSelectedExample] = useState<string>('');
  const [selectedStep, setSelectedStep] = useState<number>();

  // Reset selectedStep when selectedExample changes
  useEffect(() => {
    setSelectedStep(undefined);
  }, [selectedExample]);
  const [result, setResult] = useState<ExecuteStepResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [history, setHistory] = useState<HistoryEntry[]>([]);
  const [consumerMessages, setConsumerMessages] = useState<string[]>([]);
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isConsuming, setIsConsuming] = useState(false);

  const { stepGroups } = useStepGroups();
  const selected = examples.find(e => e.key === selectedExample);
  const isConsumer = selected?.type === 'consumer';

  useEffect(() => {
    fetchExamples().then(setExamples);
  }, []);

  // Manage SignalR connection for consumer examples when consuming
  useEffect(() => {
    if (isConsuming) {
      const newConnection = createSignalRConnection();
      setConnection(newConnection);
      newConnection.start().then(() => {
        newConnection.on('ReceiveMessage', (consumerKey: string, message: string) => {
          setConsumerMessages(prev => [`${consumerKey}: ${message}`, ...prev]);
        });
      });
      return () => {
        newConnection.stop();
        setConnection(null);
      };
    }
  }, [isConsuming]);

  const handleExecute = async () => {
    if (!selectedExample || selectedStep === undefined) return;
    setLoading(true);
    setResult(null);
    setConsumerMessages([]);
    const selected = examples.find(e => e.key === selectedExample);
    const isConsumer = selected?.type === 'consumer';
    // Find the correct step group and step
    const stepGroup = stepGroups.find(g => g.type === (isConsumer ? 'consumer' : 'publisher'));
    const stepObj = stepGroup?.steps.find(s => s.value === selectedStep);
    const stepName = stepObj?.name || selectedStep.toString();

    // For consumer 'ConsumeMessages', use executeStep to start consumer, then start SignalR
    if (isConsumer && stepName === 'ConsumeMessages') {
      setIsConsuming(true);
      try {
        await executeStep(selectedExample, selectedStep);
      } catch {
        setLoading(false);
        return;
      }
      setLoading(false);
      return;
    }

    try {
      const res = await executeStep(selectedExample, selectedStep);
      setResult(res);
      const exampleTitle = selected?.title || selectedExample;
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
      setIsConsuming(false);
    } catch (e) {
      setIsConsuming(false);
    } finally {
      setLoading(false);
    }
  };

  const handleClearHistory = () => setHistory([]);

  const publisherExamples = examples.filter(ex => ex.type === 'publisher');
  const consumerExamples = examples.filter(ex => ex.type === 'consumer');
  // Only show steps for the selected example type
  const selectedStepGroup = stepGroups.find(g => g.type === (isConsumer ? 'consumer' : 'publisher'));
  const visibleSteps = selectedStepGroup ? selectedStepGroup.steps : [];

  return (
    <div className="mbp-outer-center">
      <div className="mbp-root">
        <div className="mbp-card-group">
          <div className="mbp-card">
            <h2>Publisher Examples</h2>
            <ExampleSelector
              examples={publisherExamples}
              selectedExample={selectedExample}
              setSelectedExample={setSelectedExample}
              label="Publisher Example"
              disabled={isConsumer}
            />
            <StepSelector
              steps={!isConsumer ? visibleSteps : []}
              selectedStep={selectedStep}
              setSelectedStep={setSelectedStep}
              disabled={!selectedExample || loading || isConsumer}
            />
            <button
              className="mbp-execute-btn"
              onClick={handleExecute}
              disabled={loading || !selectedExample || selectedStep === undefined || isConsumer}
            >
              {loading ? <span className="mbp-spinner"></span> : 'Execute Step'}
            </button>
            <ResultDisplay result={result && !isConsumer ? result : null} />
          </div>
          <div className="mbp-card">
            <h2>Consumer Examples</h2>
            <ExampleSelector
              examples={consumerExamples}
              selectedExample={selectedExample}
              setSelectedExample={setSelectedExample}
              label="Consumer Example"
              disabled={!isConsumer && !!selectedExample}
            />
            <StepSelector
              steps={isConsumer ? visibleSteps : []}
              selectedStep={selectedStep}
              setSelectedStep={setSelectedStep}
              disabled={!selectedExample || loading || !isConsumer}
            />
            <button
              className="mbp-execute-btn"
              onClick={handleExecute}
              disabled={loading || !selectedExample || selectedStep === undefined || !isConsumer}
            >
              {loading ? <span className="mbp-spinner"></span> : 'Consume Messages'}
            </button>
            {isConsuming && <LiveConsumerMessages messages={consumerMessages} />}
          </div>
        </div>
        <HistoryList history={history} onClear={handleClearHistory} />
      </div>
    </div>
  );
};

export default App;
