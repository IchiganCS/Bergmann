# Block

How are blocks handled in this game

## Thoughts

It seems useful for many purposes to encode every state of every block in a single number to process. `ints` are so fast, they seem reasonable which gives us 32 bits to work with.

Let's assume we add 500 blocks with some let's say 100 mutable with each 4 states. We can easily see, 800 is far less than what any integer can hold. We can be very generous with space, instead let's make a fast layout.


Block should implement a few helper functions to extract required information more easily. `BlockRenderer` in the client package handles all necessary methods to handle block rendering albeit it actually does no rendering. It's a helper class for `ChunkRenderer`.

The given values in specification are subject to change.

## Specification

Each block can be encoded, except its position, in a 32-bit integer. From lowest to highest:

- The first 12 bits are to specify the concrete type of the block. E.g. dirt, red flower
- 8 bits are reserved for the state. E.g. crops growth, power level for a power line, or simple rotation of a block.
  - The first 4 bits are for the rotation. Rotational possibilities of 16 roations seem to be enough
  - The last 4 bits for all other
- The twelve highest bits are reserved
