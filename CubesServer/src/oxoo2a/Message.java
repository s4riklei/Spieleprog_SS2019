package oxoo2a;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

public class Message {
    public Message () {
        data = new HashMap<>();
    }

    public void set (String key, String value ) {
        data.put(key,value);
    }

    public Optional<String> get(String key ) {
        String v = data.get(key);
        if (v == null)
            return Optional.empty();
        else
            return Optional.of(v);
    }

    public Optional<String> serialize() {
        return JSONProcessor.serialize(data);
    }

    @SuppressWarnings("unchecked")
    public static Optional<Message> deserialize(String raw ) {
        Message m = new Message();
        var obj = JSONProcessor.deserialize(raw,m.data.getClass());
        if (obj.isPresent()) {
            m.data = obj.get();
            return Optional.of(m);
        }
        else
            return Optional.empty();
    }

    public static Message createWelcomeMessage ( String coordinate ) {
        Message m = new Message();
        m.set("type","welcome");
        m.set("position",coordinate);
        return m;
    }

    public static Message createNewcomerMessage ( String coordinate, String name ) {
        Message m = new Message();
        m.set("type","newcomer");
        m.set("name", name);
        m.set("position",coordinate);
        return m;
    }

    public static Message createChatMessage ( String sender, String content, boolean isWorldMessage ) {
        Message m = new Message();
        m.set("type","chat");
        m.set("sender",sender);
        m.set("content",content);
        m.set("world", isWorldMessage ? "true" : "false");
        return m;
    }

    public static Message createStateMessage ( String positions ) {
        Message m = new Message();
        m.set("type","state");
        m.set("positions",positions);
        return m;
    }

    public static Message createDisconnectMessage ( String name ) {
        Message m = new Message();
        m.set("type","disconnect");
        m.set("name",name);
        return m;
    }

    public static Message createBlockActionMessage ( String blockID, String position ) {
        Message m = new Message();
        m.set("type","blockAction");
        m.set("blockID",blockID);
        m.set("position",position);
        return m;
    }

    public static Message createPositionUpdateMessage ( String name, String position ) {
        Message m = new Message();
        m.set("type","positionUpdate");
        m.set("name",name);
        m.set("position",position);
        return m;
    }

    public static Message createPingMessage () {
        Message m = new Message();
        m.set("type","ping");
        return m;
    }


    public static Message createEndMessage () {
        Message m = new Message();
        m.set("type","end");
        return m;
    }

    private Map<String,String> data;
}
