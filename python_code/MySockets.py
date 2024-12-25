import socket

class SocketServer:
    def __init__(self, host='127.0.0.1', port=65432):
        """
        Initializes the server with the specified host and port.
        """
        self.host = host
        self.port = port
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.conn = None  # Initialize conn as None

    def start(self):
        """
        Starts the server and waits for a connection.
        """
        self.server_socket.bind((self.host, self.port))
        self.server_socket.listen(1)
        print(f"Server listening on {self.host}:{self.port}")
        self.conn, self.addr = self.server_socket.accept()
        print(f"Connected by {self.addr}")

    def send(self, data):
        """
        Sends a string to the connected client.
        """
        if self.conn:
            try:
                self.conn.sendall(data.encode())
            except (BrokenPipeError, OSError) as e:
                print(f"Error sending data: {e}")
                self.conn = None

    def receive(self):
        """
        Receives a string from the connected client.
        """
        if self.conn:
            try:
                data = self.conn.recv(1024)
                if data:
                    return data.decode()
            except (ConnectionResetError, OSError) as e:
                print(f"Error receiving data: {e}")
                self.conn = None
        return None

    def stop(self):
        """
        Closes the connection and the server socket.
        """
        if self.conn:
            self.conn.close()
            self.conn = None
        self.server_socket.close()
        print("Server stopped.")

# Example Usage:
if __name__ == "__main__":
    server = SocketServer()
    try:
        server.start()
        while True:
            received = server.receive()
            if received:
                print(f"Received: {received}")
                # Echo the received message back
                if received == "END":
                    print("Ending connection.")
                    server.stop()
                    break
                server.send(f"Echo: {received}")
            else:
                print("No message received. Waiting...")
    except KeyboardInterrupt:
        print("\nShutting down server.")
    finally:
        server.stop()
