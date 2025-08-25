import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";

export function createSignalRConnection() {
    const connection = new HubConnectionBuilder()
        .withUrl("/messageHub")
        .withAutomaticReconnect()
        .build();
    return connection;
}
