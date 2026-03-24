# Bergmann

This is a game about blocks and mining. 


## Technology

This is built entirely with .NET 6.0 and OpenGL (the wrapper is [OpenTK](https://opentk.net/), a very nice wrapper for OpenGL). The server is built with SignalR. Currently, the versions are heavily outdated, since there was no development for four years.

## Current State

See a screenshot here:

![A screenshot of the game](Meta/Screenshot.png)

With VSync-disbled, thousands of frames are possible - which is not bad, but leaves much room for improvement. Frustum culling helps immensly, but the frames can drop below 2000 when looking at a lot of chunks. 

Each chunk is rendered by one vertex array object. Its contents are very efficient and only contain the possible visible faces, as can be seen when the `wireframe` setting is turned on.

![Wireframe enabled](Meta/Wireframe.png)


Multiplayer is possible: each client can concurrently place and delete blocks and send chat messages.