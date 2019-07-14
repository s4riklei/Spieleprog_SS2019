package oxoo2a;

import java.io.*;
import java.net.Socket;
import java.net.SocketException;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

public class ClientConnect {
    public ClientConnect ( Socket client, Map<String,ClientConnect> namedClients, float[] position ) {
        this.client = client;
        this.posX = position[0];
        this.posY = position[1];
        this.posZ = position[2];
        name = "John Doe - " + this.hashCode();
        synchronized (namedClients) {
            this.namedClients = namedClients;
            namedClients.put(name,this);
        }

        input = null;
        output = null;

        inputThread = new Thread(this::handleInput);
        inputThread.start();
    }

    public String getPosition () {
        return Float.toString(posX) + ',' + posY + ',' + posZ;
    }

    private void handleHelloMessage ( Message m ) {
        // TODO Maybe it is not the first hello message from client
        Optional<String> trueName = m.get("name");
        if (trueName.isPresent()) {
            synchronized (namedClients) {
                namedClients.remove(name);
                namedClients.put(trueName.get(),this); // TODO Name might be in map already
            }
            name = trueName.get();
        }
        else
            System.out.printf("In message <%s> is no name defined\n",m.toString());
        Message welcome = Message.createWelcomeMessage(this.getPosition());
        deliverMessage(welcome);
        this.handleGetStateMessage();

        Message newcomer = Message.createNewcomerMessage(this.getPosition(), name);
        synchronized(namedClients) {
            Collection<ClientConnect> clients = namedClients.values();
            for (ClientConnect c : clients) {
                if (c != this) {
                    c.deliverMessage(newcomer);
                }
            }
        }
    }

    private void handleChatMessage ( Message m ) {
        Optional<String> receiver = m.get("receiver");
        Optional<String> content = m.get("content");
        if ((receiver.isPresent()) && (content.isPresent())) {
            Message answer = Message.createChatMessage(name,content.get(),false);
            if (receiver.get().equalsIgnoreCase("world")) {
                answer.set("world","true");
                synchronized (namedClients) {
                    Collection<ClientConnect> clients = namedClients.values();
                    for (ClientConnect c : clients) {
                        c.deliverMessage(answer);
                    }
                }
            }
            else {
                synchronized(namedClients) {
                    ClientConnect destination = namedClients.get(receiver.get());

                    if (destination != null)
                        destination.deliverMessage(answer);
                    else
                        System.out.printf("Unknown receiver <%s>; ignoring chat message\n", receiver.get());
                }
            }
        }
        else
            System.out.printf("Chat message <%s> has no receiver and/or content\n",m.toString());
    }

    private void handleGetStateMessage () {
        Map<String,String> positions = new HashMap<>();
        synchronized(namedClients) {
            Collection<ClientConnect> clients = namedClients.values();
            for (ClientConnect c : clients) {
                positions.put(c.name, c.getPosition());
            }
        }
        Optional<String> positionsAsJSON = JSONProcessor.serialize(positions);
        if (positionsAsJSON.isEmpty()) {
            System.out.printf("Failed to serialize positions\n");
            return;
        }
        Message state = Message.createStateMessage(positionsAsJSON.get());
        deliverMessage(state);
    }

    private void handleBlockActionMessage (Message m) {
        Optional<String> blockID = m.get("blockID");
        Optional<String> position = m.get("position");
        if ((blockID.isPresent()) && (position.isPresent())) {
            Message blockActionMessage = Message.createBlockActionMessage(blockID.get(), position.get());
            synchronized (namedClients) {
                Collection<ClientConnect> clients = namedClients.values();
                for (ClientConnect c : clients) {
                    if (c != this) {
                        c.deliverMessage(blockActionMessage);
                    }
                }
            }
        } else {
            System.out.printf("Block Action message <%s> has no blockID and/or position\n",m.toString());
        }
    }

    private void handlePositionUpdateMessage (Message m) {
        Optional<String> name = m.get("name");
        Optional<String> position = m.get("position");
        if ((name.isPresent()) && (position.isPresent())) {
            Message positionUpdateMessage = Message.createPositionUpdateMessage(name.get(), position.get());
            synchronized (namedClients) {
                Collection<ClientConnect> clients = namedClients.values();
                for (ClientConnect c : clients) {
                    if (c != this) {
                        c.deliverMessage(positionUpdateMessage);
                    }
                }
            }
            try {
                String[] positionRaw = position.get().split(",");
                this.posX = Float.parseFloat(positionRaw[0]);
                this.posY = Float.parseFloat(positionRaw[1]);
                this.posZ = Float.parseFloat(positionRaw[2]);
            } catch (Exception e) {
                e.printStackTrace();
            }
        } else {
            System.out.printf("Position Update message <%s> has no name and/or position\n",m.toString());
        }
    }

    private void handlePingMessage() {
        Message ping = Message.createPingMessage();
        this.deliverMessage(ping);
    }

    private void handleInput () {
        try {
            input = new BufferedReader(new InputStreamReader(client.getInputStream()));
            String rawInput;
            do {
                try {
                    rawInput = input.readLine();
                    if (rawInput == null) {
                        System.out.printf("Client %s left!\n", name);
                        Message disconnect = Message.createDisconnectMessage(this.name);
                        synchronized (namedClients) {
                            Collection<ClientConnect> clients = namedClients.values();
                            for (ClientConnect c : clients) {
                                if (c != this) {
                                    c.deliverMessage(disconnect);
                                }
                            }
                        }
                        break;
                    }
                    System.out.printf("Receiving <%s> from %s\n", rawInput, name);
                    Optional<Message> mOptional = Message.deserialize(rawInput);
                    if (mOptional.isEmpty()) {
                        System.out.printf("Ignoring message <%s> (unable to deserialize)\n", rawInput);
                        continue;
                    }
                    Message m = mOptional.get();
                    Optional<String> messageType = m.get("type");
                    if (messageType.isEmpty()) {
                        System.out.printf("There is no type field in message <%s> from %s; ignoring\n", rawInput, name);
                        continue;
                    }
                    messageType.ifPresent(t -> {
                        if (t.equalsIgnoreCase("hello"))
                            handleHelloMessage(m);
                        else if (t.equalsIgnoreCase("chat"))
                            handleChatMessage(m);
                        else if (t.equalsIgnoreCase("getState"))
                            handleGetStateMessage();
                        else if (t.equalsIgnoreCase("blockAction"))
                            handleBlockActionMessage(m);
                        else if (t.equalsIgnoreCase("positionUpdate"))
                            handlePositionUpdateMessage(m);
                        else if (t.equalsIgnoreCase("ping"))
                            handlePingMessage();
                        else
                            System.out.printf("Chat message <%s> has no receiver and/or content\n", m.toString());
                    });
                } catch (SocketException e) {
                    System.out.printf("Connection to client <%s> lost...\n", this.name);
                    Message disconnect = Message.createDisconnectMessage(this.name);
                    synchronized(namedClients) {
                        Collection<ClientConnect> clients = namedClients.values();
                        for (ClientConnect c : clients) {
                            if (c != this) {
                                c.deliverMessage(disconnect);
                            }
                        }
                    }
                    break;
                }
            } while (true);
            client.close();
            synchronized (namedClients) {
                namedClients.remove(name);
            }
        }
        catch (IOException e) {
            Main.fatal("There was an IOException while receiving data ...",e);
        }

    }

    public synchronized void deliverMessage ( Message m ) {
        if (output == null) {
            try {
                output = new BufferedWriter(new OutputStreamWriter(client.getOutputStream()));
            }
            catch (IOException e) {
                Main.fatal("Unable to open output stream",e);
            }
        }
        Optional<String> rawOutputOptional = m.serialize();
        if (rawOutputOptional.isEmpty()) {
            System.out.printf("Discarding message <%s> (error while serializing)\n",m.toString());
            return;
        }
        String rawOutput = rawOutputOptional.get();
        System.out.printf("Sending <%s> to client %s\n",rawOutput,name);
        try {
            output.write(rawOutput);
            output.newLine();
            output.flush();
        }
        catch (IOException e) {
            System.out.printf("<%s>: IOException while sending message", name);
            synchronized (namedClients) {
                namedClients.remove(name);
            }
            try {
                this.client.close();
            } catch (IOException e2) {
                System.out.printf("<%s>: IOException while closing socket after IOException while sending message", name);
            }
        }
    }

    private Socket client;
    private float posX;
    private float posY;
    private float posZ;
    private Map<String,ClientConnect> namedClients;
    private Thread inputThread;
    private BufferedReader input;
    private BufferedWriter output;
    private String name;
}
