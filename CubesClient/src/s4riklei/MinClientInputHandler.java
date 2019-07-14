package s4riklei;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.Socket;
import java.net.SocketException;

public class MinClientInputHandler extends Thread {

    private String name;
    private Socket clientSocket;
    private BufferedReader input;
    private MinClientOutputHandler outputHandler;

    public MinClientInputHandler(String name, Socket socket, MinClientOutputHandler outputHandler) {
        this.name = name;
        this.clientSocket = socket;
        this.outputHandler = outputHandler;
    }

    private void handleServerResponse() {
        try {
            this.input = new BufferedReader(new InputStreamReader(clientSocket.getInputStream()));

            String rawInput;
            do {
                try {
                    rawInput = input.readLine();
                    if (rawInput == null) {
                        break;
                    }

                    if (rawInput.matches("^.*\"type\"\\s*:\\s*\"welcome\".*$")) {
                        String positionRaw = rawInput.replace("{", "").replace("}", "").
                                replaceAll("\"type\"\\s*:\\s*\"welcome\"\\s*", "").replaceAll("\\s*\"position\"\\s*:\\s*", "").
                                replace("\"", "").replace(",", " ").trim();

                        System.out.println(name + ": Receiving positions: " + positionRaw);

                        String[] positionRawArray = positionRaw.split(" ");

                        try {
                            float x = Float.parseFloat(positionRawArray[0]);
                            float y = Float.parseFloat(positionRawArray[1]);
                            float z = Float.parseFloat(positionRawArray[2]);
                            this.outputHandler.initializePositionData(x, y, z);
                        } catch (Exception e) {
                            System.out.println(name + ": An Exception occurred while parsing position data. Terminating connection...");
                            e.printStackTrace();
                            break;
                        }
                    }

                    if (rawInput.matches("^.*\"type\"\\s*:\\s*\"end\".*$")) {
                        break;
                    }
                } catch (SocketException e) {
                    break;
                }

            } while (true);

            System.out.println(name + ": Server terminated connection");
            outputHandler.interrupt();
            this.input.close();
            this.clientSocket.close();

        } catch (IOException | NullPointerException e) {
            System.out.print(name + ": An Exception occurred while handling server responses. Terminating Thread...");
            outputHandler.interrupt();
            e.printStackTrace();
        }
    }

    @Override
    public void run() {
        handleServerResponse();
    }

}
