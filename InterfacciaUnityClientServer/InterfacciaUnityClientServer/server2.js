const { MongoClient } = require('mongodb');
const WebSocket = require('ws');

// Connection URL
const url = 'mongodb://localhost:27017';
const client = new MongoClient(url);

// Database Name
const dbName = 'tesi';

// Set up a WebSocket server
const wss = new WebSocket.Server({ port: 8080 });
console.log('WebSocket server started on ws://localhost:8080');

function pause(milliseconds) {
    const date = Date.now();
    let currentDate = null;
    do {
      currentDate = Date.now();
    } while (currentDate - date < milliseconds);
  }

// Function to broadcast the hand data
function broadcastHandData(handData) {
    wss.clients.forEach(function each(client) {
        if (client.readyState === WebSocket.OPEN) {
            console.log(JSON.stringify(handData));
            client.send(JSON.stringify(handData));
             // Pause for 2000 milliseconds (2 seconds)

        }
    });
}

// Connect to MongoDB and retrieve data
async function retrieveAndSendData() {
    await client.connect();
    console.log('Connected successfully to server');
    const db = client.db(dbName);
    const collection = db.collection('movimenti_mano');
    const filter = {};
    const projection = {
        "Hands.Fingers.bones.NextJoint": 1,
        "Timestamp": 1, 
        "Hands.IsLeft": 1, 
        "Hands.PalmPosition": 1, 
        "Hands.Fingers.bones.Type": 1, 
        "Hands.Fingers.Type": 1
    };

    // Finding documents with the specified filter and projection
    const cursor = collection.find(filter, { projection: projection });
    const documents = await cursor.toArray();

    // Process and send the data to all connected WebSocket clients
    for(let it=0; it<documents.length; it++){
        let doc = documents[it];
        let hand = { 
            thumbBTIP:doc.Hands[0].Fingers[0].bones[0].NextJoint, thumbDIP:doc.Hands[0].Fingers[0].bones[1].NextJoint, thumbPIP:doc.Hands[0].Fingers[0].bones[2].NextJoint, thumbMCP:doc.Hands[0].Fingers[0].bones[3].NextJoint,  
            indexBTIP:doc.Hands[0].Fingers[1].bones[0].NextJoint, indexDIP:doc.Hands[0].Fingers[1].bones[1].NextJoint, indexPIP:doc.Hands[0].Fingers[1].bones[2].NextJoint, indexMCP:doc.Hands[0].Fingers[1].bones[3].NextJoint,
            middleBTIP:doc.Hands[0].Fingers[2].bones[0].NextJoint, middleDIP:doc.Hands[0].Fingers[2].bones[1].NextJoint, middlePIP:doc.Hands[0].Fingers[2].bones[2].NextJoint, middleMCP:doc.Hands[0].Fingers[2].bones[3].NextJoint,  
            fingerBTIP:doc.Hands[0].Fingers[3].bones[0].NextJoint, fingerDIP:doc.Hands[0].Fingers[3].bones[1].NextJoint, fingerPIP:doc.Hands[0].Fingers[3].bones[2].NextJoint, fingerMCP:doc.Hands[0].Fingers[3].bones[3].NextJoint,  
            pinkyBTIP:doc.Hands[0].Fingers[4].bones[0].NextJoint, pinkyDIP:doc.Hands[0].Fingers[4].bones[1].NextJoint, pinkyPIP:doc.Hands[0].Fingers[4].bones[2].NextJoint, pinkyMCP:doc.Hands[0].Fingers[4].bones[3].NextJoint,  
        };
        // Example processing - replace with actual logic
        // console.log(JSON.stringify(hand));
        broadcastHandData(hand);
        pause(documents[it+1]- documents[it]);

    
    /*
    documents.forEach(doc => {
        let hand = { 
            thumbBTIP:doc.Hands[0].Fingers[0].bones[0].NextJoint, thumbDIP:doc.Hands[0].Fingers[0].bones[1].NextJoint, thumbPIP:doc.Hands[0].Fingers[0].bones[2].NextJoint, thumbMCP:doc.Hands[0].Fingers[0].bones[3].NextJoint,  
            indexBTIP:doc.Hands[0].Fingers[1].bones[0].NextJoint, indexDIP:doc.Hands[0].Fingers[1].bones[1].NextJoint, indexPIP:doc.Hands[0].Fingers[1].bones[2].NextJoint, indexMCP:doc.Hands[0].Fingers[1].bones[3].NextJoint,
            middleBTIP:doc.Hands[0].Fingers[2].bones[0].NextJoint, middleDIP:doc.Hands[0].Fingers[2].bones[1].NextJoint, middlePIP:doc.Hands[0].Fingers[2].bones[2].NextJoint, middleMCP:doc.Hands[0].Fingers[2].bones[3].NextJoint,  
            fingerBTIP:doc.Hands[0].Fingers[3].bones[0].NextJoint, fingerDIP:doc.Hands[0].Fingers[3].bones[1].NextJoint, fingerPIP:doc.Hands[0].Fingers[3].bones[2].NextJoint, fingerMCP:doc.Hands[0].Fingers[3].bones[3].NextJoint,  
            pinkyBTIP:doc.Hands[0].Fingers[4].bones[0].NextJoint, pinkyDIP:doc.Hands[0].Fingers[4].bones[1].NextJoint, pinkyPIP:doc.Hands[0].Fingers[4].bones[2].NextJoint, pinkyMCP:doc.Hands[0].Fingers[4].bones[3].NextJoint,  
        };
        // Example processing - replace with actual logic
        // console.log(JSON.stringify(hand));
        broadcastHandData(hand);
        
    })
        */
}

    await client.close();
    console.log('MongoDB connection closed.');
}

// Wait for WebSocket connection before sending data
wss.on('connection', function connection(ws) {
    console.log('Client connected');
    ws.on('message', function incoming(message) {
        console.log('received: %s', message);
    });
    
    // Once a client connects, retrieve and send data
    retrieveAndSendData().catch(console.error);
});

