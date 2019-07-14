package oxoo2a;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.util.Optional;

public class JSONProcessor {
    public static <T> Optional<String> serialize ( T o ) {
        Optional<String> result = Optional.empty();
        try {
            String json = mapper.writeValueAsString(o);
            result = Optional.of(json);
        }
        catch (JsonProcessingException e) {
            System.out.printf("Unable to serialize object of type <%s> to JSON\n",o.getClass().toString());
            System.out.println(e.getMessage());
        }
        return result;
    }

    public static <T> Optional<T> deserialize ( String json, Class<T> blueprint ) {
        Optional<T> t = Optional.empty();
        try {
            T obj = mapper.readValue(json, blueprint);
            t = Optional.of(obj);
        }
        catch (Exception e) {
            System.out.printf("Unable to deserialize <%s> to object of type <%s>\n",json,blueprint.toString());
            System.out.println(e.getMessage());
        };
        return t;
    }

    private static ObjectMapper mapper = new ObjectMapper();
}
