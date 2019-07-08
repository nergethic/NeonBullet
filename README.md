*Neon Walls*

Project focused on random dungeon generation
Room patterns are located in Resources/Textures folder - .bmp files that describe the main shape of generated rooms.
Each generation of the pattern applies random variations to the main shape.

shape pattern format:
white pixels - empty place
black pixels - floor tile
blue pixel - this tile has probability to become an entrance to the room
green pixel - probability of room exit  

Grass generation uses Poisson sampling for less random looking distribution.  

future todo:
(almost finished) - processing wall chains (multiple tiles consisting of walls facing the same direction) in order to make possible generating wall variations longer than 1 tile.

- generation of multiple room patterns connected with corridors