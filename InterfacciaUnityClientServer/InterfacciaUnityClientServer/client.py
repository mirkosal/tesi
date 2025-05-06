import asyncio
import websockets
import json
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

async def visualize_hand_data():
    uri = "ws://localhost:8080"  # WebSocket server URI
    async with websockets.connect(uri) as websocket:
        # Initialize the plot outside the loop
        plt.ion()  # Turn on interactive mode
        fig = plt.figure()
        ax = fig.add_subplot(111, projection='3d')
        ax.set_xlabel('X Label')
        ax.set_ylabel('Y Label')
        ax.set_zlabel('Z Label')
        ax.view_init(elev=0, azim=-90)

        while True:  # Keep listening for data indefinitely
            data = await websocket.recv()
            hand_data = json.loads(data)
            
            # Clear previous data points
            ax.cla()
            ax.set_xlabel('X Label')
            ax.set_ylabel('Y Label')
            ax.set_zlabel('Z Label')

            # Process and plot new data
            for finger in ['thumb', 'index', 'middle', 'finger', 'pinky']:
                for joint in ['BTIP', 'DIP', 'PIP', 'MCP']:
                    joint_name = f"{finger}{joint}"
                    if joint_name in hand_data:
                        #x, y, z = hand_data[joint_name]
                        #print(hand_data[joint_name])
                        x = float(hand_data[joint_name]["x"])
                        y = float(hand_data[joint_name]["y"])
                        z = float(hand_data[joint_name]["z"])
                        

                        ax.scatter((x), (y), (z), marker='o')

            # This is necessary to ensure the plot is updated
            plt.draw()
            plt.pause(0.001)  # Pause to ensure the plot gets updated visually

        plt.ioff()  # Turn off interactive mode

# Run the WebSocket client and visualization
asyncio.get_event_loop().run_until_complete(visualize_hand_data())
