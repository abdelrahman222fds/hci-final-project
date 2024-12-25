import threading
from pythonosc import dispatcher, osc_server
import time

class TUIOListener:
    def __init__(self, ip='127.0.0.1', port=3333):
        """
        Initializes the TUIO listener.
        :param ip: IP address to listen for TUIO messages (default is '127.0.0.1').
        :param port: Port to listen for TUIO messages (default is 3333).
        """
        self.ip = ip
        self.port = port
        self.tuio_data = []

        # Dispatcher for handling OSC messages
        self.dispatcher = dispatcher.Dispatcher()
        self.dispatcher.map("/tuio/2Dobj", self.handle_tuio_object)
        self.dispatcher.map("/tuio/2Dcur", self.handle_tuio_cursor)

    def handle_tuio_object(self, address, *args):
        """
        Callback function to handle TUIO Object messages.
        :param address: OSC message address.
        :param args: Data received from the message.
        """
        obj_type = args[0]
        if obj_type == "set":
            # Parse "set" messages with meaningful names
            parsed_data = {
                "type": "object",
                "session_id": args[1],
                "class_id": args[2],
                "x_pos": args[3],
                "y_pos": args[4],
                "angle": args[5],
                "x_speed": args[6],
                "y_speed": args[7],
                "rotation_speed": args[8],
                "motion_acceleration": args[9],
                "rotation_acceleration": args[10],
            }
            self.tuio_data.append(parsed_data)
        elif obj_type == "alive":
            # Parse "alive" messages for currently tracked IDs
            alive_ids = args[1:]
            self.tuio_data.append({"type": "alive", "object_ids": alive_ids})
        elif obj_type == "fseq":
            # Frame sequence number
            self.tuio_data.append({"type": "fseq", "frame_sequence": args[1]})

    def handle_tuio_cursor(self, address, *args):
        """
        Callback function to handle TUIO Cursor messages.
        :param address: OSC message address.
        :param args: Data received from the message.
        """
        # Add cursor handling if needed
        pass 

    def start_server(self):
        """
        Starts the OSC server to listen for TUIO messages.
        """
        server = osc_server.ThreadingOSCUDPServer((self.ip, self.port), self.dispatcher)
        print(f"Listening for TUIO messages on {self.ip}:{self.port}...")
        server.serve_forever()

    def start_server_thread(self):
        """
        Starts the OSC server in a separate thread.
        """
        server_thread = threading.Thread(target=self.start_server)
        server_thread.daemon = True
        server_thread.start()

    def get_tuio_data(self):
        """
        Returns the collected TUIO data as a dictionary.
        :return: List of dictionaries containing parsed TUIO data.
        """
        # Return data in a processed format
        return self.tuio_data


if __name__ == "__main__":
    tuio_listener = TUIOListener(ip='127.0.0.1', port=3333)
    tuio_listener.start_server_thread()

    try:
        while True:
            
            data = tuio_listener.get_tuio_data()
            if data:
                print("TUIO Data:")
                for entry in data:
                    if entry["type"] == "object":
                        print(
                            f"Object: ID={entry['class_id']}, Position=({entry['x_pos']}, {entry['y_pos']}), "
                            f"Angle={entry['angle']}, Speed=({entry['x_speed']}, {entry['y_speed']}), "
                            f"Accel={entry['motion_acceleration']}, RotationAccel={entry['rotation_acceleration']}"
                        )
                    elif entry["type"] == "alive":
                        print(f"Alive: IDs={entry['object_ids']}")
                    elif entry["type"] == "fseq":
                        print(f"Frame Sequence: {entry['frame_sequence']}")
                print("\n")
                time.sleep(0.5)  # Delay for readability
            else:
                print("No TUIO data received.")
    except KeyboardInterrupt:
        print("Exiting...")
