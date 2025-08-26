import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';

// Use VITE_SIGNALR_URL from .env, fallback to http://localhost:5042
const SIGNALR_URL = import.meta.env.VITE_SIGNALR_URL || 'http://localhost:5042';

export function createSignalRConnection(): HubConnection {
    return new HubConnectionBuilder()
        .withUrl(`${SIGNALR_URL}/messageHub`)
        .withAutomaticReconnect()
        .build();
}
