package s4riklei;

import java.io.BufferedWriter;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.net.Socket;

public class MinClientOutputHandler extends Thread {

    private float speed = 0.005f;
    private int positionDirection = 1;

    private int blockDirection = 1;
    private long lastBlockMessageTime = 0;

    private int delayBetweenPositionMessages;
    private int delayBetweenBlockMessages;

    private int currentBlockID = 0;

    private float blockX;
    private float blockY;
    private float blockZ;

    private float blockInitX;
    private float blockInitZ;

    private float blockMaxX;
    private float blockMaxZ;


    private float posX;
    private float posY;
    private float posZ;

    private float initX;
    private float initZ;

    private float xMax;
    private float zMax;

    String name;
    private Socket clientSocket;
    private BufferedWriter output;

    MinClientInputHandler inputHandler;

    public MinClientOutputHandler(int positionDelay, int blockDelay, String name, Socket socket) {
        this.delayBetweenPositionMessages = positionDelay;
        this.delayBetweenBlockMessages = blockDelay;
        this.name = name;
        this.clientSocket = socket;

        this.initializeInputHandler();
        this.sendHelloMessage();
        System.out.println(name + ": Instantiated client connection. Waiting for server welcome...");
    }

    public void initializePositionData(float x, float y, float z) {
        this.posX = x;
        this.posY = y;
        this.posZ = z;

        this.initX = x;
        this.initZ = z;

        this.xMax = this.initX + 20.f;
        this.zMax = this.initZ + 20.f;

        this.blockX = Math.round(this.posX) + 1.5f;
        this.blockY = Math.round(this.posY) + 1.5f;
        this.blockZ = Math.round(this.posZ) + 1.5f;

        this.blockInitX = this.blockX;
        this.blockInitZ = this.blockZ;

        this.blockMaxX = Math.round(this.xMax) - 1.5f;
        this.blockMaxZ = Math.round(this.zMax) - 1.5f;

        System.out.println(name + ": Initialized positions. Starting routine...");
        this.start();
    }

    private void sendHelloMessage() {
        String hello = "{ \"type\" : \"hello\", \"name\" : \"" + this.name + "\" }";
        this.deliverMessage(hello);
    }

    private void initializeInputHandler() {
        this.inputHandler = new MinClientInputHandler(this.name, this.clientSocket, this);
        this.inputHandler.start();
    }

    private void sendPositionUpdate() {
        switch (positionDirection) {
            case 1:
                if ((this.posX + this.delayBetweenPositionMessages *this.speed) >= this.xMax) {
                    this.posX = this.xMax;
                    this.positionDirection = 2;
                } else {
                    this.posX = this.posX + this.delayBetweenPositionMessages *this.speed;
                }
                break;
            case 2:
                if ((this.posZ + this.delayBetweenPositionMessages *this.speed) >= this.zMax) {
                    this.posZ = this.zMax;
                    this.positionDirection = 3;
                } else {
                    this.posZ = this.posZ + this.delayBetweenPositionMessages *this.speed;
                }
                break;
            case 3:
                if ((this.posX - this.delayBetweenPositionMessages *this.speed) <= this.initX) {
                    this.posX = this.initX;
                    this.positionDirection = 4;
                } else {
                    this.posX = this.posX - this.delayBetweenPositionMessages *this.speed;
                }
                break;
            case 4:
                if ((this.posZ - this.delayBetweenPositionMessages *this.speed) <= this.initZ) {
                    this.posZ = this.initZ;
                    this.positionDirection = 1;
                } else {
                    this.posZ = this.posZ - this.delayBetweenPositionMessages *this.speed;
                }
                break;
        }

        String posUpdate = "{ \"type\" : \"positionUpdate\", \"name\" : \"" + this.name + "\", \"position\" : \"" + this.posX + "," + this.posY + "," + this.posZ + "\" }";
        this.deliverMessage(posUpdate);
    }

    private void sendBlockUpdate() {
        if ((System.currentTimeMillis() - this.lastBlockMessageTime) >= this.delayBetweenBlockMessages) {
            switch(blockDirection) {
                case 1:
                    if ((this.blockX + 1.f) >= this.blockMaxX) {
                        this.blockX = this.blockMaxX;
                        this.blockDirection = 2;
                    } else {
                        this.blockX = this.blockX + 1.f;
                    }
                    break;
                case 2:
                    if ((this.blockZ + 1.f) >= this.blockMaxZ) {
                        this.blockZ = this.blockMaxZ;
                        this.blockDirection = 3;
                    } else {
                        this.blockZ = this.blockZ + 1.f;
                    }
                    break;
                case 3:
                    if ((this.blockX - 1.f) <= this.blockInitX) {
                        this.blockX = this.blockInitX;
                        this.blockDirection = 4;
                    } else {
                        this.blockX = this.blockX - 1.f;
                    }
                    break;
                case 4:
                    if ((this.blockZ - 1.f) <= this.blockInitZ) {
                        this.blockZ = this.blockInitZ;
                        this.blockDirection = 1;
                    } else {
                        this.blockZ = this.blockZ - 1.f;
                    }
                    break;
            }

            if (++this.currentBlockID > 4) {
                this.currentBlockID = 0;
            }

            String posUpdate = "{ \"type\" : \"blockAction\", \"blockID\" : \"" + this.currentBlockID + "\", \"position\" : \"" + this.blockX + "," + this.blockY + "," + this.blockZ + "\" }";
            this.deliverMessage(posUpdate);
            this.lastBlockMessageTime = System.currentTimeMillis();
        }
    }

    private synchronized void deliverMessage(String message) {
        if (this.output == null) {
            try {
                this.output = new BufferedWriter(new OutputStreamWriter(clientSocket.getOutputStream()));
            } catch (IOException e) {
                System.out.println(name + ": Unable to open output stream");
            }
        }
        try {
            output.write(message);
            output.newLine();
            output.flush();
        } catch (IOException e) {
            System.out.println(name + ": IOException occurred while sending message");
        }
    }

    private void handleClientActions() throws InterruptedException{
        this.sendPositionUpdate();
        this.sendBlockUpdate();
        Thread.sleep(delayBetweenPositionMessages);
    }

    @Override
    public void run() {
        try {
            while (!Thread.currentThread().isInterrupted()) {
                try {

                    handleClientActions();

                } catch (InterruptedException e) {
                    break;
                }
            }
            this.output.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

}
