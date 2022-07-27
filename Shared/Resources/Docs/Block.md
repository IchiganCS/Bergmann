# Block

How are blocks handled in this game

## Thoughts

It seems useful for many purposes to encode every state of every block in a single number to process. `ints` are so fast, they seem reasonable which gives us 32 bits to work with.

Let's assume we add 500 blocks with some let's say 100 mutable with each 4 states. We can easily see, 800 is far less than what any integer can hold. We can be very generous with space, instead let's make a fast layout.


Block should implement a few helper functions to extract required information more easily. `BlockRenderer` in the client package handles all necessary methods to handle block rendering albeit it actually does no rendering. It's a helper class for `ChunkRenderer`.

Each info for a block is stored in `Resources/Jsons/Blocks.json`.

The given values in specification are subject to change.

### Specification

Each block can be encoded, except its position, in a 32-bit integer. From lowest to highest:

- The first 12 bits are to specify the concrete type of the block. E.g. dirt, red flower
- 8 bits are reserved for the state. E.g. crops growth, power level for a power line, or simple rotation of a block.
  - The first 4 bits are for the rotation. Rotational possibilities of 16 roations seem to be enough
  - The last 4 bits for all other
- The twelve highest bits are reserved


## Block info format

For multiple purposes, it is good to handle everything in blocks of 1x1x1. If that however, is not quite what should be shown, see connected blocks like double chests, then we define helper blocks. Those blocks redirect actions unto the master block for an "object". For this purpose, any block may define an array of helper blocks with their corresponding offset. The helper blocks should be referenced by their id and instantiated like any other block in the array. There needs to be some way to import a more complex mesh with its texture in the future. Generally though, every block shall declare their `Type` as either `Normal` or `Custom`. If their type is custom, it has to declare a `Mesh` file path. Otherwise, it has to declare textures for every side (`Left`, `Right` and so on).

Custom blocks may also be rotated or only be stuck to a wall etc. Those attributes are yet to be defined. Light sources also need to be added. See `Blocks.json` for some examples.

>sidenote: `ID` shall be consecutive, so that an array for faster access to block info can be constructed. 0 is reserved for an empty block, or air.

## Texture info format

Each client should have access to a `Texture.json` file where the layers specified in `Blocks.json` are instantiated. From the texture file, a texture stack is generated.

The property `Size` has to be specified and has to be true for every supplied image as the width and height. It is the suppliers responsibility to ensure that every texture has the required dimensions. Custom meshes can supply custom textures. The specification is yet to be done.

>sidenote: `Layer` shall be consecutive, so that an array for faster access to block info can be constructed.