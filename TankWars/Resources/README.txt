Its_Not_A_Bug_Its_A_Feature TankWars Project

*********Tank Wars Server************

*Description and Design*

The TankWars server is the implementation of working server code to support the TankWars client. Much of the work was rebuilding 
the code provided for PS8 by referencing its functionality. The code is initialized in the server class, which holds all the main
functions for referencing the settings, as well as creating a ServerController and starting the server. The settings class contains
an XML reader that imports data from a formatting-compatible Settings.xml file. This class also contains world variables referenced
by other world objects. 

The ServerController is responsible for establishing the server update thread, while also opening a listener thread that opens and 
establishes sockets with clients as they connect. The server receives the names of clients, then provides a playerID andworld-size 
information to the client, followed by all wall objects necessary to display. That socket then prepares to receive all client-provided
control commands, deserializes the Json, and relays them to the world command-controller data structure. Any clients that report 
errors in their socket state are removed from the server and tank-death/dc instructions are provided to the other clients.

The world itself contains all the objects present in the game, including data structures to manage them. Addtional to the client's 
world data structures are stacks used to represent dead beams, tanks, powerups, and projectiles. These stacks allow the objects to be
sent to clients and still removed from the server's dictionary as they are encountered during an update cycle. The world also contains 
methods to manage each object conditional to both the control commands provided by the client in addition to the world physics governing
tank and projectile motion, collision mechanics, and collision consequences. We chose to add helper methods to the world to allow
for random spawn locations. Controll commands were evaluated in a private method where tanks velocities and positions were projected 
againset wall locations for collision detection. Most collision detection methods were placed into their respective classes. Wall 
collisions were overloaded to allow for both location and object inputs. This enabled us to reuse wall collision methods for
tanks, prjoectiles, and powerups. Similar code was used for tank-projectile, -beam, and -powerup collisions, though with the 
logic that projectiles and beams and projectiles could not collide with their own tank.


*Issues and Challenges*

Initial work on the server project went smoothly, though there were some late-stage hiccups. There was some difficulty where tanks that
had died were able to move again without waiting for the respawn timer. We discovered this was due to tank-projectile mechanics allowing
for tanks to continue to be shot while at an HP of zero, resulting in negative HP and breaking some of the conditionals used to process the 
respawn timer. While we built in logic to prevent a tank or powerup from spawning within a wall location, some tanks still seem to get stuck 
in walls when running multiple AI clients in test cases. We experimented with adding different buffers to the collision check, but none seemed 
to work. However, later evaluation showed that tank collisions were being captured correctly, but subsequent collisions weren't being checked 
against prior evaluated walls. New code was introduced to rectify this, and now there are no observedtank or powerup spawning collisions with 
walls.


*Work Log*

2021-12-06
Updated collision mechanics for interactions between tanks, world-edges, projectiles, and beams.

2021-12-07
- Adjusted HP for hit tanks, add score for enemy tank deaths
- Added Tank respawn + timer
- Handled client disconnect / stack overflow issue
- Added Read/write XML file code

2021-12-08
- Implemented increment tank score for kills
- Handled client sending bad data (by ignoring it)
- Handled clients disconnecting from server

2021-12-09
- Fixed tank not visibly respawning (was issue with projectile collisions with dead tanks, reducing their HP below 0)
- Extra feature: Hot streak (Tank speed boost)
   When a tank reaches a score at a multiple of 5, the tank's speed increases by 100% for 1 minute. This ability can be extended 
   if another multiple of 5 score is reached.


*********Tank Wars Server************

Dune (Tank) Wars
By Matt Streicher and Jim Morgan
University of Utah
CS3500

Dune Wars is a multiplayer game using C# and .NET Framework. This client allows the user to connect to an address
and play the game against other users. This client references a previous assignment's network library in order to connect
to the server; and send and receive information. The solution also follows MVC rules. The model here is passive, and
strictly stores the state of the world. The world is stored in Dictionaries to hold each object in the world, and uses JSON
properties to serialize objects and send to the server.

The view is the users visual representation of the current world. It allows the user to issue move and fire commands to the 
Game Controller. The Drawing Panel receives the events from the Game Controller and draws the objects from the world. The Game
Controller utilizes a static class to contain player information regarding their tank avatars' color, and initializes images for 
those colors. Colors are selected based on the player's ID, rotating through eight different colors. This static method, being 
shared, utilizes a static generic lock object to prevent race conditions. Tank death animations and the beam attack animation 
are both handled using class object wrappers including frame information and providing parameters for a moving source rectangle 
on a reference sprite sheet. The OnPaint method is locked to the World object, also to prevent race conditions. Walls are drawn 
using a TextureBrush object and the server-provided wall endpoints. All game image assetsare original and unique to the Dune 
theme selected for the game client.

The controller initiates and manages the connection between client and server using the PS7 network library. Once the initial 
handshake is complete, it continuously receives information about the current attributes of each object in the world from the 
server as JSON objects. It also sends control command requests to the server as JSON objects. The Game Controller updates objects 
in the world and publishes events to the view. The students took a lot of inspiration from previous labs, help sessions, and in-class 
demos.

2021-11-13
Began coding and design phase. Inital idea for TankWars are as follows:

2021-11-15
We started by implementing the Model classes. 

2021-11-24
Other then the final touch ups. The client is a minimal viable product.

2021-11-27
Added the Dune theme background as well as the death animation. The client was running very slow when the new background
was added. We realized the size of the background image was 16 times larger then the provided background image, so we scaled
it down and now the game runs smoothly again. The client is 99% complete. Will likely be submitting on Monday.


