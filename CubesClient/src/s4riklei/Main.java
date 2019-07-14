package s4riklei;

import java.io.IOException;
import java.net.Socket;

public class Main {

    public static void main(String[] args) {
        if (args.length != 5) {
            System.out.println("Usage: cubes_client <host> <port> <client position update frequency (in ms)> <client block placement frequency (in ms)> <client amount>");
            System.exit(-1);
        }

        try {
            String host = args[0];
            int port = Integer.parseInt(args[1]);
            int positionDelay = Integer.parseInt(args[2]);
            int blockDelay = Integer.parseInt(args[3]);
            int clientAmount = Integer.parseInt(args[4]);

            for (int i = 1; i <= clientAmount; i++) {
                new MinClientOutputHandler(positionDelay, blockDelay, ("bot" + i), new Socket(host, port));
            }
        } catch (Exception e) {
            System.out.println("An Exception occurred while trying to connect to the server");
            e.printStackTrace();
        }
    }

}
