import sys
import websocket
import json
import rel

# Define la función que se ejecutará al recibir un mensaje del WebSocket
def on_message(ws, message):
    print(" - Mensaje entrante -")
    # print(message)
    # Parsear el mensaje JSON
    message_data = json.loads(message)
    # Extraer la información del mensaje
    message_id = message_data["Id"]
    message_type = message_data["Type"]
    sender = message_data["From"]
    receiver = message_data["To"]
    content = message_data["Data"]

    # Imprimir la información del mensaje
    print(f"Mensaje ID: {message_id}, Tipo: {message_type}, De: {sender}, Para: {receiver}, Contenido: {content}")

# Define la función que se ejecutará al ocurrir un error
def on_error(ws, error):
    print("Error:", error)

# Define la función que se ejecutará al abrir la conexión
def on_open(ws):
    print("Conexión abierta")

# Define la función que se ejecutará al cerrar la conexión
def on_close(ws, close_status_code, close_msg):
    print("Conexión cerrada -> " + close_msg)

# URL del WebSocket al que deseas conectarte
websocket_url = "ws://localhost:5168/ws?from="

if __name__ == "__main__":
    if len(sys.argv) < 4:
        print("Por favor, ingrese el nombre del usuario, nombre del usuario a enviar el mensaje y mensaje.")
        print("python test.py [usuario] [usuario-receptor] [TXT/BIN] [mensaje]")
        sys.exit(1)
    
    usuario = sys.argv[1]
    to = sys.argv[2]
    mode = sys.argv[3]
    data = " ".join(sys.argv[4:])

    # Creación de la instancia del WebSocket
    ws = websocket.WebSocketApp(
        websocket_url + usuario,
        on_message=on_message,
        on_error=on_error,
        on_close=on_close,
        on_open=on_open,
    )

    # Iniciar la conexión al WebSocket
    # Setea el dispatcher para reconexion automatica cada 5 segundos
    ws.run_forever(dispatcher=rel, reconnect=5)
    # Crear el mensaje JSON para enviar

    message_to_send = {
        "Id": 1,
        "Type": "CHAT",
        "From": usuario,
        "To": to,
        "Data": data
    }
    
    # Convertir el mensaje a formato JSON o binario y enviarlo
    if mode == "TXT":
        ws.send(json.dumps(message_to_send))
    else:
        binary_message = bytes("@*" + usuario + "@*" + to + "@*" + data, 'utf-8')
        ws.send(binary_message, websocket.ABNF.OPCODE_BINARY)
    
    # Keyboard Interrupt
    rel.signal(2, rel.abort)
    rel.dispatch()